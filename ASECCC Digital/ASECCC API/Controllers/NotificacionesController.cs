using ASECCC_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASECCC_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionModel _model;

        public NotificacionesController(IConfiguration config)
        {
            _model = new NotificacionModel(config);
        }

        [HttpGet("Usuario/{usuarioId:int}")]
        public async Task<ActionResult<List<NotificacionDto>>> Usuario(int usuarioId)
        {
            var lista = await _model.ObtenerNotificacionesPorUsuarioAsync(usuarioId);
            return Ok(lista);
        }

        [HttpPost("MarcarLeida/{notificacionId:int}")]
        public async Task<ActionResult> MarcarLeida(int notificacionId)
        {
            var ok = await _model.MarcarLeidaAsync(notificacionId);

            if (!ok)
                return NotFound();

            return NoContent();
        }

        [HttpPost("MarcarTodas/{usuarioId:int}")]
        public async Task<ActionResult> MarcarTodas(int usuarioId)
        {
            var ok = await _model.MarcarTodasLeidasAsync(usuarioId);

            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
