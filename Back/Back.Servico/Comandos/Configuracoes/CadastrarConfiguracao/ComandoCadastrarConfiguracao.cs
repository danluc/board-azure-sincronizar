using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao
{
    class ComandoCadastrarConfiguracao : IRequestHandler<ParametroCadastrarConfiguracao, ResultadoCadastrarConfiguracao>
    {
        private readonly IRepositorioComando<Configuracao> _repositorioComandoConfiguracao;

        public ComandoCadastrarConfiguracao(IRepositorioComando<Configuracao> repositorioComandoConfiguracao)
        {
            _repositorioComandoConfiguracao = repositorioComandoConfiguracao;
        }

        public async Task<ResultadoCadastrarConfiguracao> Handle(ParametroCadastrarConfiguracao request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Dados.Count == 0)
                    throw new Exception($"Nenhum dado na requisição");

                await _repositorioComandoConfiguracao.InsertRange(request.Dados);
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
