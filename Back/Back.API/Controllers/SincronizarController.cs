using Back.Servico.Comandos.Board.SincronizarBoard;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Back.API.Controllers
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
            var result = await _mediator.Send(new ParametroSincronizarBoard());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
