﻿using Back.Dominio.DTO;
using MediatR;

namespace Back.Servico.Consultas.Times.ListarIterations
{
    public class ParametroListarIterations : IRequest<ResultadoListarIterations>
    {
        public ParametroListarIterations(BuscarIterationsDTO dados)
        {
            Dados = dados;
        }

        public BuscarIterationsDTO Dados { get; }
    }
}
