using Back.Dominio.Models;
using Back.Servico.Comandos.Contas.CadastrarConta;
using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Contas.AtualizarConta
{
    public class ParametroAtualizarConta : IRequest<ResultadCadastrarConta>
    {
        public ParametroAtualizarConta(List<Conta> dados)
        {
            Dados = dados;
        }

        public List<Conta> Dados { get; }
    }
}
