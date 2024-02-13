using Back.Dominio.Models;
using Back.Servico.Comandos.Contas.AtualizarConta;
using Back.Servico.Comandos.Contas.CadastrarConta;
using Back.Servico.Consultas.Contas.ListarContas;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var result = await _mediator.Send(new ParametroListarContas());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }

        [HttpPost("")]
        public async Task<ActionResult> Post(List<Conta> Dados)
        {
            var result = await _mediator.Send(new ParametroCadastrarConta(Dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }

        [HttpPut("")]
        public async Task<ActionResult> Put(List<Conta> Dados)
        {
            var result = await _mediator.Send(new ParametroAtualizarConta(Dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
