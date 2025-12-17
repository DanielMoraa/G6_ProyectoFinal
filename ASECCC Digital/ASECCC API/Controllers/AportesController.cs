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
    public class AportesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AportesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerAportesPorUsuario")]
        public IActionResult ObtenerAportesPorUsuario(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                var query = @"
                    SELECT  aporteId      AS AporteId,
                            usuarioId     AS UsuarioId,
                            tipoAporte    AS TipoAporte,
                            monto         AS Monto,
                            fechaRegistro AS FechaRegistro
                    FROM Aportes
                    WHERE usuarioId = @UsuarioId
                    ORDER BY fechaRegistro DESC";

                var resultado = context.Query<AporteResponseModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerAportesAdmin")]
        public IActionResult ObtenerAportesAdmin(
            string? nombreAsociado,
            string? tipoAporte,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Nombre", nombreAsociado);
                parametros.Add("@TipoAporte", tipoAporte);
                parametros.Add("@FechaDesde", fechaDesde);
                parametros.Add("@FechaHasta", fechaHasta);

                var query = @"
                    SELECT  a.aporteId      AS AporteId,
                            a.usuarioId     AS UsuarioId,
                            a.tipoAporte    AS TipoAporte,
                            a.monto         AS Monto,
                            a.fechaRegistro AS FechaRegistro,
                            u.nombreCompleto AS NombreCompleto,
                            u.identificacion AS Identificacion
                    FROM Aportes a
                    INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                    WHERE   (@Nombre IS NULL OR u.nombreCompleto LIKE '%' + @Nombre + '%')
                    AND     (@TipoAporte IS NULL OR a.tipoAporte = @TipoAporte)
                    AND     (@FechaDesde IS NULL OR a.fechaRegistro >= @FechaDesde)
                    AND     (@FechaHasta IS NULL OR a.fechaRegistro <= @FechaHasta)
                    ORDER BY a.fechaRegistro DESC";

                var resultado = context.Query<AporteAdminResponseModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("CrearAporte")]
        public IActionResult CrearAporte([FromBody] CrearAporteRequestModel request)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", request.UsuarioId);
                parametros.Add("@TipoAporte", request.TipoAporte);
                parametros.Add("@Monto", request.Monto);

                var query = @"
                    INSERT INTO Aportes
                        (usuarioId, tipoAporte, monto, fechaRegistro)
                    VALUES
                        (@UsuarioId, @TipoAporte, @Monto, GETDATE());

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var nuevoId = context.QuerySingle<int>(query, parametros);
                return Ok(nuevoId);
            }
        }
    }
}
