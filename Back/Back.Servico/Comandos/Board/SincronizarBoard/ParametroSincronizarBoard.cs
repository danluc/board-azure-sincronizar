using MediatR;

namespace Back.Servico.Comandos.Board.SincronizarBoard
{
    public class ParametroSincronizarBoard : IRequest<ResultadoSincronizarBoard>
    {
        public ParametroSincronizarBoard()
        {}
    }
}
