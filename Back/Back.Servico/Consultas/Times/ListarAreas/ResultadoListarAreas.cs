using Back.Dominio.DTO;
using Back.Dominio.DTO.Areas;
using System.Collections.Generic;

namespace Back.Servico.Consultas.Times.ListarAreas
{
    public class ResultadoListarAreas : ResultadoControllerDTO
    {
        public ResultadoListarAreas()
        { }

        public ResultadoListarAreas(string msg, bool sucesso = false)
        {
            Sucesso = sucesso;
            Mensagem = msg;
        }

        public List<AreaDTO> Dados { get; set; }
    }
}
