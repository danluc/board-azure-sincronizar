using Back.Dominio.Models;
using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Contas.CadastrarConta
{
    public class ParametroCadastrarConta : IRequest<ResultadCadastrarConta>
    {
        public ParametroCadastrarConta(List<Conta> dados)
        {
            Dados = dados;
        }

        public List<Conta> Dados { get; }
    }
}
