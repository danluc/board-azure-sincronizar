using Back.Dominio.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Azure.CadastrarAzure
{
    class ComandoCadastrarAzure : IRequestHandler<ParametroCadastrarAzure, ResultadoCadastrarAzure>
    {
        private readonly IRepositorioComando<Dominio.Models.Azure> _repositorioComandoConta;

        public ComandoCadastrarAzure(IRepositorioComando<Dominio.Models.Azure> repositorioComandoConta)
        {
            _repositorioComandoConta = repositorioComandoConta;
        }

        public async Task<ResultadoCadastrarAzure> Handle(ParametroCadastrarAzure request, CancellationToken cancellationToken)
        {
            try
            {
                await _repositorioComandoConta.Insert(request.Dados);
                await _repositorioComandoConta.SaveChangesAsync();

                return new ResultadoCadastrarAzure
                {
                    Sucesso = true,
                    Dados = request.Dados
                };
            }
            catch (Exception ex)
            {
                return new ResultadoCadastrarAzure
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
