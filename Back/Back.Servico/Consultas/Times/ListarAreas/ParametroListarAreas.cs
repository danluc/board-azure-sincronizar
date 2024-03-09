using MediatR;

namespace Back.Servico.Consultas.Times.ListarAreas
{
    public class ParametroListarAreas : IRequest<ResultadoListarAreas>
    {
        public ParametroListarAreas()
        {}
    }
}
