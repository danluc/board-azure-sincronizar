using Back.Dominio.DTO;
using Back.Dominio.Models;

namespace Back.Servico.Consultas.Board.UltimaSincronizacao
{
    public class ResultadoUltimaSincronizacao : ResultadoControllerDTO
    {
        public Sincronizar Dados { get; set; }
    }
}

