using Back.Dominio.Models;
using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao
{
    public class ParametroCadastrarConfiguracao : IRequest<ResultadoCadastrarConfiguracao>
    {
        public ParametroCadastrarConfiguracao(List<Configuracao> dados)
        {
            Dados = dados;
        }

        public List<Configuracao> Dados { get; }
    }
}
