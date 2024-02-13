using Back.Servico.Consultas.Projetos.ListarProjetos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjetosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var result = await _mediator.Send(new ParametroListarProjetos());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
