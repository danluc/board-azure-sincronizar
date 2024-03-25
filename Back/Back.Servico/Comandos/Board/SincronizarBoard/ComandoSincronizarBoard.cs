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
        private Dominio.Models.Azure _secundariaAzure;
        private Configuracao _configuracao;
        private readonly bool _notificarPorEmail;
        private List<SincronizarItem> _itensCadastrarLocal = new List<SincronizarItem>();
        private readonly string[] _tiposSemParente = new[] { Constantes.TIPO_ITEM_SOLICITACAO, Constantes.TIPO_ITEM_ENABLER, Constantes.TIPO_ITEM_HISTORIA, Constantes.TIPO_ITEM_DEBITO, Constantes.TIPO_ITEM_STORY, Constantes.TIPO_ITEM_STORY_ENABLER, Constantes.TIPO_ITEM_INCIDENTE, Constantes.TIPO_ITEM_SPIKE };


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
            _notificarPorEmail = _configuration.GetValue<bool>("NotificarPorEmail");
            _itensSemParente = new List<WorkItem>();
        }


        public async Task<ResultadoSincronizarBoard> Handle(ParametroSincronizarBoard request, CancellationToken cancellationToken)
        {
            //Envia a notificação para o front
            BrowserWindow window = Electron.WindowManager.BrowserWindows.First();

            var sincronizar = new Sincronizar { DataInicio = DateTime.Now, Status = (int)EStatusSincronizar.PROCESSANDO };
            try
            {
                //Contas Azure
                var azure = await _repositorioConsultaAzure.Query().ToListAsync();

                if (azure.Count == 0)
                    throw new Exception($"Nenhuma conta azure encontrada na base de dados");

                //Busca as contas
                var contas = await _repositorioConsultaConta.Query(filter: e => e.Ativo).ToListAsync();

                #region REGISTRANDO_TABELA
                await _repositorioComandoSincronizar.Insert(sincronizar);
                await _repositorioComandoSincronizar.SaveChangesAsync();
                Electron.IpcMain.Send(window, Constantes.NOTIFICACAO_SYNC_INICIO, Constantes.NOTIFICACAO_SYNC_INICIO);
                #endregion

                _configuracao = await _repositorioConsultaConfiguracao.Query().FirstOrDefaultAsync();

                //Conta principal
                var contaPrincipal = azure.FirstOrDefault(e => e.Principal);

                //Conta secundaria
                _secundariaAzure = azure.FirstOrDefault(e => !e.Principal);

                //Conectar na azure
                _vssConnectionPrincipal = new VssConnection(new Uri(contaPrincipal.UrlCorporacao), new VssBasicCredential(string.Empty, contaPrincipal.Token));
                _vssConnectionSecundaria = new VssConnection(new Uri(_secundariaAzure.UrlCorporacao), new VssBasicCredential(string.Empty, _secundariaAzure.Token));

                foreach (var item in contas)
                {
                    _itensSemParente = new List<WorkItem>();

                    //Busca as task para sincronizar
                    var itensSincronizar = await RecuperaItensSincronizar(item);

                    _logger.LogInformation($"Qtd itens a ser copiados: {itensSincronizar.Count() + itensSincronizar.Sum(c => c.Itens.Count()) + _itensSemParente.Count()}");

                    //Migra as task
                    await EnviarTaskDestino(itensSincronizar, item);

                    //Atualizar bug ja criados
                    await AtualizarBugVS(item);
                }

                #region ATUALIZANDO_TABELA
                await AtualizarUltimoSicronizar(EStatusSincronizar.CONCLUIDO);
                #endregion


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

                Electron.IpcMain.Send(window, Constantes.NOTIFICACAO_SYNC_FIM, Constantes.NOTIFICACAO_SYNC_FIM);

                return new ResultadoSincronizarBoard
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }

        private async Task<List<ItensSicronizarDTO>> RecuperaItensSincronizar(Conta conta)
        {
            //Resultado
            var historias = new List<ItensSicronizarDTO>();
            var witClient = _vssConnectionPrincipal.GetClient<WorkItemTrackingHttpClient>();
            //Filtro data
            var data = DateTime.Now.AddDays(-_configuracao.Dia).Date.ToString("yyyy-MM-dd");
            var itensBuscar = string.Join(",", _configuration.GetSection("ItensBuscar:Itens").Get<List<string>>().Select(e => "'" + e + "'"));

            // Query para buscar os itens do usuário que está assinadas ou ja foi assinadas a ele
            Wiql query = new Wiql()
            {
                Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                 WHERE [System.AssignedTo] EVER '{conta.EmailDe}'
                                 AND System.ChangedDate >= '{data} 00:00:00'
                                 AND [System.WorkItemType] IN ({itensBuscar})"
            };
            _logger.LogInformation($"RecuperaItensSincronizar Query: {query.Query}");

            //Roda a query
            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query);
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
                if (_tiposSemParente.Contains(tipoTask))
                {
                    _itensSemParente.Add(task);

                    continue;
                }

                if (historiaId is null)
                {
                    //Se é um bug ou task sem parente(sem historia) vamos cadastrar como incidente
                    if (tipoTask == Constantes.TIPO_ITEM_TASK || tipoTask == Constantes.TIPO_ITEM_BUG)
                    {
                        task.Fields["System.WorkItemType"] = Constantes.TIPO_ITEM_INCIDENTE;
                        _itensSemParente.Add(task);
                    }

                    continue;
                }

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

        private async Task EnviarTaskDestino(List<ItensSicronizarDTO> historias, Conta conta)
        {
            //Faz conexão com o azure
            WorkItemTrackingHttpClient witClient = _vssConnectionSecundaria.GetClient<WorkItemTrackingHttpClient>();

            foreach (var historia in historias)
            {
                var resultado = await CadastrarItem(_vssConnectionSecundaria, historia.Historia, witClient, conta, 0);
                
                //Se não cadastrou/atualizou  a historia, vamos pular para o proximo
                if (resultado is null)
                    continue;

                _logger.LogInformation($"Resultado historia: {resultado?.Id}");

                //Se criou/atualizou, vamos cadastrar/atualizar os filhos
                foreach (var task in historia.Itens)
                    await CadastrarItem(_vssConnectionSecundaria, task, witClient, conta, resultado.Id.Value);

            }

            //cadastra solicitação/enable
            if (_itensSemParente.Count > 0)
            {
                var historiasId = historias.Select(c => c.Historia.Id).ToList();
                _itensSemParente = _itensSemParente.Where(c => !historiasId.Contains(c.Id)).ToList();
                foreach (var solicitacao in _itensSemParente)
                {
                    var resultado = await CadastrarItem(_vssConnectionSecundaria, solicitacao, witClient, conta, 0);
                    _logger.LogInformation($"Resultado _itensSemParente: {resultado?.Id}");
                }
            }
        }

        private async Task<WorkItem> CadastrarItem(VssConnection connection, WorkItem item, WorkItemTrackingHttpClient witClient, Conta conta, int historiaId)
        {
            WorkItem resultado = new WorkItem();
            var itemTask = await VerificarSeItemExisteNoDestino(connection, item.Id.Value);
            var tipoTask = item.Fields["System.WorkItemType"].ToString();
            var status = item.Fields["System.State"].ToString();
            var operation = Operation.Replace;

            try
            {
                //Se não existir, vamos criar
                if (itemTask is null)
                {
                    _logger.LogInformation($"Fluxo cadastrar {tipoTask} - {item.Id}");
                    var ntipoTask = SincronizarHelper.RetornarTipoItem(tipoTask);
                    var novoItemTask = TratarObjeto(item, historiaId, conta);
                    resultado = await witClient.CreateWorkItemAsync(novoItemTask, Guid.Parse(_secundariaAzure.ProjetoId), ntipoTask, suppressNotifications: _notificarPorEmail);

                    //Não pode criar um item ja com status ativo, então cria como novo e logo atualiza
                    if (status != Constantes.STATUS_NOVO)
                    {
                        var patchDocument = new JsonPatchDocument();
                        SincronizarHelper.AdicionarOperacao(patchDocument, Operation.Replace, "/fields/System.State", SincronizarHelper.BuscarStatusItem(status, tipoTask, _configuration));
                        await witClient.UpdateWorkItemAsync(patchDocument, resultado.Id.Value, suppressNotifications: _notificarPorEmail);
                    }
                    operation = Operation.Add;
                    _itensCadastrarLocal.Add(new SincronizarItem(tipoTask, item.Id, resultado.Id, "Criado"));
                }
                else
                {
                    _logger.LogInformation($"Fluxo atualizar {tipoTask} - {item.Id}");

                    item.Fields["System.AssignedTo"] = itemTask.Fields.FirstOrDefault(c => c.Key == "System.AssignedTo").Value ?? conta.EmailPara;
                    var atualizarTask = TratarObjeto(item, historiaId, conta, Operation.Replace);
                    resultado = await witClient.UpdateWorkItemAsync(atualizarTask, itemTask.Id.Value, suppressNotifications: _notificarPorEmail);
                    _itensCadastrarLocal.Add(new SincronizarItem(tipoTask, item.Id, resultado.Id, "Atualizado"));
                }

                //Se for bug, vamos atualizar os campos customizaveis
                if (tipoTask == Constantes.TIPO_ITEM_BUG)
                    await CamposCustomizaveis(resultado, witClient, operation, item, conta);

                _logger.LogInformation($"Resultado: {resultado?.Id}, tipo: {tipoTask}");
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Resultado erro: {ex.Message}");
                return null;
            }
        }

        private async Task CamposCustomizaveis(WorkItem item, WorkItemTrackingHttpClient witClient, Operation operacao, WorkItem itemOriginal, Conta conta)
        {
            try
            {
                JsonPatchDocument patchDocument = new JsonPatchDocument();

                //Se tiver cliente cadastrado
                if (!string.IsNullOrEmpty(conta.Cliente))
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.Cliente", conta.Cliente);

                //Se existe motivo do bug
                var motivo = itemOriginal.Fields.FirstOrDefault(c => c.Key == "Custom.CausaRaiz").Value?.ToString();
                if (!string.IsNullOrEmpty(motivo))
                {
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.Motivodademanda", SincronizarHelper.BuscarMotivo(motivo, _configuration));

                    //é Bug
                    var ehBug = SincronizarHelper.BuscarMotivoEhBug(motivo, _configuration);
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.90c3dfca-b144-449b-a280-0371f1834781", ehBug);
                }

                //Dev responsavel pelo bug                    
                var devResponsavel = item.Fields.FirstOrDefault(c => c.Key == "Custom.DevResponsavel").Value?.ToString();
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/Custom.DevResponsavel",
                    Value = operacao == Operation.Add ? conta.EmailPara : !string.IsNullOrEmpty(devResponsavel) ? ((IdentityRef)item.Fields["Custom.DevResponsavel"]).UniqueName : conta.EmailPara,
                });

                await witClient.UpdateWorkItemAsync(patchDocument, item.Id.Value, suppressNotifications: _notificarPorEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro campo customizavel: {ex.Message}, BUG: {item.Id}");
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
                var itens = _itensCadastrarLocal.Select(e => new SincronizarItem
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

        private JsonPatchDocument TratarObjeto(WorkItem item, int historiaId, Conta _conta, Operation operacao = Operation.Add)
        {
            var tipoTask = item.Fields["System.WorkItemType"].ToString();
            var patchDocument = new JsonPatchDocument();
            string descricaoCompleta = "";

            SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/System.Title", $"{item.Id}: {item.Fields["System.Title"]}");

            //coloca a sprint so no cadastro
            if (operacao == Operation.Add)
            {
                SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/System.AreaId", _conta.AreaId.ToString());
                SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/System.IterationPath", _conta.Sprint.Replace("Iteration\\", ""));
            }

            var estado = operacao == Operation.Add ? Constantes.STATUS_NOVO : SincronizarHelper.BuscarStatusItem(item.Fields["System.State"].ToString(), tipoTask, _configuration);
            SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/System.State", estado);

            var assignedTo = operacao == Operation.Add ? _conta.EmailPara : SincronizarHelper.ObterAssignedTo(item.Fields["System.AssignedTo"]);
            SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/System.AssignedTo", assignedTo);

            var descricao = item.Fields.FirstOrDefault(c => c.Key == "System.Description").Value;
            if (descricao != null)
                descricaoCompleta = descricao.ToString();

            var steps = item.Fields.FirstOrDefault(c => c.Key == "Microsoft.VSTS.TCM.ReproSteps").Value;
            if (steps != null)
                descricaoCompleta += $"<br/> {steps}";

            //Se for Task ou Bug vincula a uma historia
            if ((tipoTask == Constantes.TIPO_ITEM_TASK || tipoTask == Constantes.TIPO_ITEM_BUG))
            {
                //Campo obrigatorio BUG
                if (tipoTask == Constantes.TIPO_ITEM_BUG)
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.BUGem", "Homologação");

                if (tipoTask == Constantes.TIPO_ITEM_TASK)
                    SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Microsoft.VSTS.Common.Activity", "Dev");

                //Link com a historia pai
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{_secundariaAzure.UrlCorporacao}/{_secundariaAzure.ProjetoNome}/_apis/wit/workItems/{historiaId}",
                    }
                });
            }

            var tecnico = item.Fields.FirstOrDefault(c => c.Key == "Custom.e0b31173-d9a6-471a-afe3-7a52d4e0ff9d").Value?.ToString();
            if (tecnico != null)
                SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/Custom.Detalhe", tecnico);

            var descricaoServiceNow = item.Fields.FirstOrDefault(c => c.Key == "Custom.DescricaoServiceNow").Value?.ToString();
            if (descricaoServiceNow != null)
                descricaoCompleta += $"<br/> {descricaoServiceNow}";

            var aceite = item.Fields.FirstOrDefault(c => c.Key == "Custom.88c5eab7-acc0-42bf-a522-614c59da35b0").Value?.ToString();
            if (aceite != null)
                descricaoCompleta += $"<br/> {aceite}";

            var solicitacaoDesc = item.Fields.FirstOrDefault(c => c.Key == "Custom.cea86993-d06d-4650-b3ba-49a878dd09a1").Value?.ToString();
            if (solicitacaoDesc != null)
                descricaoCompleta += $"<br/> {solicitacaoDesc}";


            SincronizarHelper.AdicionarOperacao(patchDocument, operacao, "/fields/System.Description", descricaoCompleta);

            return patchDocument;
        }

        private async Task AtualizarBugVS(Conta _conta)
        {
            try
            {
                DateTime dataUltimaSinc = await _repositorioConsultaSincronizar.Query(e => e.Status == (int)EStatusSincronizar.CONCLUIDO).OrderByDescending(c => c.Id).Select(c => c.DataFim).FirstOrDefaultAsync() ?? DateTime.Now.Date;

                WorkItemTrackingHttpClient witClientP = _vssConnectionPrincipal.GetClient<WorkItemTrackingHttpClient>();

                var witClient = _vssConnectionSecundaria.GetClient<WorkItemTrackingHttpClient>();
                //Filtro data
                var data = dataUltimaSinc.AddSeconds(30).ToString("yyyy-MM-dd HH:mm:ss");

                // Query para buscar bug que foi atualizado depois da ultima sincronização
                Wiql query = new Wiql()
                {
                    Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                 WHERE [System.AssignedTo] EVER '{_conta.EmailPara}'
                                 AND [System.ChangedDate] >= '{data}'
                                 AND [System.WorkItemType] IN ('{Constantes.TIPO_ITEM_BUG}')"
                };
                WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query, timePrecision: true);
                IEnumerable<int> taskIds = queryResult.WorkItems.Select(wi => wi.Id);

                foreach (var item in taskIds)
                {
                    try
                    {
                        WorkItem task = await witClient.GetWorkItemAsync(item);
                        var titulo = task.Fields["System.Title"].ToString().Split(":")[0];
                        titulo = string.Join("", titulo.ToCharArray().Where(Char.IsDigit));

                        #region PRINCIPAL
                        //Buscar bug no principal
                        var idPrincipal = Convert.ToInt32(titulo);
                        Wiql queryP = new Wiql() { Query = $@"SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.Id] = {idPrincipal}" };
                        WorkItemQueryResult queryResultP = await witClientP.QueryByWiqlAsync(queryP);
                        var bugs = queryResultP.WorkItems.FirstOrDefault();
                        var bug = await witClientP.GetWorkItemAsync(bugs.Id);
                        #endregion

                        await CamposCustomizaveis(task, witClient, Operation.Replace, bug, _conta);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"AtualizarBugVS Erro atualizar bug destino: {ex.Message}");
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}