using Back.Dominio.Interfaces;
using Back.Servico.Comandos.Azure.CadastrarAzure;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Comandos.Azure.AtualizarAzure
{
    class ComandoAtualizarAzure : IRequestHandler<ParametroAtualizarAzure, ResultadoCadastrarAzure>
    {
        private readonly IRepositorioComando<Dominio.Models.Azure> _repositorioComandoConta;

        public ComandoAtualizarAzure(IRepositorioComando<Dominio.Models.Azure> repositorioComandoConta)
        {
            _repositorioComandoConta = repositorioComandoConta;
        }

        public async Task<ResultadoCadastrarAzure> Handle(ParametroAtualizarAzure request, CancellationToken cancellationToken)
        {
            try
            {
                _repositorioComandoConta.Update(request.Dados);
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
