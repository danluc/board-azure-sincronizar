using Back.Dominio.DTO;
using Back.Dominio.Models;
using System.Collections.Generic;

namespace Back.Servico.Consultas.Board.ListarSincronizacoes
{
    public class ResultadoListarSincronizacoes : ResultadoControllerDTO
    {
        public List<Sincronizar> Dados { get; set; }
    }
}

