using MediatR;

namespace Back.Servico.Consultas.Azure.ListarAzure
{
    public class ParametroListarAzure : IRequest<ResultadoListarAzure>
    {
        public ParametroListarAzure()
        { }
    }
}
