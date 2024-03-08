using Back.Servico.Comandos.Azure.CadastrarAzure;
using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Azure.AtualizarAzure
{
    public class ParametroAtualizarAzure : IRequest<ResultadoCadastrarAzure>
    {
        public ParametroAtualizarAzure(Dominio.Models.Azure dados)
        {
            Dados = dados;
        }

        public Dominio.Models.Azure Dados { get; }
    }
}
