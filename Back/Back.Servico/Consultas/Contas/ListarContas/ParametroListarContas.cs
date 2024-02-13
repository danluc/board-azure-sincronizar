using Back.Servico.Comandos.Contas.CadastrarConta;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Back.Servico.Consultas.Contas.ListarContas
{
    public class ParametroListarContas : IRequest<ResultadCadastrarConta>
    {
        public ParametroListarContas()
        {}
    }
}
