using Back.Dominio.DTO;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;

namespace Back.Servico.Consultas.Times.ListarTimes
{
    public class ResultadoListarTimes : ResultadoControllerDTO
    {
        public ResultadoListarTimes()
        { }

        public ResultadoListarTimes(string msg, bool sucesso = false)
        {
            Sucesso = sucesso;
            Mensagem = msg;
        }

        public List<WebApiTeam> Dados { get; set; }
    }
}