using Back.Dominio.DTO.Areas;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Times.ListarIterations
{
    class ConsultaListarIterations : IRequestHandler<ParametroListarIterations, ResultadoListarIterations>
    {
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;

        public ConsultaListarIterations(IRepositorioConsulta<Conta> repositorioConsultaConta)
        {
            _repositorioConsultaConta = repositorioConsultaConta;
        }

        public async Task<ResultadoListarIterations> Handle(ParametroListarIterations request, CancellationToken cancellationToken)
        {
            try
            {
                //Conecta na Azure
                VssConnection connection = new VssConnection(new Uri(request.Dados.Url), new VssBasicCredential(string.Empty, request.Dados.Token));
                //Busca
                WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

                var iterationNodes = await witClient.GetClassificationNodeAsync(request.Dados.ProjetoNome, TreeStructureGroup.Iterations, depth: 2);
                var result = new List<ListaSprintsDTO>();
                foreach (var item in iterationNodes?.Children)
                {
                    var sprint = new ListaSprintsDTO();
                    sprint.Sprints = new List<AreaDTO>();
                    sprint.Time = new AreaDTO(item.Identifier, item.Name, item.Path);
                    if (item.HasChildren.Value)
                    {
                        foreach (var spr in item?.Children)
                            sprint.Sprints.Add(new AreaDTO(spr.Identifier, spr.Name, spr.Path));
                    }
                    
                    result.Add(sprint);
                }

                /*var listaInterations = iterationNodes.Children.Select(e => new ListaSprintsDTO
                {
                    Time = new AreaDTO(e.Identifier, e.Name, e.Path),
                    Sprints = e?.Children.Select(c => new AreaDTO(c.Identifier, c.Name, c.Path)).ToList()
                }).ToList();*/

                return new ResultadoListarIterations
                {
                    Sucesso = true,
                    Dados = result
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarIterations(ex.Message);
            }
        }
    }
}
