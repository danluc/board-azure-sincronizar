using Back.Dominio.DTO.Areas;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Back.Servico.Consultas.Times.ListarAreas
{
    class ConsultaListarAreas : IRequestHandler<ParametroListarAreas, ResultadoListarAreas>
    {
        private readonly IRepositorioConsulta<Dominio.Models.Azure> _repositorioConsultaAzure;

        public ConsultaListarAreas(IRepositorioConsulta<Dominio.Models.Azure> repositorioConsultaAzure)
        {
            _repositorioConsultaAzure = repositorioConsultaAzure;
        }

        public async Task<ResultadoListarAreas> Handle(ParametroListarAreas request, CancellationToken cancellationToken)
        {
            try
            {
                var azure = await _repositorioConsultaAzure.FirstOrDefault(e => !e.Principal);
                //Conecta na Azure
                VssConnection connection = new VssConnection(new Uri(azure.UrlCorporacao), new VssBasicCredential(string.Empty, azure.Token));
                //Busca
                WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

                var iterationNodes = await witClient.GetClassificationNodeAsync(azure.ProjetoNome, TreeStructureGroup.Areas, depth: 2);

                var listaInterations = iterationNodes.Children.FirstOrDefault(e => e.Name.ToUpper() == azure.TimeNome.ToUpper());

                var iterations = new List<AreaDTO>();
                if (listaInterations.Children != null)
                    iterations = listaInterations.Children.Select(e => new AreaDTO { Id = e.Id, Identificador = e.Identifier, Name = e.Name, Path = e.Path }).ToList();
                else
                {
                    var area = new AreaDTO { Id = listaInterations.Id, Identificador = listaInterations.Identifier, Name = listaInterations.Name, Path = listaInterations.Path };
                    iterations.Add(area);
                }

                return new ResultadoListarAreas
                {
                    Sucesso = true,
                    Dados = iterations
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarAreas(ex.Message);
            }
        }
    }
}
