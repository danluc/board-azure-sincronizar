using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Board.ListarSincronizacoes
{
    class ConsultaListarSincronizacoes : IRequestHandler<ParametroListarSincronizacoes, ResultadoListarSincronizacoes>
    {
        private readonly IRepositorioConsulta<Sincronizar> _repositorioConsultaSincronizar;

        public ConsultaListarSincronizacoes(
            IRepositorioConsulta<Sincronizar> repositorioConsultaSincronizar
            )
        {
            _repositorioConsultaSincronizar = repositorioConsultaSincronizar;
        }

        public async Task<ResultadoListarSincronizacoes> Handle(ParametroListarSincronizacoes request, CancellationToken cancellationToken)
        {
            try
            {
                var dados = await _repositorioConsultaSincronizar.Query(readOnly: true).ToListAsync();

                return new ResultadoListarSincronizacoes
                {
                    Sucesso = true,
                    Dados = dados
                };
            }
            catch (Exception ex)
            {
                return new ResultadoListarSincronizacoes
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
