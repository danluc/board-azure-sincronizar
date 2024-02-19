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

                var listaInterations = iterationNodes.Children.FirstOrDefault(e => e.Name.ToUpper() == request.Dados.AreaNome.ToUpper());

                var iterations = new List<AreaDTO>();
                if (listaInterations.Children != null)
                    iterations = listaInterations.Children.Select(e => new AreaDTO { Id = e.Identifier, Name = e.Name, Path = e.Path }).ToList();
                else
                {
                    var area = new AreaDTO { Id = listaInterations.Identifier, Name = listaInterations.Name, Path = listaInterations.Path };
                    iterations.Add(area);
                }

                return new ResultadoListarIterations
                {
                    Sucesso = true,
                    Dados = iterations
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarIterations(ex.Message);
            }
        }
    }
}
