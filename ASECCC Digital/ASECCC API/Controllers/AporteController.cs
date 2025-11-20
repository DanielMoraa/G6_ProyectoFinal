using ASECCC_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASECCC_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AportesController : ControllerBase
    {
        private readonly AportesModel _model;

        public AportesController(IConfiguration config)
        {
            _model = new AportesModel(config);
        }

        [HttpGet("MisAportes/{usuarioId:int}")]
        public async Task<ActionResult<List<AporteDto>>> MisAportes(int usuarioId)
        {
            var lista = await _model.ObtenerAportesPorUsuarioAsync(usuarioId);
            return Ok(lista);
        }

        [HttpGet("Gestionar")]
        public async Task<ActionResult<List<AporteDto>>> Gestionar(
            string? nombreAsociado,
            string? tipoAporte,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var lista = await _model.ObtenerAportesAdminAsync(
                nombreAsociado,
                tipoAporte,
                fechaDesde,
                fechaHasta);

            return Ok(lista);
        }

        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] CrearAporteRequest dto)
        {
            if (dto == null || dto.Monto <= 0 || string.IsNullOrWhiteSpace(dto.TipoAporte))
                return BadRequest("Datos inválidos");

            var nuevoId = await _model.CrearAporteAsync(dto);

            return CreatedAtAction(
                nameof(MisAportes),
                new { usuarioId = dto.UsuarioId },
                new { aporteId = nuevoId }
            );
        }
    }
}
