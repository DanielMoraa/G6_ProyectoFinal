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
    public class NotificacionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public NotificacionesController(IConfiguration configuration) => _configuration = configuration;

        [HttpGet("MisNotificaciones/{usuarioId:int}")]
        public IActionResult MisNotificaciones(int usuarioId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var p = new DynamicParameters();
            p.Add("@UsuarioId", usuarioId);

            var resultado = context.Query<NotificacionResponseModel>(
                "dbo.ObtenerNotificacionesPorUsuario",
                p,
                commandType: CommandType.StoredProcedure
            );

            return Ok(resultado);
        }

        [HttpPost("MarcarLeida")]
        public IActionResult MarcarLeida([FromBody] MarcarLeidaRequestModel request)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var p = new DynamicParameters();
            p.Add("@NotificacionId", request.NotificacionId);

            var resp = context.QuerySingle<dynamic>(
                "dbo.MarcarNotificacionLeida",
                p,
                commandType: CommandType.StoredProcedure
            );

            int filas = (int)resp.Filas;
            if (filas <= 0) return NotFound(new { mensaje = "Notificacion no encontrada" });

            return Ok(new { mensaje = "OK", filas });
        }

        [HttpPost("MarcarTodasLeidas")]
        public IActionResult MarcarTodasLeidas([FromBody] MarcarTodasLeidasRequestModel request)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var p = new DynamicParameters();
            p.Add("@UsuarioId", request.UsuarioId);

            var resp = context.QuerySingle<dynamic>(
                "dbo.MarcarTodasNotificacionesLeidas",
                p,
                commandType: CommandType.StoredProcedure
            );

            int filas = (int)resp.Filas;
            return Ok(new { mensaje = "OK", filas });
        }

        [HttpGet("AdminResumen")]
        public IActionResult AdminResumen()
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var resultado = context.Query<NotificacionAdminResumenResponseModel>(
                "dbo.ObtenerNotificacionesAdminResumen",
                commandType: CommandType.StoredProcedure
            );

            return Ok(resultado);
        }

        [HttpPost("CrearMasiva")]
        public IActionResult CrearMasiva([FromBody] CrearNotificacionMasivaRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Destino)) return BadRequest("Destino requerido");
            if (string.IsNullOrWhiteSpace(request.Titulo)) return BadRequest("Titulo requerido");
            if (string.IsNullOrWhiteSpace(request.Contenido)) return BadRequest("Contenido requerido");

            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var p = new DynamicParameters();
            p.Add("@Destino", request.Destino.Trim().ToLower());
            p.Add("@Titulo", request.Titulo.Trim());
            p.Add("@Contenido", request.Contenido.Trim());
            p.Add("@Tipo", string.IsNullOrWhiteSpace(request.Tipo) ? "General" : request.Tipo.Trim());

            int insertadas = context.QuerySingle<int>(
                "dbo.CrearNotificacionMasiva",
                p,
                commandType: CommandType.StoredProcedure
            );

            return Ok(new { mensaje = "OK", insertadas });
        }
    }
}
