using Back.Dominio.DTO.Board;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
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
        private readonly IRepositorioComando<Configuracao> _repositorioComandoConfiguracao;
        private readonly IRepositorioConsulta<Configuracao> _repositorioConsultaConfiguracao;
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;
        private readonly ILogger<ComandoSincronizarBoard> _logger;
        private Conta _contaPrincipal;
        private Conta _contaSecundaria;
        private Configuracao _configuracao;

        public ComandoSincronizarBoard(
            IRepositorioComando<Configuracao> repositorioComandoConfiguracao,
            IRepositorioConsulta<Configuracao> repositorioConsultaConfiguracao,
            IRepositorioConsulta<Conta> repositorioConsultaConta,
            ILogger<ComandoSincronizarBoard> logger
            )
        {
            _repositorioComandoConfiguracao = repositorioComandoConfiguracao;
            _repositorioConsultaConfiguracao = repositorioConsultaConfiguracao;
            _repositorioConsultaConta = repositorioConsultaConta;
            _logger = logger;
        }

        public async Task<ResultadoSincronizarBoard> Handle(ParametroSincronizarBoard request, CancellationToken cancellationToken)
        {
            try
            {
                //Busca as contas
                var contas = await _repositorioConsultaConta.Query().ToListAsync();

                if (contas.Count == 0)
                    return new ResultadoSincronizarBoard("Nenhuma conta encontrada na base de dados", true);

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

                return new ResultadoSincronizarBoard
                {
                    Sucesso = true
                };
            }
            catch (Exception ex)
            {
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
            DateTime data = DateTime.Now.AddDays(-_configuracao.Dia).Date;

            WorkItemTrackingHttpClient witClient = connection1.GetClient<WorkItemTrackingHttpClient>();

            // Query para buscar os itens (bug e task) do usuário que está assinadas ou ja foi assinadas a ele
            // Primeiro busca a task e por task, pega a historia
            Wiql query = new Wiql()
            {
                Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                WHERE [System.AssignedTo] EVER '{_contaPrincipal.NomeUsuario}'
                                AND [System.TeamProject] = '{_contaPrincipal.NomeProjeto}'
                                AND ([System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Bug')
                                AND [System.ChangedDate] >= '{data}'"
            };

            _logger.LogInformation($"RecuperaItensSincronizar Query: {query.Query}");

            Guid projetoId = Guid.Parse(_contaPrincipal.ProjetoId);

            //Roda a query
            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query, projetoId);
            IEnumerable<int> taskIds = queryResult.WorkItems.Select(wi => wi.Id);
            _logger.LogInformation($"RecuperaItensSincronizar Itens: {taskIds.Count()}");
            // Recupera as histórias o item em si
            foreach (int taskId in taskIds)
            {
                var migrar = new ItensSicronizarDTO();

                //Busca a task
                WorkItem task = await witClient.GetWorkItemAsync(taskId, expand: WorkItemExpand.Relations);

                //Se não tem pai (historia) não vamos migrar
                if (task.Relations is null)
                    continue;

                // Recupera o pai (historia) da task
                foreach (var hitoria in task.Relations)
                {
                    int hitoriaId = Convert.ToInt32(hitoria.Url.Split('/').Last());
                    WorkItem historiaItem = await witClient.GetWorkItemAsync(hitoriaId);
                    migrar.Historia = historiaItem;
                    _logger.LogInformation($"RecuperaItensSincronizar historia: {historiaItem.Id}");
                }
                _logger.LogInformation($"RecuperaItensSincronizar task: {task.Id}");
                migrar.Itens.Add(task);
                historias.Add(migrar);
            }

            return historias;
        }

        private async Task EnviarTaskDestino(List<ItensSicronizarDTO> historias)
        {
            if (historias.Count == 0)
                return;

            //Faz conexão com o azure
            VssConnection connection = new VssConnection(new Uri(_contaSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, _contaSecundaria.Token));

            WorkHttpClient workClient = connection.GetClient<WorkHttpClient>();
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            // Obter a lista de iterações por equipe
            TeamContext teamContext = new TeamContext(Guid.Parse(_contaSecundaria.ProjetoId), Guid.Parse(_contaSecundaria.TimeId));
            List<TeamSettingsIteration> iterations = await workClient.GetTeamIterationsAsync(teamContext);

            //Pega o ultimo iteration
            var iteration = iterations.FirstOrDefault();

            _logger.LogInformation($"Interation destino: {iteration.Path}");

            foreach (var historia in historias)
            {
                var resultado = await CadastrarItem(connection, historia.Historia, iteration.Path, witClient);
                _logger.LogInformation($"Resultado historia: {resultado}");

                //Se não cadastrou/atualizou  a historia, vamos pular para o proximo
                if (!resultado)
                    continue;

                //Se criou, vamos cadastrar os filhos
                foreach (var task in historia.Itens)
                {
                    await CadastrarItem(connection, task, iteration.Path, witClient);
                }
            }
        }

        private async Task<bool> CadastrarItem(VssConnection connection, WorkItem item, string iterationPath, WorkItemTrackingHttpClient witClient)
        {
            try
            {
                var itemTask = await VerificarSeItemExisteNoDestino(connection, item.Id.Value);
                WorkItem resultado = new WorkItem();
                var tipoTask = item.Fields["System.WorkItemType"].ToString();
                //Se não existir, vamos criar
                if (itemTask is null)
                {
                    _logger.LogInformation($"Fluxo cadastrar {tipoTask} - {item.Id}");
                    var novoItemTask = TratarObjeto(item, iterationPath);
                    resultado = await witClient.CreateWorkItemAsync(novoItemTask, Guid.Parse(_contaSecundaria.ProjetoId), tipoTask);
                }
                else
                {
                    _logger.LogInformation($"Fluxo atualizar {tipoTask} - {item.Id}");
                    var atualizarTask = TratarObjeto(item, iterationPath, Operation.Replace);
                    resultado = await witClient.UpdateWorkItemAsync(atualizarTask, itemTask.Id.Value);
                }

                _logger.LogInformation($"Resultado: {resultado.Id}, tipo: {tipoTask}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Resultado erro: {ex.Message}");
                return false;
            }
        }

        private JsonPatchDocument TratarObjeto(WorkItem item, string iterationPath, Operation operacao = Operation.Add)
        {

            JsonPatchDocument patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.Title",
                    Value = $"{item.Fields["System.Id"]} | {item.Fields["System.Title"]}",
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.AreaPath",
                    Value = $"{_contaSecundaria.AreaPath}"
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.IterationPath",
                    Value = iterationPath
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.State",
                    Value = $"{item.Fields["System.State"]}",
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.AssignedTo",
                    Value = operacao == Operation.Add ? _contaSecundaria.NomeUsuario : $"{item.Fields["System.AssignedTo"]}",
                }
            };

            return patchDocument;
        }

        private async Task<WorkItem> VerificarSeItemExisteNoDestino(VssConnection connection, int id)
        {
            Wiql query = new Wiql() { Query = $@"SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.Title] CONTAINS WORDS '{id}'" };

            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(query);

            var task = queryResult.WorkItems.FirstOrDefault();

            if (task is null)
                return null;

            return await witClient.GetWorkItemAsync(task.Id, expand: WorkItemExpand.Relations);
        }
    }
}