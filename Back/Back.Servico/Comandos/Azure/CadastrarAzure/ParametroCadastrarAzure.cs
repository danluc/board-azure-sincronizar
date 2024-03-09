using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Azure.CadastrarAzure
{
    public class ParametroCadastrarAzure : IRequest<ResultadoCadastrarAzure>
    {
        public ParametroCadastrarAzure(List<Dominio.Models.Azure> dados)
        {
            Dados = dados;
        }

        public List<Dominio.Models.Azure> Dados { get; }
    }
}
