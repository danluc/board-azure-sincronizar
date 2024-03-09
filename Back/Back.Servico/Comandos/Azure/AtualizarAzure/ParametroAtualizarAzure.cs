using Back.Servico.Comandos.Azure.CadastrarAzure;
using MediatR;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Azure.AtualizarAzure
{
    public class ParametroAtualizarAzure : IRequest<ResultadoCadastrarAzure>
    {
        public ParametroAtualizarAzure(List<Dominio.Models.Azure> dados)
        {
            Dados = dados;
        }

        public List<Dominio.Models.Azure> Dados { get; }
    }
}
