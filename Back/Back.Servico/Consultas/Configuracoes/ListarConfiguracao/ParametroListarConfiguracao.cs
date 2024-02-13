using Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao;
using MediatR;

namespace Back.Servico.Consultas.Configuracoes.ListarConfiguracao
{
    public class ParametroListarConfiguracao : IRequest<ResultadoCadastrarConfiguracao>
    {
        public ParametroListarConfiguracao()
        {
        }
    }
}
