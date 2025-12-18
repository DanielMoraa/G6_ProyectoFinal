using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ASECCC_Digital.Controllers
{
    [Seguridad]
    public class PrestamosController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public PrestamosController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult SolicitudPrestamo()
        {
            try
            {
                var modelo = new SolicitudPrestamoModel();
                return View(modelo);
            }
            catch (Exception ex)
            {
                // Log temporal para ver el error exacto
                ViewBag.Mensaje = $"Error: {ex.Message} - {ex.StackTrace}";
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult SolicitudPrestamo(SolicitudPrestamoModel solicitud)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Por favor complete todos los campos requeridos";
                return View(solicitud);
            }

            solicitud.UsuarioId = (int)HttpContext.Session.GetInt32("UsuarioId")!;

            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/CrearSolicitudPrestamo";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.PostAsJsonAsync(url, solicitud).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var solicitudId = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (solicitudId > 0)
                    {
                        TempData["SuccessMessage"] = "Solicitud de préstamo enviada exitosamente, una vez aprobado, saldrá en el sistema ";
                        return RedirectToAction("ConsultaPrestamoAsociado");
                    }
                }

                ViewBag.Mensaje = "Error al procesar la solicitud de préstamo";
                return View(solicitud);
            }
        }

        [HttpGet]
        public IActionResult ConsultaPrestamoAsociado()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/ObtenerPrestamosPorUsuario?usuarioId=" + usuarioId;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    // ?? AQUÍ ESTABA EL ERROR - Cambia PrestamoModel por SolicitudPrestamoCompletaModel
                    var solicitudes = respuesta.Content.ReadFromJsonAsync<List<SolicitudPrestamoCompletaModel>>().Result;
                    return View(solicitudes ?? new List<SolicitudPrestamoCompletaModel>());
                }

                ViewBag.Mensaje = "No se encontraron solicitudes de préstamo";
                return View(new List<SolicitudPrestamoCompletaModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int prestamoId)
        {
            using var client = _http.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] +
                      $"Prestamos/ObtenerDetallePrestamo?prestamoId={prestamoId}";

            var respuesta = await client.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                ViewBag.Mensaje = "No se encontró el préstamo";
                return RedirectToAction("ConsultaPrestamoAsociado");
            }

            var prestamo = await respuesta.Content.ReadFromJsonAsync<PrestamoDetalleModel>();

            if (prestamo == null)
            {
                ViewBag.Mensaje = "No se encontró el préstamo";
                return RedirectToAction("ConsultaPrestamoAsociado");
            }

            var urlTransacciones = _configuration["Valores:UrlAPI"] +
                                   $"Prestamos/ObtenerTransaccionesPrestamo?prestamoId={prestamoId}";

            var respuestaTransacciones = await client.GetAsync(urlTransacciones);

            if (respuestaTransacciones.IsSuccessStatusCode)
            {
                prestamo.Transacciones =
                    await respuestaTransacciones.Content
                        .ReadFromJsonAsync<List<PrestamoTransaccionModel>>()
                    ?? new List<PrestamoTransaccionModel>();
            }

            return View(prestamo);
        }


        [HttpPost]
        public IActionResult RegistrarAbono(AbonoPrestamoModel abono)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/RegistrarAbonoPrestamo";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.PostAsJsonAsync(url, abono).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var resultado = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (resultado > 0)
                    {
                        return Json(new { success = true, message = "Abono registrado exitosamente" });
                    }
                }

                return Json(new { success = false, message = "Error al registrar el abono" });
            }
        }

        [HttpGet]
        public IActionResult GestionSolicitudes()
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/ObtenerSolicitudesPendientes";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var solicitudes = respuesta.Content.ReadFromJsonAsync<List<SolicitudPrestamoListaModel>>().Result;
                    return View(solicitudes ?? new List<SolicitudPrestamoListaModel>());
                }

                ViewBag.Mensaje = "No se encontraron solicitudes";
                return View(new List<SolicitudPrestamoListaModel>());
            }
        }

        [HttpPost]
        public IActionResult AprobarSolicitud(int solicitudPrestamoId)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/AprobarSolicitudPrestamo";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new { SolicitudPrestamoId = solicitudPrestamoId };
                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Solicitud aprobada exitosamente" });
                }

                return Json(new { success = false, message = "Error al aprobar la solicitud" });
            }
        }

        [HttpPost]
        public IActionResult RechazarSolicitud(int solicitudPrestamoId)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/RechazarSolicitudPrestamo";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new { SolicitudPrestamoId = solicitudPrestamoId };
                var respuesta = client.PutAsJsonAsync(url, payload).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Solicitud rechazada" });
                }

                return Json(new { success = false, message = "Error al rechazar la solicitud" });
            }
        }

        [HttpGet]
        public IActionResult RevisionPrestamos()
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/ObtenerTodasSolicitudes";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var todasSolicitudes = respuesta.Content
                        .ReadFromJsonAsync<List<SolicitudPrestamoDetalleModel>>().Result 
                        ?? new List<SolicitudPrestamoDetalleModel>();

                    var viewModel = new SolicitudPrestamoViewModel
                    {
                        Solicitudes = new SolicitudesAgrupadas
                        {
                            Pendientes = todasSolicitudes.Where(s => s.EstadoSolicitud == "Pendiente").ToList(),
                            EnRevision = todasSolicitudes.Where(s => s.EstadoSolicitud == "En Revisión").ToList(),
                            Aprobadas = todasSolicitudes.Where(s => s.EstadoSolicitud == "Aprobada").ToList(),
                            Rechazadas = todasSolicitudes.Where(s => s.EstadoSolicitud == "Rechazada").ToList()
                        }
                    };

                    return View(viewModel);
                }

                return View(new SolicitudPrestamoViewModel());
            }
        }

        [HttpGet]
        public IActionResult ObtenerSolicitudPorId(int id)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/ObtenerSolicitudPorId?solicitudId=" + id;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var solicitud = respuesta.Content.ReadFromJsonAsync<SolicitudPrestamoCompletaModel>().Result;
                    return Json(solicitud);
                }

                return Json(new { error = "No se encontró la solicitud" });
            }
        }

        [HttpPost]
        public IActionResult CambiarEstadoSolicitud(int solicitudId, string nuevoEstado)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Prestamos/CambiarEstadoSolicitud";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new { SolicitudPrestamoId = solicitudId, NuevoEstado = nuevoEstado };
                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Estado actualizado correctamente" });
                }

                return Json(new { success = false, message = "Error al actualizar el estado" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerSolicitudDetalleAjax(int solicitudId)
        {
            using var client = _http.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

            var url = _configuration["Valores:UrlAPI"] +
                      $"Prestamos/ObtenerSolicitudPorId?solicitudId={solicitudId}";

            var respuesta = await client.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
                return BadRequest();

            var solicitud = await respuesta.Content
                .ReadFromJsonAsync<SolicitudPrestamoCompletaModel>();

            return Json(solicitud);
        }

    }
}