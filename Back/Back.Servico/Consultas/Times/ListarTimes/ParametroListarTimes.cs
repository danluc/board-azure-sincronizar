using MediatR;

namespace Back.Servico.Consultas.Times.ListarTimes
{
    public class ParametroListarTimes : IRequest<ResultadoListarTimes>
    {
        public ParametroListarTimes(string projetoNome, string url, string token)
        {
            ProjetoNome = projetoNome;
            Url = url;
            Token = token;
        }

        public string ProjetoNome { get; }
        public string Url { get; }
        public string Token { get; }
    }
}