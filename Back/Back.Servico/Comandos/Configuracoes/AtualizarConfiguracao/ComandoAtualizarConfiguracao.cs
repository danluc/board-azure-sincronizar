using Back.Dominio;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao;
using Back.Servico.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Configuracoes.AtualizarConfiguracao
{
    class ComandoAtualizarConfiguracao : IRequestHandler<ParametroAtualizarConfiguracao, ResultadoCadastrarConfiguracao>
    {
        private readonly IRepositorioComando<Configuracao> _repositorioComandoConfiguracao;
        private readonly IRepositorioConsulta<Configuracao> _repositorioConsultaConfiguracao;
        private readonly QuartzHostedService _quartzHostedService;

        public ComandoAtualizarConfiguracao(
            IRepositorioComando<Configuracao> repositorioComandoConfiguracao,
            IRepositorioConsulta<Configuracao> repositorioConsultaConfiguracao,
            QuartzHostedService quartzHostedService
            )
        {
            _repositorioComandoConfiguracao = repositorioComandoConfiguracao;
            _repositorioConsultaConfiguracao = repositorioConsultaConfiguracao;
            _quartzHostedService = quartzHostedService;
        }

        public async Task<ResultadoCadastrarConfiguracao> Handle(ParametroAtualizarConfiguracao request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Dados.Count == 0)
                    throw new Exception($"Nenhum dado na requisição");

                var configuracoes = await _repositorioConsultaConfiguracao.Query(readOnly: true).FirstOrDefaultAsync();

                if (configuracoes is null)
                    throw new Exception($"Nenhuma configuração encontrada na base de dados");

                if (request.Dados[0].HoraCron != configuracoes.HoraCron)
                    await _quartzHostedService.AtualizarHoraJob(cancellationToken, request.Dados[0].HoraCron);

                _repositorioComandoConfiguracao.UpdateRange(request.Dados);
                await _repositorioComandoConfiguracao.SaveChangesAsync();

                return new ResultadoCadastrarConfiguracao
                {
                    Sucesso = true,
                    Dados = request.Dados
                };
            }
            catch (Exception ex)
            {
                return new ResultadoCadastrarConfiguracao
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}

