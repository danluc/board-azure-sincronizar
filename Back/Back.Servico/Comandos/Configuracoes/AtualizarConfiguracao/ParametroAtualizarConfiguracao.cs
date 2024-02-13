using Back.Dominio.Models;
using Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao;
using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Configuracoes.AtualizarConfiguracao
{
    public class ParametroAtualizarConfiguracao : IRequest<ResultadoCadastrarConfiguracao>
    {
        public ParametroAtualizarConfiguracao(List<Configuracao> dados)
        {
            Dados = dados;
        }

        public List<Configuracao> Dados { get; }
    }
}
