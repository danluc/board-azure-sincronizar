using Back.Servico.Comandos.Board.SincronizarBoard;
using Back.Servico.Consultas.Board.ListarSincronizacoes;
using Back.Servico.Consultas.Board.UltimaSincronizacao;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SincronizarController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SincronizarController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var result = await _mediator.Send(new ParametroListarSincronizacoes());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
        
        [HttpGet("Ultimo")]
        public async Task<ActionResult> Ultimo()
        {
            var result = await _mediator.Send(new ParametroUltimaSincronizacao());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result.Dados);
        }
        
        [HttpPost("")]
        public async Task<ActionResult> Post()
        {
            var result = await _mediator.Send(new ParametroSincronizarBoard());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
