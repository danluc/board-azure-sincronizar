using Back.Dominio.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Back.Servico.Comandos.Board.SincronizarBoard
{
    public class ResultadoSincronizarBoard : ResultadoControllerDTO
    {
        public ResultadoSincronizarBoard()
        { }

        public ResultadoSincronizarBoard(string msg, bool sucesso = false)
        {
            Sucesso = sucesso;
            Mensagem = msg;
        }
    }
}

