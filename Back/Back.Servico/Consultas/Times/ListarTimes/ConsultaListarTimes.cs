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


namespace Back.Servico.Consultas.Times.ListarTimes
{
    class ConsultaListarTimes : IRequestHandler<ParametroListarTimes, ResultadoListarTimes>
    {
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;

        public ConsultaListarTimes(IRepositorioConsulta<Conta> repositorioConsultaConta)
        {
            _repositorioConsultaConta = repositorioConsultaConta;
        }

        public async Task<ResultadoListarTimes> Handle(ParametroListarTimes request, CancellationToken cancellationToken)
        {
            try
            {
                //Busca as configuracao
                var configuracaoSecundaria = await _repositorioConsultaConta.FirstOrDefault(e => !e.Principal);

                if (configuracaoSecundaria is null)
                    return new ResultadoListarTimes("Nenhuma conta encontrada na base de dados", true);

                //Conecta na Azure
                VssConnection connection2 = new VssConnection(new Uri(configuracaoSecundaria.UrlCorporacao), new VssBasicCredential(string.Empty, configuracaoSecundaria.Token));

                //Busca os times
                TeamHttpClient teamClients = connection2.GetClient<TeamHttpClient>();
                List<WebApiTeam> teams = await teamClients.GetTeamsAsync(request.ProjetoNome);

                return new ResultadoListarTimes
                {
                    Sucesso = true,
                    Dados = teams
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarTimes(ex.Message);
            }
        }
    }
}
