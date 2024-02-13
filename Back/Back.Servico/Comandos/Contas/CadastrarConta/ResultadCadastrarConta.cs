using Back.Dominio.DTO;
using Back.Dominio.Models;
using System.Collections.Generic;

namespace Back.Servico.Comandos.Contas.CadastrarConta
{
    public class ResultadCadastrarConta : ResultadoControllerDTO
    {
        public List<Conta> Dados { get; set; }
    }
}

