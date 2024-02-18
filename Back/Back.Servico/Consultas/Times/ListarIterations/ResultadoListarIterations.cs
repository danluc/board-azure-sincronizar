using Back.Dominio.DTO;
using Microsoft.TeamFoundation.Work.WebApi;
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

        public List<TeamSettingsIteration> Dados { get; set; }
    }
}
