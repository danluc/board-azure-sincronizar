using Back.Dominio.DTO;
using Back.Dominio.DTO.Projetos;
using Back.Servico.Consultas.Times.ListarIterations;
using Back.Servico.Consultas.Times.ListarTimes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APP.Controllers
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

        [HttpPost("{projeto}")]
        public async Task<ActionResult> Post(BuscarProjetoDTO dados, string projeto)
        {
            var result = await _mediator.Send(new ParametroListarTimes(projeto, dados.Url, dados.Token));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }

        [HttpPost("BuscarAreas")]
        public async Task<ActionResult> BuscarAreas(BuscarIterationsDTO dados)
        {
            var result = await _mediator.Send(new ParametroListarIterations(dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
