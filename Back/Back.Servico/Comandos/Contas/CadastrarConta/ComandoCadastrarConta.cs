using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Contas.CadastrarConta
{
    class ComandoCadastrarConta : IRequestHandler<ParametroCadastrarConta, ResultadCadastrarConta>
    {
        private readonly IRepositorioComando<Conta> _repositorioComandoConta;

        public ComandoCadastrarConta(IRepositorioComando<Conta> repositorioComandoConta)
        {
            _repositorioComandoConta = repositorioComandoConta;
        }

        public async Task<ResultadCadastrarConta> Handle(ParametroCadastrarConta request, CancellationToken cancellationToken)
        {
            try
            {
                await _repositorioComandoConta.Insert(request.Dados);
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
