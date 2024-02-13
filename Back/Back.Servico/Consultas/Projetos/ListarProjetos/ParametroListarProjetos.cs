using MediatR;

namespace Back.Servico.Consultas.Projetos.ListarProjetos
{
    public class ParametroListarProjetos : IRequest<ResultadoListarProjetos>
    {
        public ParametroListarProjetos()
        {}
    }
}
