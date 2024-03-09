using Back.Dominio.DTO;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Azure.CadastrarAzure
{
    public class ResultadoCadastrarAzure : ResultadoControllerDTO
    {
        public List<Dominio.Models.Azure> Dados { get; set; }
    }
}
