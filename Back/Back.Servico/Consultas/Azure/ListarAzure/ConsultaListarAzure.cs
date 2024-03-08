using Back.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Azure.ListarAzure
{
    class ConsultaListarAzure : IRequestHandler<ParametroListarAzure, ResultadoListarAzure>
    {
        private readonly IRepositorioConsulta<Dominio.Models.Azure> _repositorioConsultaConta;

        public ConsultaListarAzure(IRepositorioConsulta<Dominio.Models.Azure> repositorioConsultaConta)
        {
            _repositorioConsultaConta = repositorioConsultaConta;
        }

        public async Task<ResultadoListarAzure> Handle(ParametroListarAzure request, CancellationToken cancellationToken)
        {
            try
            {
                var configuracoes = await _repositorioConsultaConta.Query(readOnly: true).ToListAsync();

                return new ResultadoListarAzure
                {
                    Sucesso = true,
                    Dados = configuracoes
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarAzure
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
