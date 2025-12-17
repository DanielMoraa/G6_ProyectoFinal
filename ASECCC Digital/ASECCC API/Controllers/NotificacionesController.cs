using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ASECCC_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public NotificacionesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerNotificacionesPorUsuario")]
        public IActionResult ObtenerNotificacionesPorUsuario(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                var query = @"
                    SELECT  notificacionId AS NotificacionId,
                            usuarioId      AS UsuarioId,
                            titulo         AS Titulo,
                            mensaje        AS Mensaje,
                            fecha          AS Fecha,
                            leido          AS Leido
                    FROM Notificaciones
                    WHERE usuarioId = @UsuarioId
                    ORDER BY fecha DESC";

                var resultado = context.Query<NotificacionResponseModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("MarcarLeida")]
        public IActionResult MarcarLeida([FromBody] MarcarLeidaRequestModel request)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@NotificacionId", request.NotificacionId);

                var query = @"
                    UPDATE Notificaciones
                    SET leido = 1
                    WHERE notificacionId = @NotificacionId";

                var filas = context.Execute(query, parametros);

                if (filas <= 0)
                    return NotFound(new { mensaje = "Notificación no encontrada" });

                return Ok(new { mensaje = "Notificación marcada como leída" });
            }
        }

        [HttpPost]
        [Route("MarcarTodasLeidas")]
        public IActionResult MarcarTodasLeidas([FromBody] MarcarTodasLeidasRequestModel request)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", request.UsuarioId);

                var query = @"
                    UPDATE Notificaciones
                    SET leido = 1
                    WHERE usuarioId = @UsuarioId
                    AND leido = 0";

                var filas = context.Execute(query, parametros);

                if (filas <= 0)
                    return NotFound(new { mensaje = "No hay notificaciones pendientes o usuario no válido" });

                return Ok(new { mensaje = "Todas las notificaciones fueron marcadas como leídas" });
            }
        }
    }
}
