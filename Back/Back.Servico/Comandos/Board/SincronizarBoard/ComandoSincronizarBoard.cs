using Back.Dominio;
using Back.Dominio.DTO.Board;
using Back.Dominio.Enum;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Hubs.Notificacoes;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IRepositorioConsulta<Configuracao> _repositorioConsultaConfiguracao;
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;
        private readonly ILogger<ComandoSincronizarBoard> _logger;
        private readonly NotificationHubService _notificationHubService;
        private Conta _contaPrincipal;
        private Conta _contaSecundaria;
        private Configuracao _configuracao;
        private int _areaId = 0;

        public ComandoSincronizarBoard(
            IRepositorioComando<Sincronizar> repositorioComandoSincronizar,
            IRepositorioConsulta<Configuracao> repositorioConsultaConfiguracao,
            IRepositorioConsulta<Conta> repositorioConsultaConta,
            ILogger<ComandoSincronizarBoard> logger,
            NotificationHubService notificationHubService
            )
        {
            _repositorioComandoSincronizar = repositorioComandoSincronizar;
            _repositorioConsultaConfiguracao = repositorioConsultaConfiguracao;
            _repositorioConsultaConta = repositorioConsultaConta;
            _logger = logger;
            _notificationHubService = notificationHubService;
        }

        public async Task<ResultadoSincronizarBoard> Handle(ParametroSincronizarBoard request, CancellationToken cancellationToken)
        {
            int sincronizarId = 0;
            var sincronizar = new Sincronizar { DataInicio = DateTime.Now, Status = (int)EStatusSincronizar.PROCESSANDO };
            try
            {
                //Busca as contas
                var contas = await _repositorioConsultaConta.Query().ToListAsync();

                if (contas.Count == 0)
                    return new ResultadoSincronizarBoard("Nenhuma conta encontrada na base de dados", true);

                #region REGISTRANDO_TABELA
                var insert = await _repositorioComandoSincronizar.Insert(sincronizar);
                await _repositorioComandoSincronizar.SaveChangesAsync();
                sincronizarId = insert.Id;
                #endregion

                _configuracao = await _repositorioConsultaConfiguracao.Query().FirstOrDefaultAsync();

                //Conta principal
                _contaPrincipal = contas.FirstOrDefault(e => e.Principal);

                //Busca as task para sincronizar
                var itensSincronizar = await RecuperaItensSincronizar();

                //Conta secundaria
                _contaSecundaria = contas.FirstOrDefault(e => !e.Principal);

                _logger.LogInformation($"Qtd itens a ser copiados: {itensSincronizar.Count()}");

                //Migra as task
                await EnviarTaskDestino(itensSincronizar);

                #region ATUALIZANDO_TABELA
                insert.DataFim = DateTime.Now;
                insert.Status = (int)EStatusSincronizar.CONCLUIDO;
                _repositorioComandoSincronizar.Update(insert);
                await _repositorioComandoSincronizar.SaveChangesAsync();
                #endregion


                await _notificationHubService.Notificar(Constantes.NOTIFICACAO_GRUPO_LOCAL);

                return new ResultadoSincronizarBoard
                {
                    Sucesso = true
                };
            }
            catch (Exception ex)
            {
                #region REGISTRANDO_TABELA
                var sincronizarErro = new Sincronizar();
                sincronizarErro.DataInicio = sincronizar.DataInicio;
                sincronizarErro.DataFim = DateTime.Now;
                sincronizarErro.Status = (int)EStatusSincronizar.ERRO;
                sincronizarErro.Id = sincronizarId;
                _repositorioComandoSincronizar.Update(sincronizarErro);
                await _repositorioComandoSincronizar.SaveChangesAsync();
                #endregion

                return new ResultadoSincronizarBoard
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }

        private async Task<List<ItensSicronizarDTO>> RecuperaItensSincronizar()
        {
            //Resultado
            var historias = new List<ItensSicronizarDTO>();

            //Faz conexão com o azure
            VssConnection connection1 = new VssConnection(new Uri(_contaPrincipal.UrlCorporacao), new VssBasicCredential(string.Empty, _contaPrincipal.Token));

            //Filtro data
            string data = DateTime.Now.AddDays(-_configuracao.Dia).Date.ToString("yyyy-MM-dd");

            WorkItemTrackingHttpClient witClient = connection1.GetClient<WorkItemTrackingHttpClient>();

            // Query para buscar os itens (bug e task) do usuário que está assinadas ou ja foi assinadas a ele
            // Primeiro busca a task e por task, pega a historia
            Wiql query = new Wiql()
            {
                Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                WHERE [System.AssignedTo] EVER @Me
                                AND [System.TeamProject] = '{_contaPrincipal.ProjetoNome}'
                                AND ([System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Bug')
                                AND System.ChangedDate >= '{data} 00:00:00'"
            };

            _logger.LogInformation($"RecuperaItensSincronizar Query: {query.Query}");

            Guid projetoId = Guid.Parse(_contaPrincipal.ProjetoId);

            //Roda a query
            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query, projetoId);
            IEnumerable<int> taskIds = queryResult.WorkItems.Select(wi => wi.Id);
            _logger.LogInformation($"RecuperaItensSincronizar Itens: {taskIds.Count()}");
            // Recupera as histórias, o item em si
            foreach (int taskId in taskIds)
            {
                var migrar = new ItensSicronizarDTO();

                //Busca a task
                WorkItem task = await witClient.GetWorkItemAsync(taskId, expand: WorkItemExpand.Relations);

                //Se não tem pai (historia) não vamos migrar
                if (task.Relations is null)
                    continue;

                int historiaId = 0;
                // Recupera o pai (historia) da task
                foreach (var hitoria in task.Relations)
                {
                    historiaId = Convert.ToInt32(hitoria.Url.Split('/').Last());
                    WorkItem historiaItem = await witClient.GetWorkItemAsync(historiaId);
                    //Verifica se ja existe a historia
                    var hasHistoria = historias.FirstOrDefault(c => c.Historia.Id == historiaItem.Id);
                    if (hasHistoria != null)
                    {
                        migrar.Historia = null;
                        historiaId = historiaItem.Id.Value;
                    }
                    else
                        migrar.Historia = historiaItem;

                    _logger.LogInformation($"RecuperaItensSincronizar historia: {historiaItem.Id}");
                }
                _logger.LogInformation($"RecuperaItensSincronizar task: {task.Id}");

                //Verifica se ja existe a task
                var hasTask = migrar.Itens.FirstOrDefault(c => c.Id == task.Id);
                if (hasTask is null)
                    migrar.Itens.Add(task);

                if (migrar.Historia is null)
                {
                    var historia = historias.FirstOrDefault(c => c.Historia.Id == historiaId);
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
            if (historias.Count == 0)
                return;

            //Faz conexão com o azure
            VssConnection connection = new VssConnection(new Uri(_contaSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, _contaSecundaria.Token));

            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            _areaId = await BuscarArea(witClient) ?? 0;

            foreach (var historia in historias)
            {
                var resultado = await CadastrarItem(connection, historia.Historia, witClient, 0);
                _logger.LogInformation($"Resultado historia: {resultado}");

                //Se não cadastrou/atualizou  a historia, vamos pular para o proximo
                if (resultado is null)
                    continue;

                //Se criou/atualizou, vamos cadastrar/atualizar os filhos
                foreach (var task in historia.Itens)
                    await CadastrarItem(connection, task, witClient, resultado.Id.Value);

            }
        }

        private async Task<int?> BuscarArea(WorkItemTrackingHttpClient witClient)
        {
            var areasNode = await witClient.GetClassificationNodeAsync(_contaSecundaria.ProjetoNome, TreeStructureGroup.Areas, depth: 2);

            if (areasNode.Children is null)
                return areasNode.Id;

            var listaAreas = areasNode.Children.FirstOrDefault(e => _contaSecundaria.TimeNome.ToUpper().Contains(e.Name.ToUpper()));

            if (listaAreas.Children != null)
                return listaAreas.Children.FirstOrDefault(e => e.Name.ToUpper() == _contaSecundaria.AreaPath.ToUpper())?.Id;
            else
                return listaAreas.Id;
        }

        private async Task<WorkItem> CadastrarItem(VssConnection connection, WorkItem item, WorkItemTrackingHttpClient witClient, int historiaId)
        {
            try
            {
                WorkItem resultado = new WorkItem();
                var itemTask = await VerificarSeItemExisteNoDestino(connection, item.Id.Value);
                var tipoTask = item.Fields["System.WorkItemType"].ToString();
                var status = item.Fields["System.State"].ToString();
                //Se não existir, vamos criar
                if (itemTask is null)
                {
                    _logger.LogInformation($"Fluxo cadastrar {tipoTask} - {item.Id}");
                    var novoItemTask = TratarObjeto(item, historiaId);
                    resultado = await witClient.CreateWorkItemAsync(novoItemTask, Guid.Parse(_contaSecundaria.ProjetoId), tipoTask);
                    //Não pode criar um item ja com status ativo, então cria como novo e logo atualiza
                    if (status != Constantes.STATUS_NOVO)
                    {
                        var atualizarTask = new JsonPatchDocument
                        {
                             new JsonPatchOperation()
                             {
                               Operation = Operation.Replace,
                               Path = "/fields/System.State",
                               Value = status,
                             }
                        };
                        await witClient.UpdateWorkItemAsync(atualizarTask, resultado.Id.Value);
                    }
                }
                else
                {
                    _logger.LogInformation($"Fluxo atualizar {tipoTask} - {item.Id}");
                    item.Fields["System.AssignedTo"] = itemTask.Fields["System.AssignedTo"];
                    var atualizarTask = TratarObjeto(item, historiaId, Operation.Replace);
                    resultado = await witClient.UpdateWorkItemAsync(atualizarTask, itemTask.Id.Value);
                }

                _logger.LogInformation($"Resultado: {resultado.Id}, tipo: {tipoTask}");
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Resultado erro: {ex.Message}");
                return null;
            }
        }

        private JsonPatchDocument TratarObjeto(WorkItem item, int historiaId, Operation operacao = Operation.Add)
        {

            JsonPatchDocument patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.Title",
                    Value = $"{item.Id}: {item.Fields["System.Title"]}",
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.AreaId",
                    Value = $"{_areaId}"
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.IterationPath",
                    Value = $"{_contaSecundaria.Sprint.Replace("Iteration\\", "")}"
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.State",
                    Value = operacao == Operation.Add ? Constantes.STATUS_NOVO : $"{item.Fields["System.State"]}",
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.Description",
                    Value = $"{item.Fields["System.Description"]}",
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.AssignedTo",
                    Value = operacao == Operation.Add ? _contaSecundaria.NomeUsuario : ((IdentityRef)item.Fields["System.AssignedTo"]).UniqueName,
                }
            };
            var tipoTask = item.Fields["System.WorkItemType"].ToString();
            if ((tipoTask == Constantes.TIPO_ITEM_TASK || tipoTask == Constantes.TIPO_ITEM_BUG))
            {
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{_contaSecundaria.UrlCorporacao}/{_contaSecundaria.ProjetoNome}/_apis/wit/workItems/{historiaId}",
                    }
                });
            }

            return patchDocument;
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
    }
}