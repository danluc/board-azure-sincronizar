using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Comandos.Contas.CadastrarConta;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Contas.AtualizarConta
{
    class ComandoAtualizarConta : IRequestHandler<ParametroAtualizarConta, ResultadCadastrarConta>
    {
        private readonly IRepositorioComando<Conta> _repositorioComandoConta;

        public ComandoAtualizarConta(IRepositorioComando<Conta> repositorioComandoConta)
        {
            _repositorioComandoConta = repositorioComandoConta;
        }

        public async Task<ResultadCadastrarConta> Handle(ParametroAtualizarConta request, CancellationToken cancellationToken)
        {
            try
            {
                _repositorioComandoConta.Update(request.Dados);
                await _repositorioComandoConta.SaveChangesAsync();

                return new ResultadCadastrarConta
                {
                    Sucesso = true,
                    Dados = request.Dados
                };
            }
            catch (Exception ex)
            {
                return new ResultadCadastrarConta
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
