using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Back.Servico.Consultas.Configuracoes.ListarConfiguracao
{
    class ConsultaListarConfiguracao : IRequestHandler<ParametroListarConfiguracao, ResultadoCadastrarConfiguracao>
    {
        private readonly IRepositorioComando<Configuracao> _repositorioComandoConfiguracao;
        private readonly IRepositorioConsulta<Configuracao> _repositorioConsultaConfiguracao;

        public ConsultaListarConfiguracao(
            IRepositorioComando<Configuracao> repositorioComandoConfiguracao,
            IRepositorioConsulta<Configuracao> repositorioConsultaConfiguracao
            )
        {
            _repositorioComandoConfiguracao = repositorioComandoConfiguracao;
            _repositorioConsultaConfiguracao = repositorioConsultaConfiguracao;
        }

        public async Task<ResultadoCadastrarConfiguracao> Handle(ParametroListarConfiguracao request, CancellationToken cancellationToken)
        {
            try
            {
                var configuracoes = await _repositorioConsultaConfiguracao.Query(readOnly: true).ToListAsync();

                return new ResultadoCadastrarConfiguracao
                {
                    Sucesso = true,
                    Dados = configuracoes
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
