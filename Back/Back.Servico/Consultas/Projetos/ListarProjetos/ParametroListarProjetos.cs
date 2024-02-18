using MediatR;

namespace Back.Servico.Consultas.Projetos.ListarProjetos
{
    public class ParametroListarProjetos : IRequest<ResultadoListarProjetos>
    {
        public ParametroListarProjetos(string url, string token)
        {
            Url = url;
            Token = token;
        }

        public string Url { get; }
        public string Token { get; }
    }
}
