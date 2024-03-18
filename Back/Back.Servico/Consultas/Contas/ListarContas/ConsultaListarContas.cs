using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Contas.ListarContas
{
    class ConsultaListarContas : IRequestHandler<ParametroListarContas, ResultadoListarContas>
    {
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;

        public ConsultaListarContas(
            IRepositorioConsulta<Conta> repositorioConsultaConta
            )
        {
            _repositorioConsultaConta = repositorioConsultaConta;
        }

        public async Task<ResultadoListarContas> Handle(ParametroListarContas request, CancellationToken cancellationToken)
        {
            try
            {
                var configuracoes = await _repositorioConsultaConta.Query(readOnly: true).ToListAsync();

                return new ResultadoListarContas
                {
                    Sucesso = true,
                    Dados = configuracoes
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarContas
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
