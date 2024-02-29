﻿using Back.Dominio;
using Back.Dominio.DTO.Board;
using Back.Dominio.Enum;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Comandos.Board._Helpers;
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
        private readonly IRepositorioConsulta<Sincronizar> _repositorioConsultaSincronizar;
        private readonly IRepositorioConsulta<Configuracao> _repositorioConsultaConfiguracao;
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;
        private readonly ILogger<ComandoSincronizarBoard> _logger;
        private readonly NotificationHubService _notificationHubService;
        private List<WorkItem> _itensSolicitacoes;
        private Conta _contaPrincipal;
        private Conta _contaSecundaria;
        private Configuracao _configuracao;
        private int _areaId = 0;

        public ComandoSincronizarBoard(
            IRepositorioComando<Sincronizar> repositorioComandoSincronizar,
            IRepositorioConsulta<Sincronizar> repositorioConsultaSincronizar,
            IRepositorioConsulta<Configuracao> repositorioConsultaConfiguracao,
            IRepositorioConsulta<Conta> repositorioConsultaConta,
            ILogger<ComandoSincronizarBoard> logger,
            NotificationHubService notificationHubService
            )
        {
            _repositorioComandoSincronizar = repositorioComandoSincronizar;
            _repositorioConsultaSincronizar = repositorioConsultaSincronizar;
            _repositorioConsultaConfiguracao = repositorioConsultaConfiguracao;
            _repositorioConsultaConta = repositorioConsultaConta;
            _logger = logger;
            _notificationHubService = notificationHubService;
            _itensSolicitacoes = new List<WorkItem>();
        }

        public async Task<ResultadoSincronizarBoard> Handle(ParametroSincronizarBoard request, CancellationToken cancellationToken)
        {
            //Envia a notificação para o front
            await _notificationHubService.NotificarInicio();

            var sincronizar = new Sincronizar { DataInicio = DateTime.Now, Status = (int)EStatusSincronizar.PROCESSANDO };
            try
            {
                //Busca as contas
                var contas = await _repositorioConsultaConta.Query().ToListAsync();

                if (contas.Count == 0)
                    throw new Exception($"Nenhuma conta encontrada na base de dados");

                #region REGISTRANDO_TABELA
                await _repositorioComandoSincronizar.Insert(sincronizar);
                await _repositorioComandoSincronizar.SaveChangesAsync();
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
                await AtualizarUltimoSicronizar(EStatusSincronizar.CONCLUIDO);
                #endregion


                //Atualizar historias fechadas
                await AtualizarStatusHistorias();

                await _notificationHubService.NotificarFim();

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

                await _notificationHubService.NotificarFim();

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
                                AND System.ChangedDate >= '{data} 00:00:00'
                                AND ([System.WorkItemType] = '{Constantes.TIPO_ITEM_TASK}'
                                    OR [System.WorkItemType] = '{Constantes.TIPO_ITEM_BUG}'
                                    OR [System.WorkItemType] = '{Constantes.TIPO_ITEM_SOLICITACAO}')
                                "
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

                //Pega apenas itens de trabalho
                var historiaId = task.Fields.FirstOrDefault(c => c.Key.Contains("System.Parent")).Value;

                //Se não tem pai (historia) não vamos migrar
                if (historiaId is null)
                {
                    //Verificar se é uma solicitação, ela não tem historia
                    var tipoTask = task.Fields["System.WorkItemType"].ToString();
                    if (tipoTask == Constantes.TIPO_ITEM_SOLICITACAO)
                        _itensSolicitacoes.Add(task);

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

            //Verifica se tem soliciatação para migrar
            foreach (var solicitacao in _itensSolicitacoes)
            {
                var resultado = await CadastrarItem(connection, solicitacao, witClient, 0);
                _logger.LogInformation($"Resultado Solicitação: {resultado}");
            }
        }

        private async Task<int?> BuscarArea(WorkItemTrackingHttpClient witClient)
        {
            var areasNode = await witClient.GetClassificationNodeAsync(_contaSecundaria.ProjetoNome, TreeStructureGroup.Areas, depth: 2);

            if (areasNode.Children is null)
                return areasNode.Id;

            var listaAreas = areasNode.Children.FirstOrDefault(e => _contaSecundaria.TimeNome.ToUpper().Contains(e.Name.ToUpper()));

            if (listaAreas is null)
                return areasNode.Id;

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
                var operation = Operation.Replace;
                //Se não existir, vamos criar
                if (itemTask is null)
                {
                    _logger.LogInformation($"Fluxo cadastrar {tipoTask} - {item.Id}");
                    var novoItemTask = SincronizarHelper.TratarObjeto(item, historiaId, _contaSecundaria, _areaId);
                    resultado = await witClient.CreateWorkItemAsync(novoItemTask, Guid.Parse(_contaSecundaria.ProjetoId), tipoTask);
                    //Não pode criar um item ja com status ativo, então cria como novo e logo atualiza
                    if (status != Constantes.STATUS_NOVO)
                    {
                        var patchDocument = new JsonPatchDocument();
                        SincronizarHelper.AdicionarOperacao(patchDocument, Operation.Replace, "/fields/System.State", SincronizarHelper.BuscarStatusItem(status));
                        await witClient.UpdateWorkItemAsync(patchDocument, resultado.Id.Value);
                    }
                    operation = Operation.Add;
                }
                else
                {
                    _logger.LogInformation($"Fluxo atualizar {tipoTask} - {item.Id}");

                    item.Fields["System.AssignedTo"] = itemTask.Fields.FirstOrDefault(c => c.Key == "System.AssignedTo").Value ?? _contaSecundaria.NomeUsuario;
                    var atualizarTask = SincronizarHelper.TratarObjeto(item, historiaId, _contaSecundaria, _areaId, Operation.Replace);
                    resultado = await witClient.UpdateWorkItemAsync(atualizarTask, itemTask.Id.Value);
                }

                //Se for bug, vamos atualizar os campos customizaveis
                if (tipoTask == Constantes.TIPO_ITEM_BUG)
                    await CamposCustomizaveis(resultado, witClient, operation, item);

                _logger.LogInformation($"Resultado: {resultado.Id}, tipo: {tipoTask}");
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

                //Se tiver cliente cadastrado
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

                await witClient.UpdateWorkItemAsync(patchDocument, item.Id.Value);
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
            }
            catch (Exception)
            { }
        }


        private async Task AtualizarStatusHistorias()
        {
            //Faz conexão com o azure
            VssConnection connection1 = new VssConnection(new Uri(_contaPrincipal.UrlCorporacao), new VssBasicCredential(string.Empty, _contaPrincipal.Token));
            VssConnection conectDestino = new VssConnection(new Uri(_contaSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, _contaSecundaria.Token));

            //Filtro data
            string data = DateTime.Now.AddDays(-60).Date.ToString("yyyy-MM-dd");

            WorkItemTrackingHttpClient witClient = connection1.GetClient<WorkItemTrackingHttpClient>();

            // Query para buscar os itens (bug e task) do usuário que está assinadas ou ja foi assinadas a ele
            // Primeiro busca a task e por task, pega a historia
            Wiql query = new Wiql()
            {
                Query = $@"SELECT [System.Id], [System.Title]  FROM WorkItems
                                WHERE [System.AssignedTo] EVER @Me
                                AND [System.TeamProject] = '{_contaPrincipal.ProjetoNome}'
                                AND System.ChangedDate >= '{data} 00:00:00'
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
                //Não pode criar um item ja com status ativo, então cria como novo e logo atualiza
                if (status != historia.Fields["System.State"].ToString())
                {
                    _logger.LogInformation($"Historia fechada: {historia.Id.Value}");
                    var patchDocument = new JsonPatchDocument();
                    SincronizarHelper.AdicionarOperacao(patchDocument, Operation.Replace, "/fields/System.State", SincronizarHelper.BuscarStatusItem(status));
                    await witClient.UpdateWorkItemAsync(patchDocument, historia.Id.Value);
                }
            }
        }
    }
}