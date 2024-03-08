using Back.Dominio.Models;
using Back.Servico.Comandos.Azure.AtualizarAzure;
using Back.Servico.Comandos.Azure.CadastrarAzure;
using Back.Servico.Consultas.Azure.ListarAzure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AzureController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AzureController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var result = await _mediator.Send(new ParametroListarAzure());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result.Dados);
        }

        [HttpPost("")]
        public async Task<ActionResult> Post(Azure Dados)
        {
            var result = await _mediator.Send(new ParametroCadastrarAzure(Dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }

        [HttpPut("")]
        public async Task<ActionResult> Put(Azure Dados)
        {
            var result = await _mediator.Send(new ParametroAtualizarAzure(Dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
