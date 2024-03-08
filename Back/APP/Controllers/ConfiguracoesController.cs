using Back.Dominio.Models;
using Back.Servico.Comandos.Configuracoes.AtualizarConfiguracao;
using Back.Servico.Comandos.Configuracoes.CadastrarConfiguracao;
using Back.Servico.Consultas.Configuracoes.ListarConfiguracao;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracoesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConfiguracoesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get()
        {
            var result = await _mediator.Send(new ParametroListarConfiguracao());
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result.Dados);
        }

        [HttpPost("")]
        public async Task<ActionResult> Post(List<Configuracao> Dados)
        {
            var result = await _mediator.Send(new ParametroCadastrarConfiguracao(Dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }

        [HttpPut("")]
        public async Task<ActionResult> Put(List<Configuracao> Dados)
        {
            var result = await _mediator.Send(new ParametroAtualizarConfiguracao(Dados));
            if (!result.Sucesso)
                return BadRequest(result.Mensagem);

            return Ok(result);
        }
    }
}
