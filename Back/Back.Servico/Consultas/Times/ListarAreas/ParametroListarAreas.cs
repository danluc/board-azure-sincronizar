using Back.Dominio.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Back.Servico.Consultas.Times.ListarAreas
{
    public class ParametroListarAreas : IRequest<ResultadoListarAreas>
    {
        public ParametroListarAreas(BuscarIterationsDTO dados)
        {
            Dados = dados;
        }

        public BuscarIterationsDTO Dados { get; }
    }
}
