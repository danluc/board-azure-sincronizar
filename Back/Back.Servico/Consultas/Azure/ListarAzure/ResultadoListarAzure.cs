using Back.Dominio.DTO;
using System.Collections.Generic;

namespace Back.Servico.Consultas.Azure.ListarAzure
{
    public class ResultadoListarAzure : ResultadoControllerDTO
    {
        public List<Dominio.Models.Azure> Dados { get; set; }
    }
}
