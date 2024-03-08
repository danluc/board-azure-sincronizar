using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Azure.CadastrarAzure
{
    public class ParametroCadastrarAzure : IRequest<ResultadoCadastrarAzure>
    {
        public ParametroCadastrarAzure(Dominio.Models.Azure dados)
        {
            Dados = dados;
        }

        public Dominio.Models.Azure Dados { get; }
    }
}
