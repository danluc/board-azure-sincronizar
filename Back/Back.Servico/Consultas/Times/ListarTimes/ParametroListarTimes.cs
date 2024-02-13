using MediatR;

namespace Back.Servico.Consultas.Times.ListarTimes
{
    public class ParametroListarTimes : IRequest<ResultadoListarTimes>
    {
        public ParametroListarTimes(string projetoNome)
        {
            ProjetoNome = projetoNome;
        }

        public string ProjetoNome { get; }
    }
}