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

                // Total Préstamos
                var queryPrestamos = @"SELECT 
                                      ISNULL(SUM(saldoPendiente), 0) as TotalPrestamos,
                                      COUNT(*) as CantidadPrestamos
                                      FROM Prestamos 
                                      WHERE usuarioId = @UsuarioId 
                                      AND estadoPrestamo = 'Aprobado'";

                var prestamos = context.QueryFirstOrDefault<dynamic>(queryPrestamos, parametros);

                // Total Ahorros
                var queryAhorros = @"SELECT 
                                    ISNULL(SUM(montoActual), 0) as TotalAhorros,
                                    COUNT(*) as CantidadAhorros
                                    FROM Ahorros 
                                    WHERE usuarioId = @UsuarioId 
                                    AND estado = 'Activo'";

                var ahorros = context.QueryFirstOrDefault<dynamic>(queryAhorros, parametros);

                // Total Aportes
                var queryAportes = @"SELECT 
                                    ISNULL(SUM(monto), 0) as TotalAportes,
                                    COUNT(*) as CantidadAportes
                                    FROM Aportes 
                                    WHERE usuarioId = @UsuarioId";

                var aportes = context.QueryFirstOrDefault<dynamic>(queryAportes, parametros);

                var resultado = new
                {
                    TotalPrestamos = prestamos.TotalPrestamos,
                    CantidadPrestamos = prestamos.CantidadPrestamos,
                    TotalAhorros = ahorros.TotalAhorros,
                    CantidadAhorros = ahorros.CantidadAhorros,
                    TotalAportes = aportes.TotalAportes,
                    CantidadAportes = aportes.CantidadAportes
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
                parametros.Add("@FechaInicio", fechaInicio ?? DateTime.Now.AddMonths(-3));
                parametros.Add("@FechaFin", fechaFin ?? DateTime.Now);

                var query = @"
                    SELECT 'Préstamo' as TipoTransaccion, 
                           pt.fechaPago as Fecha, 
                           pt.montoAbonado as Monto,
                           'Abono a préstamo' as Descripcion
                    FROM PrestamosTransacciones pt
                    INNER JOIN Prestamos p ON pt.prestamoId = p.prestamoId
                    WHERE p.usuarioId = @UsuarioId 
                    AND pt.fechaPago BETWEEN @FechaInicio AND @FechaFin

                    UNION ALL

                    SELECT 'Ahorro' as TipoTransaccion,
                           at.fechaTransaccion as Fecha,
                           at.monto as Monto,
                           at.descripcion as Descripcion
                    FROM AhorroTransacciones at
                    INNER JOIN Ahorros a ON at.ahorroId = a.ahorroId
                    WHERE a.usuarioId = @UsuarioId
                    AND at.fechaTransaccion BETWEEN @FechaInicio AND @FechaFin

                    UNION ALL

                    SELECT 'Aporte' as TipoTransaccion,
                           apt.fechaTransaccion as Fecha,
                           apt.monto as Monto,
                           apt.descripcion as Descripcion
                    FROM AportesTransacciones apt
                    INNER JOIN Aportes ap ON apt.aporteId = ap.aporteId
                    WHERE ap.usuarioId = @UsuarioId
                    AND apt.fechaTransaccion BETWEEN @FechaInicio AND @FechaFin

                    ORDER BY Fecha DESC";

                var resultado = context.Query<TransaccionReporteModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("EstadoCuentaDetallado")]
        public IActionResult EstadoCuentaDetallado(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                // Información del Usuario
                var queryUsuario = @"SELECT usuarioId, nombreCompleto, identificacion, 
                                    correoElectronico, telefono, fechaIngreso
                                    FROM Usuario WHERE usuarioId = @UsuarioId";

                var usuario = context.QueryFirstOrDefault<UsuarioReporteModel>(queryUsuario, parametros);

                // Préstamos Activos
                var queryPrestamos = @"SELECT prestamoId, tipoPrestamo, montoAprobado, 
                                      saldoPendiente, cuotaSemanal, estadoPrestamo
                                      FROM Prestamos 
                                      WHERE usuarioId = @UsuarioId 
                                      AND estadoPrestamo IN ('Aprobado', 'Activo')";

                var prestamos = context.Query<PrestamoReporteModel>(queryPrestamos, parametros);

                // Ahorros Activos
                var queryAhorros = @"SELECT a.ahorroId, cta.tipoAhorro, a.montoInicial, 
                                    a.montoActual, a.fechaInicio, a.plazo, a.estado
                                    FROM Ahorros a
                                    INNER JOIN CatalogoTipoAhorro cta ON a.tipoAhorroId = cta.tipoAhorroId
                                    WHERE a.usuarioId = @UsuarioId 
                                    AND a.estado = 'Activo'";

                var ahorros = context.Query<AhorroReporteModel>(queryAhorros, parametros);

                // Aportes
                var queryAportes = @"SELECT aporteId, tipoAporte, monto, fechaRegistro
                                    FROM Aportes 
                                    WHERE usuarioId = @UsuarioId";

                var aportes = context.Query<AporteReporteModel>(queryAportes, parametros);

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
                parametros.Add("@FechaInicio", fechaInicio ?? DateTime.Now.AddMonths(-12));
                parametros.Add("@FechaFin", fechaFin ?? DateTime.Now);

                var whereClause = "WHERE p.fechaSolicitud BETWEEN @FechaInicio AND @FechaFin";
                
                if (!string.IsNullOrEmpty(estado))
                {
                    whereClause += " AND p.estadoPrestamo = @Estado";
                    parametros.Add("@Estado", estado);
                }

                var query = $@"SELECT p.prestamoId, u.nombreCompleto, u.identificacion,
                              p.tipoPrestamo, p.montoAprobado, p.saldoPendiente,
                              p.cuotaSemanal, p.plazo, p.estadoPrestamo, p.fechaSolicitud
                              FROM Prestamos p
                              INNER JOIN Usuario u ON p.usuarioId = u.usuarioId
                              {whereClause}
                              ORDER BY p.fechaSolicitud DESC";

                var resultado = context.Query<PrestamoReporteDetalladoModel>(query, parametros);
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
                parametros.Add("@FechaInicio", fechaInicio ?? DateTime.Now.AddMonths(-12));
                parametros.Add("@FechaFin", fechaFin ?? DateTime.Now);

                var query = @"SELECT a.ahorroId, u.nombreCompleto, u.identificacion,
                             cta.tipoAhorro, a.montoInicial, a.montoActual,
                             a.fechaInicio, a.plazo, a.estado
                             FROM Ahorros a
                             INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                             INNER JOIN CatalogoTipoAhorro cta ON a.tipoAhorroId = cta.tipoAhorroId
                             WHERE a.fechaInicio BETWEEN @FechaInicio AND @FechaFin
                             ORDER BY a.fechaInicio DESC";

                var resultado = context.Query<AhorroReporteDetalladoModel>(query, parametros);
                return Ok(resultado);
            }
        }
    }
}