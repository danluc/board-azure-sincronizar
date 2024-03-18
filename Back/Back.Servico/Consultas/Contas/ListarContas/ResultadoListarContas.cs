using Back.Dominio.DTO;
using Back.Dominio.Models;
using System.Collections.Generic;

namespace Back.Servico.Consultas.Contas.ListarContas
{
    public class ResultadoListarContas : ResultadoControllerDTO
    {
        public List<Conta> Dados { get; set; }
    }
}
