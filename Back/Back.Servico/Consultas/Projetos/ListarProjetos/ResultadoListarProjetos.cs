using Back.Dominio.DTO;
using Microsoft.TeamFoundation.Core.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Back.Servico.Consultas.Projetos.ListarProjetos
{
    public class ResultadoListarProjetos : ResultadoControllerDTO
    {
        public ResultadoListarProjetos()
        {}

        public ResultadoListarProjetos(string msg, bool sucesso = false)
        {
            Sucesso = sucesso;
            Mensagem = msg;
        }

        public List<TeamProjectReference> ProjetosPrincipal { get; set; }
        public List<TeamProjectReference> ProjetosSecundario { get; set; }
    }
}
