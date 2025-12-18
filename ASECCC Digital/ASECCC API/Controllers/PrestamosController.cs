using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

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

                var resultado = context.Query<PrestamoResponseModel>(
                    "ObtenerPrestamosPorUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

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

                var resultado = context.QueryFirstOrDefault<PrestamoDetalleResponseModel>(
                    "ObtenerDetallePrestamo",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

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

                var resultado = context.Query<PrestamoTransaccionResponseModel>(
                    "ObtenerTransaccionesPrestamo",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

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

                // SP debe insertar y devolver el ID (SELECT CAST(SCOPE_IDENTITY() as int))
                var resultado = context.QuerySingle<int>(
                    "CrearSolicitudPrestamo",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("RegistrarAbonoPrestamo")]
        public IActionResult RegistrarAbonoPrestamo(AbonoPrestamoRequestModel abono)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@PrestamoId", abono.PrestamoId);
                parametros.Add("@MontoAbonado", abono.MontoAbonado);

                // El SP RegistrarAbonoPrestamo debe:
                // - Insertar en PrestamosTransacciones
                // - Actualizar Prestamos (saldoPendiente y estadoPrestamo)
                // - Manejar la transacción internamente
                var resultado = context.Execute(
                    "RegistrarAbonoPrestamo",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerSolicitudesPendientes")]
        public IActionResult ObtenerSolicitudesPendientes()
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var resultado = context.Query<SolicitudPrestamoResponseModel>(
                    "ObtenerSolicitudesPendientes",
                    commandType: CommandType.StoredProcedure
                );

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
                        // 1. Obtener datos de la solicitud
                        var parametrosSolicitud = new DynamicParameters();
                        parametrosSolicitud.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        var solicitudData = context.QueryFirstOrDefault<SolicitudPrestamoCompleta>(
                            "ObtenerSolicitudPrestamoPorId",
                            parametrosSolicitud,
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        if (solicitudData == null)
                            return NotFound(new { mensaje = "Solicitud no encontrada" });

                        // Verificar que la solicitud esté en estado Pendiente
                        if (solicitudData.EstadoSolicitud != "Pendiente")
                            return BadRequest(new { mensaje = $"La solicitud ya fue {solicitudData.EstadoSolicitud.ToLower()}" });

                        // 2. Crear préstamo (SP CrearPrestamo debe insertar y devolver el ID)
                        var parametrosPrestamo = new DynamicParameters();
                        parametrosPrestamo.Add("@UsuarioId", solicitudData.UsuarioId);
                        parametrosPrestamo.Add("@MontoAprobado", solicitudData.MontoSolicitud);
                        parametrosPrestamo.Add("@Plazo", solicitudData.PlazoMeses);
                        parametrosPrestamo.Add("@CuotaSemanal", solicitudData.CuotaSemanalSolicitud);
                        parametrosPrestamo.Add("@TipoPrestamo", solicitudData.TipoPrestamo);

                        var prestamoId = context.QuerySingle<int>(
                            "CrearPrestamo",
                            parametrosPrestamo,
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        // 3. Actualizar estado de solicitud usando SP
                        var parametrosActualizar = new DynamicParameters();
                        parametrosActualizar.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        context.Execute(
                            "AprobarSolicitudPrestamoEstado",
                            parametrosActualizar,
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        transaction.Commit();
                        return Ok(new
                        {
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

                        var solicitudData = context.QueryFirstOrDefault<SolicitudPrestamoCompleta>(
                            "ObtenerSolicitudPrestamoPorId",
                            parametrosVerificar,
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        if (solicitudData == null)
                            return NotFound(new { mensaje = "Solicitud no encontrada" });

                        if (solicitudData.EstadoSolicitud != "Pendiente")
                            return BadRequest(new { mensaje = $"La solicitud ya fue {solicitudData.EstadoSolicitud.ToLower()}" });

                        // Actualizar estado usando SP
                        var parametros = new DynamicParameters();
                        parametros.Add("@SolicitudId", solicitud.SolicitudPrestamoId);

                        var resultado = context.Execute(
                            "RechazarSolicitudPrestamoEstado",
                            parametros,
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );

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
                // SP debe devolver las mismas columnas que tu SELECT original
                var filas = context.Query<dynamic>(
                    "ObtenerTodasSolicitudes",
                    commandType: CommandType.StoredProcedure
                );

                var resultado = filas.Select(s => new
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

                // SP ObtenerSolicitudPorId debe hacer el JOIN con Usuario
                var resultado = context.QueryFirstOrDefault<dynamic>(
                    "ObtenerSolicitudPorId",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

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
                        if (request.NuevoEstado == "Aprobada")
                        {
                            // 1. Obtener datos de la solicitud
                            var parametrosSolicitud = new DynamicParameters();
                            parametrosSolicitud.Add("@SolicitudId", request.SolicitudPrestamoId);

                            var solicitudData = context.QueryFirstOrDefault<SolicitudPrestamoCompleta>(
                                "ObtenerSolicitudPrestamoPorId",
                                parametrosSolicitud,
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );

                            if (solicitudData == null)
                            {
                                transaction.Rollback();
                                return NotFound(new { success = false, message = "Solicitud no encontrada" });
                            }

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

                            var prestamoId = context.QuerySingle<int>(
                                "CrearPrestamo",
                                parametrosPrestamo,
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );

                            // 3. Actualizar estado a "Aprobada"
                            var parametrosActualizar = new DynamicParameters();
                            parametrosActualizar.Add("@SolicitudId", request.SolicitudPrestamoId);

                            context.Execute(
                                "AprobarSolicitudPrestamoEstado",
                                parametrosActualizar,
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );

                            transaction.Commit();

                            return Ok(new
                            {
                                success = true,
                                message = "Préstamo aprobado y creado exitosamente",
                                prestamoId = prestamoId,
                                montoAprobado = solicitudData.MontoSolicitud
                            });
                        }
                        else if (request.NuevoEstado == "Rechazada")
                        {
                            var parametrosRechazar = new DynamicParameters();
                            parametrosRechazar.Add("@SolicitudId", request.SolicitudPrestamoId);

                            var resultado = context.Execute(
                                "RechazarSolicitudPrestamoEstado",
                                parametrosRechazar,
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );

                            transaction.Commit();

                            return Ok(new
                            {
                                success = true,
                                message = "Solicitud rechazada correctamente",
                                resultado
                            });
                        }
                        else
                        {
                            // Otros estados: SP genérico
                            var parametros = new DynamicParameters();
                            parametros.Add("@SolicitudId", request.SolicitudPrestamoId);
                            parametros.Add("@NuevoEstado", request.NuevoEstado);

                            var resultado = context.Execute(
                                "CambiarEstadoSolicitud",
                                parametros,
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );

                            transaction.Commit();

                            return Ok(new
                            {
                                success = true,
                                message = $"Estado cambiado a {request.NuevoEstado}",
                                resultado
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Error al procesar la solicitud: {ex.Message}"
                        });
                    }
                }
            }
        }
    }
}
