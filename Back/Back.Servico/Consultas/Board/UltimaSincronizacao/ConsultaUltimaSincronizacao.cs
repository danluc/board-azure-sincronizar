using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Consultas.Board.UltimaSincronizacao
{
    class ConsultaUltimaSincronizacao : IRequestHandler<ParametroUltimaSincronizacao, ResultadoUltimaSincronizacao>
    {
        private readonly IRepositorioConsulta<Sincronizar> _repositorioConsultaSincronizar;

        public ConsultaUltimaSincronizacao(
            IRepositorioConsulta<Sincronizar> repositorioConsultaSincronizar
            )
        {
            _repositorioConsultaSincronizar = repositorioConsultaSincronizar;
        }

        public async Task<ResultadoUltimaSincronizacao> Handle(ParametroUltimaSincronizacao request, CancellationToken cancellationToken)
        {
            try
            {
                var dados = await _repositorioConsultaSincronizar.Query(readOnly: true).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                return new ResultadoUltimaSincronizacao
                {
                    Sucesso = true,
                    Dados = dados
                };
            }
            catch (Exception ex)
            {
                return new ResultadoUltimaSincronizacao
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
