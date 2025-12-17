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
    public class AhorrosController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AhorrosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        [Route("ObtenerAhorrosPorUsuario")]
        public IActionResult ObtenerAhorrosPorUsuario(int usuarioId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", usuarioId);

            var query = @"
                SELECT  a.ahorroId      AS AhorroId,
                        a.usuarioId     AS UsuarioId,
                        a.tipoAhorroId  AS TipoAhorroId,
                        c.tipoAhorro    AS TipoAhorro,
                        a.montoInicial  AS MontoInicial,
                        a.montoActual   AS MontoActual,
                        a.fechaInicio   AS FechaInicio,
                        a.plazo         AS Plazo,
                        a.estado        AS Estado,
                        ''              AS NombreAsociado,
                        ''              AS Identificacion
                FROM Ahorros a
                INNER JOIN CatalogoTipoAhorro c ON a.tipoAhorroId = c.tipoAhorroId
                WHERE a.usuarioId = @UsuarioId
                ORDER BY a.fechaInicio DESC, a.ahorroId DESC;";

            var resultado = context.Query<AhorroResponseModel>(query, parametros);
            return Ok(resultado);
        }


        [HttpGet]
        [Route("ObtenerAhorrosAdmin")]
        public IActionResult ObtenerAhorrosAdmin(string? nombreAsociado, int? tipoAhorroId, string? estado)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var nombre = string.IsNullOrWhiteSpace(nombreAsociado) ? null : nombreAsociado;
            var est = string.IsNullOrWhiteSpace(estado) ? null : estado;

            var parametros = new DynamicParameters();
            parametros.Add("@Nombre", nombre);
            parametros.Add("@TipoAhorroId", tipoAhorroId);
            parametros.Add("@Estado", est);

            var query = @"
                SELECT  a.ahorroId      AS AhorroId,
                        a.usuarioId     AS UsuarioId,
                        a.tipoAhorroId  AS TipoAhorroId,
                        c.tipoAhorro    AS TipoAhorro,
                        a.montoInicial  AS MontoInicial,
                        a.montoActual   AS MontoActual,
                        a.fechaInicio   AS FechaInicio,
                        a.plazo         AS Plazo,
                        a.estado        AS Estado,
                        u.nombreCompleto AS NombreAsociado,
                        u.identificacion AS Identificacion
                FROM Ahorros a
                INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                INNER JOIN CatalogoTipoAhorro c ON a.tipoAhorroId = c.tipoAhorroId
                WHERE   (@Nombre IS NULL OR u.nombreCompleto LIKE '%' + @Nombre + '%')
                AND     (@TipoAhorroId IS NULL OR a.tipoAhorroId = @TipoAhorroId)
                AND     (@Estado IS NULL OR a.estado = @Estado)
                ORDER BY a.fechaInicio DESC, a.ahorroId DESC;";

            var resultado = context.Query<AhorroResponseModel>(query, parametros);
            return Ok(resultado);
        }


        [HttpGet]
        [Route("ObtenerDetalleAhorro")]
        public IActionResult ObtenerDetalleAhorro(int ahorroId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);

            var query = @"
                SELECT  a.ahorroId      AS AhorroId,
                        a.usuarioId     AS UsuarioId,
                        a.tipoAhorroId  AS TipoAhorroId,
                        c.tipoAhorro    AS TipoAhorro,
                        a.montoInicial  AS MontoInicial,
                        a.montoActual   AS MontoActual,
                        a.fechaInicio   AS FechaInicio,
                        a.plazo         AS Plazo,
                        a.estado        AS Estado,
                        u.nombreCompleto AS NombreAsociado,
                        u.identificacion AS Identificacion
                FROM Ahorros a
                INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                INNER JOIN CatalogoTipoAhorro c ON a.tipoAhorroId = c.tipoAhorroId
                WHERE a.ahorroId = @AhorroId;";

            var resultado = context.QueryFirstOrDefault<AhorroResponseModel>(query, parametros);

            if (resultado == null) return NotFound("No existe el ahorro.");
            return Ok(resultado);
        }


        [HttpGet]
        [Route("ObtenerTransaccionesAhorro")]
        public IActionResult ObtenerTransaccionesAhorro(int ahorroId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);

            var query = @"
                SELECT  t.transaccionAhorroId AS TransaccionId,
                        t.ahorroId           AS AhorroId,
                        t.fechaTransaccion   AS Fecha,
                        ct.tipoTransaccion   AS Tipo,
                        t.monto              AS Monto,
                        t.descripcion        AS Descripcion
                FROM AhorroTransacciones t
                INNER JOIN CatalogoTipoTransaccion ct ON t.tipoTransaccionId = ct.tipoTransaccionId
                WHERE t.ahorroId = @AhorroId
                ORDER BY t.fechaTransaccion DESC, t.transaccionAhorroId DESC;";

            var resultado = context.Query<AhorroTransaccionResponseModel>(query, parametros);
            return Ok(resultado);
        }

        [HttpPost]
        [Route("CrearAhorro")]
        public IActionResult CrearAhorro([FromBody] CrearAhorroRequest request)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", request.UsuarioId);
            parametros.Add("@TipoAhorroId", request.TipoAhorroId);
            parametros.Add("@MontoInicial", request.MontoInicial);
            parametros.Add("@Plazo", request.Plazo);

            var query = @"
                INSERT INTO Ahorros
                    (usuarioId, tipoAhorroId, montoInicial, montoActual, fechaInicio, plazo, estado)
                VALUES
                    (@UsuarioId, @TipoAhorroId, @MontoInicial, @MontoInicial, GETDATE(), @Plazo, 'Activo');

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var nuevoId = context.QuerySingle<int>(query, parametros);
            return Ok(nuevoId);
        }

        [HttpPut]
        [Route("ModificarMontoAhorro")]
        public IActionResult ModificarMontoAhorro(int ahorroId, decimal nuevoMonto)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);
            parametros.Add("@NuevoMonto", nuevoMonto);

            var query = @"
                UPDATE Ahorros
                SET montoInicial = @NuevoMonto
                WHERE ahorroId = @AhorroId;";

            var filas = context.Execute(query, parametros);
            if (filas == 0) return NotFound("No existe el ahorro.");

            return Ok(filas);
        }

        [HttpGet]
        [Route("ObtenerCatalogoTipoAhorro")]
        public IActionResult ObtenerCatalogoTipoAhorro()
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var query = @"
                SELECT  tipoAhorroId AS TipoAhorroId,
                        tipoAhorro   AS TipoAhorro
                FROM CatalogoTipoAhorro
                ORDER BY tipoAhorro;";

            var resultado = context.Query<CatalogoTipoAhorroResponseModel>(query);
            return Ok(resultado);
        }


        [HttpDelete]
        [Route("EliminarAhorro")]
        public IActionResult EliminarAhorro(int ahorroId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);

            var query = @"DELETE FROM Ahorros WHERE ahorroId = @AhorroId;";

            var filas = context.Execute(query, parametros);
            if (filas == 0) return NotFound("No existe el ahorro.");

            return Ok(filas);
        }
    }
}
