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
                //Busca as configuracao
                var configuracoes = await _repositorioConsultaConta.Query().ToListAsync();

                if (configuracoes.Count == 0)
                    return new ResultadoListarProjetos("Nenhuma conta encontrada na base de dados", true);

                var configuracaoPrincipal = configuracoes.FirstOrDefault(e => e.Principal);
                var configuracaoSecundaria = configuracoes.FirstOrDefault(e => !e.Principal);

                //Conecta na Azure
                VssConnection connection1 = new VssConnection(new Uri(configuracaoPrincipal.UrlCorporacao), new VssBasicCredential(string.Empty, configuracaoPrincipal.Token));
                VssConnection connection2 = new VssConnection(new Uri(configuracaoSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, configuracaoSecundaria.Token));

                //Busca os projetos
                ProjectHttpClient projetosClient1 = connection1.GetClient<ProjectHttpClient>();
                ProjectHttpClient projetosClient2 = connection2.GetClient<ProjectHttpClient>();

                var projetos1 = await projetosClient1.GetProjects(ProjectState.WellFormed);
                var projetos2 = await projetosClient2.GetProjects(ProjectState.WellFormed);

                return new ResultadoListarProjetos
                {
                    Sucesso = true,
                    ProjetosPrincipal = projetos1.ToList(),
                    ProjetosSecundario = projetos2.ToList()
                };
            
            }
            catch (Exception ex)
            {
                return new ResultadoListarProjetos(ex.Message);
            }
        }
    }
}
