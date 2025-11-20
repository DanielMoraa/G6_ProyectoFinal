using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ASECCC_Digital.Controllers
{
    [Seguridad]
    public class ReportesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public ReportesController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ResumenFinanciero()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Reportes/ResumenFinanciero?usuarioId=" + usuarioId;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var resumen = respuesta.Content.ReadFromJsonAsync<ResumenFinancieroModel>().Result;
                    return View(resumen ?? new ResumenFinancieroModel());
                }

                ViewBag.Mensaje = "No se pudo obtener el resumen financiero";
                return View(new ResumenFinancieroModel());
            }
        }


        [HttpPost]
        public IActionResult ObtenerHistorialTransacciones(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] +
                    $"Reportes/HistorialTransacciones?usuarioId={usuarioId}";

                if (fechaInicio.HasValue)
                    url += $"&fechaInicio={fechaInicio.Value:yyyy-MM-dd}";
                if (fechaFin.HasValue)
                    url += $"&fechaFin={fechaFin.Value:yyyy-MM-dd}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var transacciones = respuesta.Content.ReadFromJsonAsync<List<TransaccionReporteModel>>().Result;
                    return Json(new { success = true, data = transacciones });
                }

                return Json(new { success = false, message = "Error al obtener el historial" });
            }
        }

        [HttpGet]
        public IActionResult EstadoCuenta()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Reportes/EstadoCuentaDetallado?usuarioId=" + usuarioId;
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var estadoCuenta = respuesta.Content.ReadFromJsonAsync<EstadoCuentaModel>().Result;
                    return View(estadoCuenta ?? new EstadoCuentaModel());
                }

                ViewBag.Mensaje = "No se pudo obtener el estado de cuenta";
                return View(new EstadoCuentaModel());
            }
        }

        [HttpGet]
        public IActionResult ReportePrestamos()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ObtenerReportePrestamos(DateTime? fechaInicio, DateTime? fechaFin, string? estado)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Reportes/ReportePrestamos?";

                var parametros = new List<string>();
                if (fechaInicio.HasValue)
                    parametros.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-dd}");
                if (fechaFin.HasValue)
                    parametros.Add($"fechaFin={fechaFin.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(estado))
                    parametros.Add($"estado={estado}");

                url += string.Join("&", parametros);

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var prestamos = respuesta.Content.ReadFromJsonAsync<List<PrestamoReporteDetalladoModel>>().Result;
                    return Json(new { success = true, data = prestamos });
                }

                return Json(new { success = false, message = "Error al obtener el reporte" });
            }
        }

        [HttpGet]
        public IActionResult ReporteAhorros()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ObtenerReporteAhorros(DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Reportes/ReporteAhorros?";

                var parametros = new List<string>();
                if (fechaInicio.HasValue)
                    parametros.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-dd}");
                if (fechaFin.HasValue)
                    parametros.Add($"fechaFin={fechaFin.Value:yyyy-MM-dd}");

                url += string.Join("&", parametros);

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var respuesta = client.GetAsync(url).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var ahorros = respuesta.Content.ReadFromJsonAsync<List<AhorroReporteDetalladoModel>>().Result;
                    return Json(new { success = true, data = ahorros });
                }

                return Json(new { success = false, message = "Error al obtener el reporte" });
            }
        }
    }
}