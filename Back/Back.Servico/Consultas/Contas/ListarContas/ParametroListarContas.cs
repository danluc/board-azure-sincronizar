using MediatR;

namespace Back.Servico.Consultas.Contas.ListarContas
{
    public class ParametroListarContas : IRequest<ResultadoListarContas>
    {
        public ParametroListarContas()
        {}
    }
}
