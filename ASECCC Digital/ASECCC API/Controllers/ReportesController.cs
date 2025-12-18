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
    public class ReportesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ReportesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ResumenFinanciero")]
        public IActionResult ResumenFinanciero(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                var resumen = context.QueryFirstOrDefault<dynamic>(
                    "ResumenFinanciero",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                var resultado = new
                {
                    TotalPrestamos = (decimal?)(resumen?.TotalPrestamos ?? 0m) ?? 0m,
                    CantidadPrestamos = (int?)(resumen?.CantidadPrestamos ?? 0) ?? 0,
                    TotalAhorros = (decimal?)(resumen?.TotalAhorros ?? 0m) ?? 0m,
                    CantidadAhorros = (int?)(resumen?.CantidadAhorros ?? 0) ?? 0,
                    TotalAportes = (decimal?)(resumen?.TotalAportes ?? 0m) ?? 0m,
                    CantidadAportes = (int?)(resumen?.CantidadAportes ?? 0) ?? 0
                };

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("HistorialTransacciones")]
        public IActionResult HistorialTransacciones(int usuarioId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);
                parametros.Add("@FechaInicio", fechaInicio);
                parametros.Add("@FechaFin", fechaFin);

                var resultado = context.Query<dynamic>(
                    "HistorialTransacciones",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("EstadoCuentaDetallado")]
        public IActionResult EstadoCuentaDetallado(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                // Usuario
                var usuario = context.QueryFirstOrDefault<dynamic>(
                    "ObtenerUsuarioEstadoCuenta",
                    new { UsuarioId = usuarioId },
                    commandType: CommandType.StoredProcedure
                );

                if (usuario == null)
                    return NotFound();

                // Préstamos
                var prestamos = context.Query<dynamic>(
                    "ObtenerPrestamosEstadoCuenta",
                    new { UsuarioId = usuarioId },
                    commandType: CommandType.StoredProcedure
                );

                // Ahorros
                var ahorros = context.Query<dynamic>(
                    "ObtenerAhorrosEstadoCuenta",
                    new { UsuarioId = usuarioId },
                    commandType: CommandType.StoredProcedure
                );

                // Aportes
                var aportes = context.Query<dynamic>(
                    "ObtenerAportesEstadoCuenta",
                    new { UsuarioId = usuarioId },
                    commandType: CommandType.StoredProcedure
                );

                var resultado = new
                {
                    Usuario = usuario,
                    Prestamos = prestamos,
                    Ahorros = ahorros,
                    Aportes = aportes
                };

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ReportePrestamos")]
        public IActionResult ReportePrestamos(DateTime? fechaInicio, DateTime? fechaFin, string? estado)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@FechaInicio", fechaInicio);
                parametros.Add("@FechaFin", fechaFin);
                parametros.Add("@Estado", estado);

                var resultado = context.Query<dynamic>(
                    "ReportePrestamos",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ReporteAhorros")]
        public IActionResult ReporteAhorros(DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@FechaInicio", fechaInicio);
                parametros.Add("@FechaFin", fechaFin);

                var resultado = context.Query<dynamic>(
                    "ReporteAhorros",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }
    }
}
