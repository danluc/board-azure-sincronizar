using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Projetos.ListarProjetos
{
    class ConsultaListarProjetos : IRequestHandler<ParametroListarProjetos, ResultadoListarProjetos>
    {
         private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;

        public ConsultaListarProjetos(IRepositorioConsulta<Conta> repositorioConsultaConta)
        {
            _repositorioConsultaConta = repositorioConsultaConta;
        }

        public async Task<ResultadoListarProjetos> Handle(ParametroListarProjetos request, CancellationToken cancellationToken)
        {
            try
            {
                VssConnection connection = new VssConnection(new Uri(request.Url), new VssBasicCredential(string.Empty, request.Token));

                //Busca os projetos
                ProjectHttpClient projetosClient = connection.GetClient<ProjectHttpClient>();
                var projetos = await projetosClient.GetProjects(ProjectState.WellFormed);

                return new ResultadoListarProjetos
                {
                    Sucesso = true,
                    Projetos = projetos.ToList()
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarProjetos(ex.Message);
            }
        }
    }
}
