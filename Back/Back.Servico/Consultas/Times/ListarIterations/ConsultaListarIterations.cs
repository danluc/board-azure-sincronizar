﻿using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
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
                WorkHttpClient workClient = connection.GetClient<WorkHttpClient>();
                TeamContext teamContext = new TeamContext(request.Dados.ProjetoNome, request.Dados.TimeNome);

                List<TeamSettingsIteration> iterations = await workClient.GetTeamIterationsAsync(teamContext);

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
