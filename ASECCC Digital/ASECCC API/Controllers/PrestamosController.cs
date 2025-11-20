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
    public class PrestamosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public PrestamosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerPrestamosPorUsuario")]
        public IActionResult ObtenerPrestamosPorUsuario(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                var query = @"SELECT prestamoId, usuarioId, montoAprobado, plazo, cuotaSemanal, 
                             tipoPrestamo, estadoPrestamo, fechaSolicitud, fechaEstado, 
                             saldoPendiente, observaciones 
                             FROM Prestamos 
                             WHERE usuarioId = @UsuarioId 
                             ORDER BY fechaSolicitud DESC";

                var resultado = context.Query<PrestamoResponseModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerDetallePrestamo")]
        public IActionResult ObtenerDetallePrestamo(int prestamoId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@PrestamoId", prestamoId);

                var query = @"SELECT p.prestamoId, p.usuarioId, p.montoAprobado, p.plazo, 
                             p.cuotaSemanal, p.tipoPrestamo, p.estadoPrestamo, 
                             p.fechaSolicitud, p.fechaEstado, p.saldoPendiente, p.observaciones,
                             u.nombreCompleto, u.identificacion
                             FROM Prestamos p
                             INNER JOIN Usuario u ON p.usuarioId = u.usuarioId
                             WHERE p.prestamoId = @PrestamoId";

                var resultado = context.QueryFirstOrDefault<PrestamoDetalleResponseModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerTransaccionesPrestamo")]
        public IActionResult ObtenerTransaccionesPrestamo(int prestamoId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@PrestamoId", prestamoId);

                var query = @"SELECT transaccionPrestamoId, prestamoId, montoAbonado, fechaPago 
                             FROM PrestamosTransacciones 
                             WHERE prestamoId = @PrestamoId 
                             ORDER BY fechaPago DESC";

                var resultado = context.Query<PrestamoTransaccionResponseModel>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("CrearSolicitudPrestamo")]
        public IActionResult CrearSolicitudPrestamo(SolicitudPrestamoRequestModel solicitud)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", solicitud.UsuarioId);
                parametros.Add("@EstadoCivil", solicitud.EstadoCivil);
                parametros.Add("@PagaAlquiler", solicitud.PagaAlquiler);
                parametros.Add("@MontoAlquiler", solicitud.MontoAlquiler);
                parametros.Add("@NombreAcreedor", solicitud.NombreAcreedor);
                parametros.Add("@TotalCredito", solicitud.TotalCredito);
                parametros.Add("@AbonoSemanal", solicitud.AbonoSemanal);
                parametros.Add("@SaldoCredito", solicitud.SaldoCredito);
                parametros.Add("@NombreDeudor", solicitud.NombreDeudor);
                parametros.Add("@TotalPrestamo", solicitud.TotalPrestamo);
                parametros.Add("@SaldoPrestamo", solicitud.SaldoPrestamo);
                parametros.Add("@TipoPrestamo", solicitud.TipoPrestamo);
                parametros.Add("@MontoSolicitud", solicitud.MontoSolicitud);
                parametros.Add("@CuotaSemanalSolicitud", solicitud.CuotaSemanalSolicitud);
                parametros.Add("@PlazoMeses", solicitud.PlazoMeses);
                parametros.Add("@PropositoPrestamo", solicitud.PropositoPrestamo);

                var query = @"INSERT INTO SolicitudesPrestamo 
                             (usuarioId, estadoCivil, pagaAlquiler, montoAlquiler, nombreAcreedor, 
                              totalCredito, abonoSemanal, saldoCredito, nombreDeudor, totalPrestamo, 
                              saldoPrestamo, tipoPrestamo, montoSolicitud, estadoSolicitud, 
                              cuotaSemanalSolicitud, plazoMeses, propositoPrestamo, fechaSolicitud)
                             VALUES 
                             (@UsuarioId, @EstadoCivil, @PagaAlquiler, @MontoAlquiler, @NombreAcreedor, 
                              @TotalCredito, @AbonoSemanal, @SaldoCredito, @NombreDeudor, @TotalPrestamo, 
                              @SaldoPrestamo, @TipoPrestamo, @MontoSolicitud, 'Pendiente', 
                              @CuotaSemanalSolicitud, @PlazoMeses, @PropositoPrestamo, GETDATE());
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                var resultado = context.QuerySingle<int>(query, parametros);
                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("RegistrarAbonoPrestamo")]
        public IActionResult RegistrarAbonoPrestamo(AbonoPrestamoRequestModel abono)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                context.Open();
                using (var transaction = context.BeginTransaction())
                {
                    try
                    {
                        // Registrar transacción
                        var parametrosTransaccion = new DynamicParameters();
                        parametrosTransaccion.Add("@PrestamoId", abono.PrestamoId);
                        parametrosTransaccion.Add("@MontoAbonado", abono.MontoAbonado);

                        var queryTransaccion = @"INSERT INTO PrestamosTransacciones 
                                                (prestamoId, montoAbonado, fechaPago)
                                                VALUES (@PrestamoId, @MontoAbonado, GETDATE())";

                        context.Execute(queryTransaccion, parametrosTransaccion, transaction);

                        // Actualizar saldo del préstamo
                        var parametrosActualizar = new DynamicParameters();
                        parametrosActualizar.Add("@PrestamoId", abono.PrestamoId);
                        parametrosActualizar.Add("@MontoAbonado", abono.MontoAbonado);

                        var queryActualizar = @"UPDATE Prestamos 
                                               SET saldoPendiente = saldoPendiente - @MontoAbonado,
                                                   estadoPrestamo = CASE 
                                                       WHEN (saldoPendiente - @MontoAbonado) <= 0 THEN 'Pagado'
                                                       ELSE estadoPrestamo 
                                                   END
                                               WHERE prestamoId = @PrestamoId";

                        var resultado = context.Execute(queryActualizar, parametrosActualizar, transaction);

                        transaction.Commit();
                        return Ok(resultado);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return BadRequest("Error al registrar el abono");
                    }
                }
            }
        }

        [HttpGet]
        [Route("ObtenerSolicitudesPendientes")]
        public IActionResult ObtenerSolicitudesPendientes()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var query = @"SELECT sp.solicitudPrestamoId, sp.usuarioId, u.nombreCompleto, 
                             sp.tipoPrestamo, sp.montoSolicitud, sp.estadoSolicitud, 
                             sp.fechaSolicitud, sp.plazoMeses
                             FROM SolicitudesPrestamo sp
                             INNER JOIN Usuario u ON sp.usuarioId = u.usuarioId
                             WHERE sp.estadoSolicitud = 'Pendiente'
                             ORDER BY sp.fechaSolicitud DESC";

                var resultado = context.Query<SolicitudPrestamoResponseModel>(query);
                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("AprobarSolicitudPrestamo")]
        public IActionResult AprobarSolicitudPrestamo(AprobarSolicitudRequestModel solicitud)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                context.Open();
                using (var transaction = context.BeginTransaction())
                {
                    try
                    {
                        // Obtener datos de la solicitud
                        var parametrosSolicitud = new DynamicParameters();
                        parametrosSolicitud.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        var querySolicitud = @"SELECT * FROM SolicitudesPrestamo 
                                              WHERE solicitudPrestamoId = @SolicitudId";

                        var solicitudData = context.QueryFirstOrDefault<SolicitudPrestamoCompleta>(
                            querySolicitud, parametrosSolicitud, transaction);

                        if (solicitudData == null)
                            return NotFound(new { mensaje = "Solicitud no encontrada" });

                        // Verificar que la solicitud esté en estado Pendiente
                        if (solicitudData.EstadoSolicitud != "Pendiente")
                            return BadRequest(new { mensaje = $"La solicitud ya fue {solicitudData.EstadoSolicitud.ToLower()}" });

                        // Crear préstamo
                        var parametrosPrestamo = new DynamicParameters();
                        parametrosPrestamo.Add("@UsuarioId", solicitudData.UsuarioId);
                        parametrosPrestamo.Add("@MontoAprobado", solicitudData.MontoSolicitud);
                        parametrosPrestamo.Add("@Plazo", solicitudData.PlazoMeses);
                        parametrosPrestamo.Add("@CuotaSemanal", solicitudData.CuotaSemanalSolicitud);
                        parametrosPrestamo.Add("@TipoPrestamo", solicitudData.TipoPrestamo);

                        var queryPrestamo = @"INSERT INTO Prestamos 
                                             (usuarioId, montoAprobado, plazo, cuotaSemanal, tipoPrestamo, 
                                              estadoPrestamo, fechaSolicitud, fechaEstado, saldoPendiente)
                                             VALUES 
                                             (@UsuarioId, @MontoAprobado, @Plazo, @CuotaSemanal, @TipoPrestamo, 
                                              'Aprobado', GETDATE(), GETDATE(), @MontoAprobado);
                                             SELECT CAST(SCOPE_IDENTITY() as int)";

                        var prestamoId = context.QuerySingle<int>(queryPrestamo, parametrosPrestamo, transaction);

                        // Actualizar estado de solicitud
                        var parametrosActualizar = new DynamicParameters();
                        parametrosActualizar.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        var queryActualizar = @"UPDATE SolicitudesPrestamo 
                                               SET estadoSolicitud = 'Aprobada', 
                                                   fechaAprobacion = GETDATE()
                                               WHERE solicitudPrestamoId = @SolicitudId";

                        context.Execute(queryActualizar, parametrosActualizar, transaction);

                        transaction.Commit();
                        return Ok(new { 
                            mensaje = "Préstamo aprobado exitosamente", 
                            prestamoId = prestamoId,
                            montoAprobado = solicitudData.MontoSolicitud,
                            usuarioId = solicitudData.UsuarioId
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(new { mensaje = $"Error al aprobar el préstamo: {ex.Message}" });
                    }
                }
            }
        }

        [HttpPost]
        [Route("RechazarSolicitudPrestamo")]
        public IActionResult RechazarSolicitudPrestamo(RechazarSolicitudRequestModel solicitud)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                context.Open();
                using (var transaction = context.BeginTransaction())
                {
                    try
                    {
                        // Verificar que la solicitud existe y está pendiente
                        var parametrosVerificar = new DynamicParameters();
                        parametrosVerificar.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        var queryVerificar = @"SELECT estadoSolicitud FROM SolicitudesPrestamo 
                                              WHERE solicitudPrestamoId = @SolicitudId";

                        var estadoActual = context.QueryFirstOrDefault<string>(queryVerificar, parametrosVerificar, transaction);

                        if (estadoActual == null)
                            return NotFound(new { mensaje = "Solicitud no encontrada" });

                        if (estadoActual != "Pendiente")
                            return BadRequest(new { mensaje = $"La solicitud ya fue {estadoActual.ToLower()}" });

                        // Actualizar estado
                        var parametros = new DynamicParameters();
                        parametros.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        var query = @"UPDATE SolicitudesPrestamo 
                                     SET estadoSolicitud = 'Rechazada',
                                         fechaRechazo = GETDATE()
                                     WHERE solicitudPrestamoId = @SolicitudId";

                        var resultado = context.Execute(query, parametros, transaction);
                        
                        transaction.Commit();
                        return Ok(new { mensaje = "Solicitud rechazada exitosamente", resultado });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(new { mensaje = $"Error al rechazar la solicitud: {ex.Message}" });
                    }
                }
            }
        }

        [HttpGet]
        [Route("ObtenerTodasSolicitudes")]
        public IActionResult ObtenerTodasSolicitudes()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var query = @"SELECT sp.solicitudPrestamoId, sp.usuarioId, u.nombreCompleto, 
                     sp.tipoPrestamo, sp.montoSolicitud, sp.estadoSolicitud, 
                     sp.fechaSolicitud, sp.plazoMeses, u.identificacion
                     FROM SolicitudesPrestamo sp
                     INNER JOIN Usuario u ON sp.usuarioId = u.usuarioId
                     ORDER BY sp.fechaSolicitud DESC";

                var resultado = context.Query<dynamic>(query).Select(s => new
                {
                    SolicitudPrestamoId = (int)s.solicitudPrestamoId,
                    UsuarioId = (int)s.usuarioId,
                    TipoPrestamo = (string)s.tipoPrestamo,
                    MontoSolicitud = (decimal)s.montoSolicitud,
                    EstadoSolicitud = (string)s.estadoSolicitud,
                    FechaSolicitud = (DateTime)s.fechaSolicitud,
                    PlazoMeses = (int)s.plazoMeses,
                    Usuario = new
                    {
                        UsuarioId = (int)s.usuarioId,
                        NombreCompleto = (string)s.nombreCompleto,
                        Identificacion = (string)s.identificacion
                    }
                });

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerSolicitudPorId")]
        public IActionResult ObtenerSolicitudPorId(int solicitudId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@SolicitudId", solicitudId);

                var query = @"SELECT sp.*, u.nombreCompleto 
                     FROM SolicitudesPrestamo sp
                     INNER JOIN Usuario u ON sp.usuarioId = u.usuarioId
                     WHERE sp.solicitudPrestamoId = @SolicitudId";

                var resultado = context.QueryFirstOrDefault<dynamic>(query, parametros);

                if (resultado == null)
                    return NotFound();

                return Ok(new
                {
                    SolicitudPrestamoId = (int)resultado.solicitudPrestamoId,
                    UsuarioId = (int)resultado.usuarioId,
                    NombreCompleto = (string)resultado.nombreCompleto,
                    EstadoCivil = (string)resultado.estadoCivil,
                    PagaAlquiler = resultado.pagaAlquiler ? "Si" : "No",
                    MontoAlquiler = resultado.montoAlquiler != null ? (decimal?)resultado.montoAlquiler : null,
                    NombreAcreedor = resultado.nombreAcreedor != null ? (string)resultado.nombreAcreedor : null,
                    TotalCredito = resultado.totalCredito != null ? (decimal?)resultado.totalCredito : null,
                    AbonoSemanal = resultado.abonoSemanal != null ? (decimal?)resultado.abonoSemanal : null,
                    SaldoCredito = resultado.saldoCredito != null ? (decimal?)resultado.saldoCredito : null,
                    NombreDeudor = resultado.nombreDeudor != null ? (string)resultado.nombreDeudor : null,
                    TotalPrestamo = resultado.totalPrestamo != null ? (decimal?)resultado.totalPrestamo : null,
                    SaldoPrestamo = resultado.saldoPrestamo != null ? (decimal?)resultado.saldoPrestamo : null,
                    TipoPrestamo = (string)resultado.tipoPrestamo,
                    MontoSolicitud = (decimal)resultado.montoSolicitud,
                    EstadoSolicitud = (string)resultado.estadoSolicitud,
                    CuotaSemanalSolicitud = (decimal)resultado.cuotaSemanalSolicitud,
                    PlazoMeses = (int)resultado.plazoMeses,
                    PropositoPrestamo = (string)resultado.propositoPrestamo,
                    FechaSolicitud = (DateTime)resultado.fechaSolicitud
                });
            }
        }

        [HttpPost]
        [Route("CambiarEstadoSolicitud")]
        public IActionResult CambiarEstadoSolicitud([FromBody] CambiarEstadoSolicitudRequestModel request)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                context.Open();
                using (var transaction = context.BeginTransaction())
                {
                    try
                    {
                        // Si el nuevo estado es "Aprobada", crear el préstamo
                        if (request.NuevoEstado == "Aprobada")
                        {
                            // 1. Obtener datos de la solicitud
                            var parametrosSolicitud = new DynamicParameters();
                            parametrosSolicitud.Add("@SolicitudId", request.SolicitudPrestamoId);

                            var querySolicitud = @"SELECT * FROM SolicitudesPrestamo 
                                                  WHERE solicitudPrestamoId = @SolicitudId";

                            var solicitudData = context.QueryFirstOrDefault<SolicitudPrestamoCompleta>(
                                querySolicitud, parametrosSolicitud, transaction);

                            if (solicitudData == null)
                            {
                                transaction.Rollback();
                                return NotFound(new { success = false, message = "Solicitud no encontrada" });
                            }

                            // Verificar que no esté ya aprobada
                            if (solicitudData.EstadoSolicitud == "Aprobada")
                            {
                                transaction.Rollback();
                                return BadRequest(new { success = false, message = "La solicitud ya fue aprobada" });
                            }

                            // 2. Crear el préstamo
                            var parametrosPrestamo = new DynamicParameters();
                            parametrosPrestamo.Add("@UsuarioId", solicitudData.UsuarioId);
                            parametrosPrestamo.Add("@MontoAprobado", solicitudData.MontoSolicitud);
                            parametrosPrestamo.Add("@Plazo", solicitudData.PlazoMeses);
                            parametrosPrestamo.Add("@CuotaSemanal", solicitudData.CuotaSemanalSolicitud);
                            parametrosPrestamo.Add("@TipoPrestamo", solicitudData.TipoPrestamo);

                            var queryPrestamo = @"INSERT INTO Prestamos 
                                                 (usuarioId, montoAprobado, plazo, cuotaSemanal, tipoPrestamo, 
                                                  estadoPrestamo, fechaSolicitud, fechaEstado, saldoPendiente)
                                                 VALUES 
                                                 (@UsuarioId, @MontoAprobado, @Plazo, @CuotaSemanal, @TipoPrestamo, 
                                                  'Aprobado', GETDATE(), GETDATE(), @MontoAprobado);
                                                 SELECT CAST(SCOPE_IDENTITY() as int)";

                            var prestamoId = context.QuerySingle<int>(queryPrestamo, parametrosPrestamo, transaction);

                            // 3. Actualizar estado de solicitud a "Aprobada"
                            var parametrosActualizar = new DynamicParameters();
                            parametrosActualizar.Add("@SolicitudId", request.SolicitudPrestamoId);
                            parametrosActualizar.Add("@NuevoEstado", "Aprobada");

                            var queryActualizar = @"UPDATE SolicitudesPrestamo 
                                                   SET estadoSolicitud = @NuevoEstado,
                                                       fechaAprobacion = GETDATE()
                                                   WHERE solicitudPrestamoId = @SolicitudId";

                            context.Execute(queryActualizar, parametrosActualizar, transaction);

                            transaction.Commit();
                            
                            return Ok(new { 
                                success = true, 
                                message = "Préstamo aprobado y creado exitosamente",
                                prestamoId = prestamoId,
                                montoAprobado = solicitudData.MontoSolicitud
                            });
                        }
                        else if (request.NuevoEstado == "Rechazada")
                        {
                            // Si es rechazo, solo actualizar el estado
                            var parametrosRechazar = new DynamicParameters();
                            parametrosRechazar.Add("@SolicitudId", request.SolicitudPrestamoId);
                            parametrosRechazar.Add("@NuevoEstado", "Rechazada");

                            var queryRechazar = @"UPDATE SolicitudesPrestamo 
                                                 SET estadoSolicitud = @NuevoEstado,
                                                     fechaRechazo = GETDATE()
                                                 WHERE solicitudPrestamoId = @SolicitudId";

                            var resultado = context.Execute(queryRechazar, parametrosRechazar, transaction);
                            
                            transaction.Commit();
                            
                            return Ok(new { 
                                success = true, 
                                message = "Solicitud rechazada correctamente",
                                resultado 
                            });
                        }
                        else
                        {
                            // Para cualquier otro estado (En Revisión, etc.)
                            var parametros = new DynamicParameters();
                            parametros.Add("@SolicitudId", request.SolicitudPrestamoId);
                            parametros.Add("@NuevoEstado", request.NuevoEstado);

                            var query = @"UPDATE SolicitudesPrestamo 
                                         SET estadoSolicitud = @NuevoEstado
                                         WHERE solicitudPrestamoId = @SolicitudId";

                            var resultado = context.Execute(query, parametros, transaction);
                            
                            transaction.Commit();
                            
                            return Ok(new { 
                                success = true, 
                                message = $"Estado cambiado a {request.NuevoEstado}",
                                resultado 
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(new { 
                            success = false, 
                            message = $"Error al procesar la solicitud: {ex.Message}" 
                        });
                    }
                }
            }
        }
    }
}