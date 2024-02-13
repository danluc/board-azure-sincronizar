using Back.Dominio.DTO;
using Back.Dominio.Models;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao
{
    public class ResultadoCadastrarConfiguracao : ResultadoControllerDTO
    {
        public List<Configuracao> Dados { get; set; }
    }
}
