using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ASECCC_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AportesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AportesController(IConfiguration configuration) => _configuration = configuration;

        [HttpGet("MisAportes/{usuarioId:int}")]
        public IActionResult MisAportes(int usuarioId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", usuarioId);

            var resultado = context.Query<AporteResponseModel>(
                "dbo.ObtenerAportesPorUsuario",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return Ok(resultado);
        }

        [HttpGet("Gestionar")]
        public IActionResult Gestionar(string? nombreAsociado, string? tipoAporte, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@Nombre", string.IsNullOrWhiteSpace(nombreAsociado) ? null : nombreAsociado);
            parametros.Add("@TipoAporte", string.IsNullOrWhiteSpace(tipoAporte) ? null : tipoAporte);
            parametros.Add("@FechaDesde", fechaDesde);
            parametros.Add("@FechaHasta", fechaHasta);

            var resultado = context.Query<AporteAdminResponseModel>(
                "dbo.ObtenerAportesAdmin",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return Ok(resultado);
        }

        [HttpPost]
        public IActionResult Crear([FromBody] CrearAporteRequestModel request)
        {
            if (request == null) return BadRequest("Request invalido");
            if (request.UsuarioId <= 0) return BadRequest("UsuarioId invalido");
            if (string.IsNullOrWhiteSpace(request.TipoAporte)) return BadRequest("TipoAporte requerido");
            if (request.Monto <= 0) return BadRequest("Monto invalido");

            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", request.UsuarioId);
            parametros.Add("@TipoAporte", request.TipoAporte.Trim());
            parametros.Add("@Monto", request.Monto);

            var nuevoId = context.QuerySingle<int>(
                "dbo.CrearAporte",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return Ok(nuevoId);
        }
    }
}
