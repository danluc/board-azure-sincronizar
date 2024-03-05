using Back.Dominio.DTO;
using Back.Dominio.DTO.Areas;
using System.Collections.Generic;

namespace Back.Servico.Consultas.Times.ListarIterations
{
    public class ResultadoListarIterations : ResultadoControllerDTO
    {
        public ResultadoListarIterations()
        { }

        public ResultadoListarIterations(string msg, bool sucesso = false)
        {
            Sucesso = sucesso;
            Mensagem = msg;
        }

        public List<ListaSprintsDTO> Dados { get; set; }
    }
}
