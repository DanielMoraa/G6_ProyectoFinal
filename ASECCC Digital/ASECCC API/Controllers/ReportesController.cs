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
                // Total Préstamos
                var queryPrestamos = @"
                    SELECT 
                        ISNULL(SUM(montoAprobado), 0) as TotalPrestamos,
                        COUNT(*) as CantidadPrestamos
                    FROM Prestamos 
                    WHERE usuarioId = @UsuarioId AND estadoPrestamo = 'Aprobado'";

                var prestamos = context.QueryFirstOrDefault<dynamic>(queryPrestamos, new { UsuarioId = usuarioId });

                // Total Ahorros
                var queryAhorros = @"
                    SELECT 
                        ISNULL(SUM(montoActual), 0) as TotalAhorros,
                        COUNT(*) as CantidadAhorros
                    FROM Ahorros 
                    WHERE usuarioId = @UsuarioId AND estado = 'Activo'";

                var ahorros = context.QueryFirstOrDefault<dynamic>(queryAhorros, new { UsuarioId = usuarioId });

                // Total Aportes
                var queryAportes = @"
                    SELECT 
                        ISNULL(SUM(monto), 0) as TotalAportes,
                        COUNT(*) as CantidadAportes
                    FROM Aportes 
                    WHERE usuarioId = @UsuarioId";

                var aportes = context.QueryFirstOrDefault<dynamic>(queryAportes, new { UsuarioId = usuarioId });

                var resultado = new
                {
                    TotalPrestamos = (decimal)(prestamos?.TotalPrestamos ?? 0),
                    CantidadPrestamos = (int)(prestamos?.CantidadPrestamos ?? 0),
                    TotalAhorros = (decimal)(ahorros?.TotalAhorros ?? 0),
                    CantidadAhorros = (int)(ahorros?.CantidadAhorros ?? 0),
                    TotalAportes = (decimal)(aportes?.TotalAportes ?? 0),
                    CantidadAportes = (int)(aportes?.CantidadAportes ?? 0)
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
                var query = @"
                    SELECT 
                        'Préstamo' as TipoTransaccion,
                        p.fechaSolicitud as Fecha,
                        p.montoAprobado as Monto,
                        CONCAT('Préstamo ', p.tipoPrestamo) as Descripcion
                    FROM Prestamos p
                    WHERE p.usuarioId = @UsuarioId
                    AND (@FechaInicio IS NULL OR p.fechaSolicitud >= @FechaInicio)
                    AND (@FechaFin IS NULL OR p.fechaSolicitud <= @FechaFin)
                    
                    UNION ALL
                    
                    SELECT 
                        'Abono Préstamo' as TipoTransaccion,
                        pt.fechaPago as Fecha,
                        pt.montoAbonado as Monto,
                        CONCAT('Abono a préstamo #', pt.prestamoId) as Descripcion
                    FROM PrestamosTransacciones pt
                    INNER JOIN Prestamos p ON pt.prestamoId = p.prestamoId
                    WHERE p.usuarioId = @UsuarioId
                    AND (@FechaInicio IS NULL OR pt.fechaPago >= @FechaInicio)
                    AND (@FechaFin IS NULL OR pt.fechaPago <= @FechaFin)
                    
                    UNION ALL
                    
                    SELECT 
                        'Ahorro' as TipoTransaccion,
                        a.fechaInicio as Fecha,
                        a.montoInicial as Monto,
                        CONCAT('Ahorro ', cta.tipoAhorro) as Descripcion
                    FROM Ahorros a
                    INNER JOIN CatalogoTipoAhorro cta ON a.tipoAhorroId = cta.tipoAhorroId
                    WHERE a.usuarioId = @UsuarioId
                    AND (@FechaInicio IS NULL OR a.fechaInicio >= @FechaInicio)
                    AND (@FechaFin IS NULL OR a.fechaInicio <= @FechaFin)
                    
                    UNION ALL
                    
                    SELECT 
                        'Aporte' as TipoTransaccion,
                        ap.fechaRegistro as Fecha,
                        ap.monto as Monto,
                        CONCAT('Aporte ', ap.tipoAporte) as Descripcion
                    FROM Aportes ap
                    WHERE ap.usuarioId = @UsuarioId
                    AND (@FechaInicio IS NULL OR ap.fechaRegistro >= @FechaInicio)
                    AND (@FechaFin IS NULL OR ap.fechaRegistro <= @FechaFin)
                    
                    ORDER BY Fecha DESC";

                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);
                parametros.Add("@FechaInicio", fechaInicio);
                parametros.Add("@FechaFin", fechaFin);

                var resultado = context.Query<dynamic>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("EstadoCuentaDetallado")]
        public IActionResult EstadoCuentaDetallado(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                // Información del usuario
                var queryUsuario = @"
                    SELECT 
                        usuarioId as UsuarioId,
                        nombreCompleto as NombreCompleto,
                        identificacion as Identificacion,
                        correoElectronico as CorreoElectronico,
                        telefono as Telefono,
                        fechaIngreso as FechaIngreso
                    FROM Usuario
                    WHERE usuarioId = @UsuarioId";

                var usuario = context.QueryFirstOrDefault<dynamic>(queryUsuario, new { UsuarioId = usuarioId });

                if (usuario == null)
                    return NotFound();

                // Préstamos
                var queryPrestamos = @"
                    SELECT 
                        prestamoId as PrestamoId,
                        tipoPrestamo as TipoPrestamo,
                        montoAprobado as MontoAprobado,
                        saldoPendiente as SaldoPendiente,
                        cuotaSemanal as CuotaSemanal,
                        estadoPrestamo as EstadoPrestamo
                    FROM Prestamos
                    WHERE usuarioId = @UsuarioId AND estadoPrestamo = 'Aprobado'";

                var prestamos = context.Query<dynamic>(queryPrestamos, new { UsuarioId = usuarioId });

                // Ahorros - CORREGIDO CON JOIN
                var queryAhorros = @"
                    SELECT 
                        a.ahorroId as AhorroId,
                        cta.tipoAhorro as TipoAhorro,
                        a.montoInicial as MontoInicial,
                        a.montoActual as MontoActual,
                        a.fechaInicio as FechaInicio,
                        a.plazo as Plazo,
                        a.estado as Estado
                    FROM Ahorros a
                    INNER JOIN CatalogoTipoAhorro cta ON a.tipoAhorroId = cta.tipoAhorroId
                    WHERE a.usuarioId = @UsuarioId AND a.estado = 'Activo'";

                var ahorros = context.Query<dynamic>(queryAhorros, new { UsuarioId = usuarioId });

                // Aportes
                var queryAportes = @"
                    SELECT 
                        aporteId as AporteId,
                        tipoAporte as TipoAporte,
                        monto as Monto,
                        fechaRegistro as FechaRegistro
                    FROM Aportes
                    WHERE usuarioId = @UsuarioId
                    ORDER BY fechaRegistro DESC";

                var aportes = context.Query<dynamic>(queryAportes, new { UsuarioId = usuarioId });

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
                var query = @"
                    SELECT 
                        p.prestamoId as PrestamoId,
                        u.nombreCompleto as NombreCompleto,
                        u.identificacion as Identificacion,
                        p.tipoPrestamo as TipoPrestamo,
                        p.montoAprobado as MontoAprobado,
                        p.saldoPendiente as SaldoPendiente,
                        p.cuotaSemanal as CuotaSemanal,
                        p.plazo as Plazo,
                        p.estadoPrestamo as EstadoPrestamo,
                        p.fechaSolicitud as FechaSolicitud
                    FROM Prestamos p
                    INNER JOIN Usuario u ON p.usuarioId = u.usuarioId
                    WHERE 1=1
                    AND (@FechaInicio IS NULL OR p.fechaSolicitud >= @FechaInicio)
                    AND (@FechaFin IS NULL OR p.fechaSolicitud <= @FechaFin)
                    AND (@Estado IS NULL OR @Estado = '' OR p.estadoPrestamo = @Estado)
                    ORDER BY p.fechaSolicitud DESC";

                var parametros = new DynamicParameters();
                parametros.Add("@FechaInicio", fechaInicio);
                parametros.Add("@FechaFin", fechaFin);
                parametros.Add("@Estado", estado);

                var resultado = context.Query<dynamic>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ReporteAhorros")]
        public IActionResult ReporteAhorros(DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                // CORREGIDO: Agregado JOIN con CatalogoTipoAhorro
                var query = @"
                    SELECT 
                        a.ahorroId as AhorroId,
                        u.nombreCompleto as NombreCompleto,
                        u.identificacion as Identificacion,
                        cta.tipoAhorro as TipoAhorro,
                        a.montoInicial as MontoInicial,
                        a.montoActual as MontoActual,
                        a.fechaInicio as FechaInicio,
                        a.plazo as Plazo,
                        a.estado as Estado
                    FROM Ahorros a
                    INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                    INNER JOIN CatalogoTipoAhorro cta ON a.tipoAhorroId = cta.tipoAhorroId
                    WHERE 1=1
                    AND (@FechaInicio IS NULL OR a.fechaInicio >= @FechaInicio)
                    AND (@FechaFin IS NULL OR a.fechaInicio <= @FechaFin)
                    ORDER BY a.fechaInicio DESC";

                var parametros = new DynamicParameters();
                parametros.Add("@FechaInicio", fechaInicio);
                parametros.Add("@FechaFin", fechaFin);

                var resultado = context.Query<dynamic>(query, parametros);
                return Ok(resultado);
            }
        }
    }
}