using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Comandos.Contas.CadastrarConta;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Contas.ListarContas
{
    class ConsultaListarContas : IRequestHandler<ParametroListarContas, ResultadCadastrarConta>
    {
        private readonly IRepositorioConsulta<Conta> _repositorioConsultaConta;

        public ConsultaListarContas(
            IRepositorioConsulta<Conta> repositorioConsultaConta
            )
        {
            _repositorioConsultaConta = repositorioConsultaConta;
        }

        public async Task<ResultadCadastrarConta> Handle(ParametroListarContas request, CancellationToken cancellationToken)
        {
            try
            {
                var configuracoes = await _repositorioConsultaConta.Query(readOnly: true).ToListAsync();

                return new ResultadCadastrarConta
                {
                    Sucesso = true,
                    Dados = configuracoes
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
