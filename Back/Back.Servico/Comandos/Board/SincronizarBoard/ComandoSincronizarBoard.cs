using Back.Dominio;
using Back.Dominio.DTO.Board;
using Back.Dominio.Enum;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Comandos.Board._Helpers;
using ElectronNET.API;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Board.SincronizarBoard
{
    class ComandoSincronizarBoard : IRequestHandler<ParametroSincronizarBoard, ResultadoSincronizarBoard>
    {
        private readonly IRepositorioComando<Sincronizar> _repositorioComandoSincronizar;
        private readonly IRepositorioComando<SincronizarItem> _repositorioComandoSincronizarItem;
        private readonly IRepositorioConsulta<Sincronizar> _repositorioConsultaSincronizar;
        private readonly IRepositorioConsulta<Configuracao> _repositorioConsultaConfiguracao;
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;
        private readonly IRepositorioConsulta<Dominio.Models.Azure> _repositorioConsultaAzure;
        private readonly ILogger<ComandoSincronizarBoard> _logger;
        private readonly IConfiguration _configuration;
        private List<WorkItem> _itensSemParente;
        private VssConnection _vssConnectionPrincipal;
        private VssConnection _vssConnectionSecundaria;
        private Configuracao _configuracao;
        private bool _usandoElectron;
        private List<SincronizarItem> _itensEnviarEmail = new List<SincronizarItem>();

        public ComandoSincronizarBoard(
            IRepositorioComando<Sincronizar> repositorioComandoSincronizar,
            IRepositorioComando<SincronizarItem> repositorioComandoSincronizarItem,
            IRepositorioConsulta<Sincronizar> repositorioConsultaSincronizar,
            IRepositorioConsulta<Configuracao> repositorioConsultaConfiguracao,
            IRepositorioConsulta<Conta> repositorioConsultaConta,
            IRepositorioConsulta<Dominio.Models.Azure> repositorioConsultaAzure,
            ILogger<ComandoSincronizarBoard> logger,
            IConfiguration configuration)
        {
            _repositorioComandoSincronizar = repositorioComandoSincronizar;
            _repositorioComandoSincronizarItem = repositorioComandoSincronizarItem;
            _repositorioConsultaSincronizar = repositorioConsultaSincronizar;
            _repositorioConsultaConfiguracao = repositorioConsultaConfiguracao;
            _repositorioConsultaConta = repositorioConsultaConta;
            _repositorioConsultaAzure = repositorioConsultaAzure;
            _logger = logger;
            _configuration = configuration;
            _usandoElectron = _configuration.GetValue<bool>("Electron.Ativo");
            _itensSemParente = new List<WorkItem>();
        }


        public async Task<ResultadoSincronizarBoard> Handle(ParametroSincronizarBoard request, CancellationToken cancellationToken)
        {
            //Envia a notificação para o front
            var window = Electron.WindowManager?.BrowserWindows?.First();

            var sincronizar = new Sincronizar { DataInicio = DateTime.Now, Status = (int)EStatusSincronizar.PROCESSANDO };
            try
            {
                //Contas Azure
                var azure = await _repositorioConsultaAzure.Query().ToListAsync();

                if (azure.Count == 0)
                    throw new Exception($"Nenhuma conta azure encontrada na base de dados");

                //Busca as contas
                var contas = await _repositorioConsultaConta.Query().ToListAsync();

                #region REGISTRANDO_TABELA
                await _repositorioComandoSincronizar.Insert(sincronizar);
                await _repositorioComandoSincronizar.SaveChangesAsync();
                if (_usandoElectron)
                    Electron.IpcMain.Send(window, Constantes.NOTIFICACAO_SYNC_INICIO, Constantes.NOTIFICACAO_SYNC_INICIO);
                #endregion

                _configuracao = await _repositorioConsultaConfiguracao.Query().FirstOrDefaultAsync();

                //Conta principal
                var contaPrincipal = azure.FirstOrDefault(e => e.Principal);

                //Conta secundaria
                var contaSecundaria = azure.FirstOrDefault(e => !e.Principal);

                //Conectar na azure
                _vssConnectionPrincipal = new VssConnection(new Uri(contaPrincipal.UrlCorporacao), new VssBasicCredential(string.Empty, contaPrincipal.Token));
                _vssConnectionSecundaria = new VssConnection(new Uri(contaSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, contaSecundaria.Token));

                foreach (var item in contas)
                {
                    //Busca as task para sincronizar
                    var itensSincronizar = await RecuperaItensSincronizar(item, contaPrincipal);
                    _logger.LogInformation($"Qtd itens a ser copiados: {itensSincronizar.Count() + itensSincronizar.Sum(c => c.Itens.Count())}");

                    //Migra as task
                    await EnviarTaskDestino(itensSincronizar);
                }

                #region ATUALIZANDO_TABELA
                await AtualizarUltimoSicronizar(EStatusSincronizar.CONCLUIDO);
                #endregion

                //Atualizar historias fechadas
                await AtualizarStatusHistorias();

                if (_usandoElectron)
                    Electron.IpcMain.Send(window, Constantes.NOTIFICACAO_SYNC_FIM, Constantes.NOTIFICACAO_SYNC_FIM);

                return new ResultadoSincronizarBoard
                {
                    Sucesso = true
                };
            }
            catch (Exception ex)
            {
                #region REGISTRANDO_TABELA
                await AtualizarUltimoSicronizar(EStatusSincronizar.ERRO);
                #endregion

                // Electron.IpcMain.Send(window, Constantes.NOTIFICACAO_SYNC_FIM, Constantes.NOTIFICACAO_SYNC_FIM);

                return new ResultadoSincronizarBoard
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }

        private async Task<List<ItensSicronizarDTO>> RecuperaItensSincronizar(Conta conta, Dominio.Models.Azure azure)
        {
            //Resultado
            var historias = new List<ItensSicronizarDTO>();
            var witClient = _vssConnectionPrincipal.GetClient<WorkItemTrackingHttpClient>();
            //Filtro data
            var data = DateTime.Now.AddDays(-_configuracao.Dia).Date.ToString("yyyy-MM-dd");

            // Query para buscar os itens do usuário que está assinadas ou ja foi assinadas a ele
            Wiql query = new Wiql()
            {
                Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                 WHERE [System.AssignedTo] EVER '{conta.EmailDe}'
                                 AND [System.TeamProject] = '{azure.ProjetoNome}'
                                 AND System.ChangedDate >= '{data} 00:00:00'
                                 AND [System.WorkItemType] IN ('{Constantes.TIPO_ITEM_TASK}',
                                                               '{Constantes.TIPO_ITEM_BUG}',
                                                               '{Constantes.TIPO_ITEM_ENABLER}',
                                                               '{Constantes.TIPO_ITEM_DEBITO}',
                                                               '{Constantes.TIPO_ITEM_SOLICITACAO}',
                                                               '{Constantes.TIPO_ITEM_HISTORIA}')"
            };
            _logger.LogInformation($"RecuperaItensSincronizar Query: {query.Query}");

            Guid projetoId = Guid.Parse(azure.ProjetoId);

            //Roda a query
            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query, projetoId);
            IEnumerable<int> taskIds = queryResult.WorkItems.Select(wi => wi.Id);

            // Recupera as histórias, o item em si
            foreach (int taskId in taskIds)
            {
                var migrar = new ItensSicronizarDTO();

                //Busca a task
                WorkItem task = await witClient.GetWorkItemAsync(taskId, expand: WorkItemExpand.Relations);
                var tipoTask = task.Fields["System.WorkItemType"].ToString();

                //Pega apenas itens de trabalho
                var historiaId = task.Fields.FirstOrDefault(c => c.Key.Contains("System.Parent")).Value;

                if (tipoTask == Constantes.TIPO_ITEM_SOLICITACAO || tipoTask == Constantes.TIPO_ITEM_ENABLER || tipoTask == Constantes.TIPO_ITEM_HISTORIA || tipoTask == Constantes.TIPO_ITEM_DEBITO)
                {
                    _itensSemParente.Add(task);

                    continue;
                }

                if (historiaId is null)
                    continue;

                // Recupera o pai (historia) da task
                var idHistoria = Convert.ToInt32(historiaId.ToString());
                WorkItem historiaItem = await witClient.GetWorkItemAsync(idHistoria);
                _logger.LogInformation($"RecuperaItensSincronizar historia: {historiaItem.Id}");

                //Verifica se ja existe a historia no array para migrar
                var hasHistoria = historias.FirstOrDefault(c => c.Historia.Id == historiaItem.Id);
                if (hasHistoria != null)
                {
                    migrar.Historia = null;
                    historiaId = historiaItem.Id.Value;
                }
                else
                    migrar.Historia = historiaItem;

                _logger.LogInformation($"RecuperaItensSincronizar task: {task.Id}");

                //Verifica se ja existe a task no array
                var hasTask = migrar.Itens.FirstOrDefault(c => c.Id == task.Id);
                if (hasTask is null)
                    migrar.Itens.Add(task);

                if (migrar.Historia is null)
                {
                    var historia = historias.FirstOrDefault(c => c.Historia.Id == idHistoria);
                    historia.Itens.AddRange(migrar.Itens);
                }
                else
                {
                    historias.Add(migrar);
                }
            }

            return historias;
        }

        private async Task EnviarTaskDestino(List<ItensSicronizarDTO> historias)
        {
            //Faz conexão com o azure
            WorkItemTrackingHttpClient witClient = _vssConnectionSecundaria.GetClient<WorkItemTrackingHttpClient>();

            foreach (var historia in historias)
            {
                var resultado = await CadastrarItem(_vssConnectionSecundaria, historia.Historia, witClient, 0);
                _logger.LogInformation($"Resultado historia: {resultado.Id}");

                //Se não cadastrou/atualizou  a historia, vamos pular para o proximo
                if (resultado is null)
                    continue;

                //Se criou/atualizou, vamos cadastrar/atualizar os filhos
                foreach (var task in historia.Itens)
                    await CadastrarItem(_vssConnectionSecundaria, task, witClient, resultado.Id.Value);

            }

            //cadastra solicitação/enable
            if (_itensSemParente.Count > 0)
            {
                var historiasId = historias.Select(c => c.Historia.Id).ToList();
                _itensSemParente = _itensSemParente.Where(c => !historiasId.Contains(c.Id)).ToList();
                foreach (var solicitacao in _itensSemParente)
                {
                    var resultado = await CadastrarItem(_vssConnectionSecundaria, solicitacao, witClient, 0);
                    _logger.LogInformation($"Resultado _itensSemParente: {resultado?.Id}");
                }
            }
        }

        private async Task<WorkItem> CadastrarItem(VssConnection connection, WorkItem item, WorkItemTrackingHttpClient witClient, int historiaId)
        {
            WorkItem resultado = new WorkItem();
            var itemTask = await VerificarSeItemExisteNoDestino(connection, item.Id.Value);
            var tipoTask = item.Fields["System.WorkItemType"].ToString();
            var status = item.Fields["System.State"].ToString();
            var operation = Operation.Replace;

            try
            {
                //Se não existir, vamos criar
                /*if (itemTask is null)
                {
                    _logger.LogInformation($"Fluxo cadastrar {tipoTask} - {item.Id}");
                    var ntipoTask = SincronizarHelper.RetornarTipoItem(tipoTask);
                    var novoItemTask = SincronizarHelper.TratarObjeto(item, historiaId, _contaSecundaria, _areaId);
                    resultado = await witClient.CreateWorkItemAsync(novoItemTask, Guid.Parse(_contaSecundaria.ProjetoId), ntipoTask);
                    //Não pode criar um item ja com status ativo, então cria como novo e logo atualiza
                    if (status != Constantes.STATUS_NOVO)
                    {
                        var patchDocument = new JsonPatchDocument();
                        SincronizarHelper.AdicionarOperacao(patchDocument, Operation.Replace, "/fields/System.State", SincronizarHelper.BuscarStatusItem(status));
                        await witClient.UpdateWorkItemAsync(patchDocument, resultado.Id.Value);
                    }
                    operation = Operation.Add;
                    _itensEnviarEmail.Add(new SincronizarItem(tipoTask, item.Id, resultado.Id, "Criado"));
                }
                else
                {
                    _logger.LogInformation($"Fluxo atualizar {tipoTask} - {item.Id}");

                    item.Fields["System.AssignedTo"] = itemTask.Fields.FirstOrDefault(c => c.Key == "System.AssignedTo").Value ?? _contaSecundaria.NomeUsuario;
                    var atualizarTask = SincronizarHelper.TratarObjeto(item, historiaId, _contaSecundaria, _areaId, Operation.Replace);
                    resultado = await witClient.UpdateWorkItemAsync(atualizarTask, itemTask.Id.Value);
                    _itensEnviarEmail.Add(new SincronizarItem(tipoTask, item.Id, resultado.Id, "Atualizado"));
                }

                //Se for bug, vamos atualizar os campos customizaveis
                if (tipoTask == Constantes.TIPO_ITEM_BUG)
                    await CamposCustomizaveis(resultado, witClient, operation, item);

                _logger.LogInformation($"Resultado: {resultado.Id}, tipo: {tipoTask}");*/
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Resultado erro: {ex.Message}");
                return null;
            }
        }

        private async Task CamposCustomizaveis(WorkItem item, WorkItemTrackingHttpClient witClient, Operation operacao, WorkItem itemOriginal)
        {
            try
            {
                JsonPatchDocument patchDocument = new JsonPatchDocument();

                /*//Se tiver cliente cadastrado
                if (!string.IsNullOrEmpty(_configuracao.Cliente))
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.Cliente", _configuracao.Cliente);

                //Se existe motivo do bug
                var motivo = itemOriginal.Fields.FirstOrDefault(c => c.Key == "Custom.CausaRaiz").Value?.ToString();
                if (!string.IsNullOrEmpty(motivo))
                {
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.Motivodademanda", SincronizarHelper.BuscarMotivo(motivo));

                    //Dev responsavel pelo bug                    
                    var devResponsavel = item.Fields.FirstOrDefault(c => c.Key == "Custom.DevResponsavel").Value?.ToString();
                    patchDocument.Add(new JsonPatchOperation()
                    {
                        Operation = operacao,
                        Path = "/fields/Custom.DevResponsavel",
                        Value = operacao == Operation.Add ? _contaSecundaria.NomeUsuario : string.IsNullOrEmpty(devResponsavel) ? ((IdentityRef)item.Fields["Custom.DevResponsavel"]).UniqueName : _contaSecundaria.NomeUsuario,
                    });
                }

                await witClient.UpdateWorkItemAsync(patchDocument, item.Id.Value);*/
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro campo customizavel: {ex.Message}, BUG: {item.Id.Value}");
            }
        }

        private async Task<WorkItem> VerificarSeItemExisteNoDestino(VssConnection connection, int id)
        {
            Wiql query = new Wiql() { Query = $@"SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.Title] CONTAINS WORDS '{id}:'" };

            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query);

            var task = queryResult.WorkItems.FirstOrDefault();

            if (task is null)
                return null;

            return await witClient.GetWorkItemAsync(task.Id, expand: WorkItemExpand.Relations);
        }

        private async Task AtualizarUltimoSicronizar(EStatusSincronizar eStatus)
        {
            try
            {
                var item = await _repositorioConsultaSincronizar.Query().OrderByDescending(c => c.Id).FirstOrDefaultAsync();
                item.DataFim = DateTime.Now;
                item.Status = (int)eStatus;
                _repositorioComandoSincronizar.Update(item);
                await _repositorioComandoSincronizar.SaveChangesAsync();

                await AtualizarSicronizarItens(item.Id);
            }
            catch (Exception)
            { }
        }

        private async Task AtualizarSicronizarItens(int sincronizarId)
        {
            try
            {
                var itens = _itensEnviarEmail.Select(e => new SincronizarItem
                {
                    Destino = e.Destino,
                    Erro = e.Erro,
                    Origem = e.Origem,
                    Status = e.Status,
                    Tipo = e.Tipo,
                    SincronizarId = sincronizarId
                });

                _repositorioComandoSincronizarItem.UpdateRange(itens);
                await _repositorioComandoSincronizarItem.SaveChangesAsync();
            }
            catch (Exception)
            { }
        }

        private async Task AtualizarStatusHistorias()
        {
            //Faz conexão com o azure
            /*VssConnection connection1 = new VssConnection(new Uri(_contaPrincipal.UrlCorporacao), new VssBasicCredential(string.Empty, _contaPrincipal.Token));
            VssConnection conectDestino = new VssConnection(new Uri(_contaSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, _contaSecundaria.Token));

            //Filtro data
            int dias = Convert.ToInt32(_configuration.GetValue<string>("DiasHistoriaFechada"));
            string data = DateTime.Now.AddDays(-dias).Date.ToString("yyyy-MM-dd");

            WorkItemTrackingHttpClient witClient = connection1.GetClient<WorkItemTrackingHttpClient>();
            WorkItemTrackingHttpClient witClientDestino = conectDestino.GetClient<WorkItemTrackingHttpClient>();

            // Query para buscar os historias fechadas
            Wiql query = new Wiql()
            {
                Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                WHERE [System.AssignedTo] EVER @Me
                                AND [System.TeamProject] = '{_contaPrincipal.ProjetoNome}'
                                AND System.ChangedDate >= '{data} 00:00:00'
                                AND System.State = 'Em produção'
                                AND [System.WorkItemType] = '{Constantes.TIPO_ITEM_HISTORIA}'"
            };
            _logger.LogInformation($"Recupera historias fechadas Query: {query.Query}");

            Guid projetoId = Guid.Parse(_contaPrincipal.ProjetoId);

            //Roda a query
            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query, projetoId);
            IEnumerable<int> taskIds = queryResult.WorkItems.Select(wi => wi.Id);
            _logger.LogInformation($"Historias fechadas: {taskIds.Count()}");

            // Recupera as histórias, o item em si
            foreach (int taskId in taskIds)
            {
                //Busca a historia
                WorkItem task = await witClient.GetWorkItemAsync(taskId);

                //Busca a historia que existe no board de destino
                var historia = await VerificarSeItemExisteNoDestino(conectDestino, taskId);

                if (historia is null)
                    continue;

                var status = SincronizarHelper.BuscarStatusItem(task.Fields["System.State"].ToString());

                if (status != historia.Fields["System.State"].ToString())
                {
                    _logger.LogInformation($"Historia fechada: {historia.Id.Value}");
                    var patchDocument = new JsonPatchDocument();
                    SincronizarHelper.AdicionarOperacao(patchDocument, Operation.Replace, "/fields/System.State", SincronizarHelper.BuscarStatusItem(status));
                    await witClientDestino.UpdateWorkItemAsync(patchDocument, historia.Id.Value);
                }
            }*/
        }

        private string CorpoEmail()
        {
            string texto = @"<table style='width: 100%'>
                              <tr>
                                <th>Tipo</th>
                                <th>Origem</th>
                                <th>Destino</th>
                                <th>Status</th>
                              </tr>";

            foreach (var item in _itensEnviarEmail)
            {
                texto += $@"<tr>
                            <td>{item.Tipo}</td>
                            <td>{item.Origem}</td>
                            <td>{item.Destino}</td>
                            <td>{item.Status}</td>
                          </tr>";
            }

            texto += "</table>";
            return texto;
        }
    }
}