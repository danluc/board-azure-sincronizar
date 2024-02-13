using Back.Servico.Consultas.Times.ListarTimes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Back.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TimesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{time}")]
        public async Task<ActionResult> Get(string time)
        {
            var result = await _mediator.Send(new ParametroListarTimes(time));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
