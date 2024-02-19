using Back.Dominio.Enum;
using MediatR;

namespace Back.Servico.Consultas.Board.ListarSincronizacoes
{
    public class ParametroListarSincronizacoes : IRequest<ResultadoListarSincronizacoes>
    {
        public ParametroListarSincronizacoes(EStatusSincronizar status = EStatusSincronizar.PROCESSANDO)
        {
            Status = status;
        }

        public EStatusSincronizar Status { get; }
    }
}
