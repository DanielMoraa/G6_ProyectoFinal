using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace ASECCC_Digital.Controllers
{
    public class ContactoySoporteController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        [HttpGet]
        public IActionResult Contacto() => View(new ContactoSoporteVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contacto(ContactoSoporteVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var subject = "Hemos recibido tu consulta";
            var body = $@"
                <div style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:30px 0;'>
                    <table align='center' width='100%' cellpadding='0' cellspacing='0' style='max-width:500px;background:#fff;border-radius:8px;box-shadow:0 2px 8px #e0e0e0;overflow:hidden;'>
                        <tr>
                            <td style='background:#007bff;padding:20px 0;text-align:center;'>
                                <span style='color:#fff;font-size:22px;font-weight:bold;'>ASECCC Digital</span>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:32px 24px 16px 24px;'>
                                <h2 style='color:#333;margin-top:0;'>¡Hola {model.Nombre}!</h2>
                                <p style='color:#444;font-size:16px;margin-bottom:24px;'>
                                    Hemos recibido tu consulta y pronto te responderemos.
                                </p>
                                <div style='background:#f1f3f6;border-radius:6px;padding:16px 20px;margin-bottom:24px;'>
                                    <p style='margin:0;'><b>Asunto:</b> {model.Asunto}</p>
                                    <p style='margin:0;'><b>Mensaje:</b> {model.Mensaje}</p>
                                </div>
                                <p style='color:#555;font-size:15px;margin-bottom:0;'>
                                    Gracias por contactarnos.<br>
                                    <span style='color:#007bff;font-weight:bold;'>Equipo ASECCC Digital</span>
                                </p>
                            </td>
                        </tr>
                    </table>
                </div>
            ";

            EnviarCorreo(subject, body, model.Correo);

            TempData["Msg"] = "Mensaje enviado correctamente. Pronto te contactaremos.";
            return RedirectToAction(nameof(Confirmacion));
        }

        [HttpGet]
        public IActionResult Confirmacion() => View();
        
        [HttpGet]
        public IActionResult Soporte() => View();

        private void EnviarCorreo(string subject, string body, string destinatario)
        {
            var correoSMTP = _configuration["Valores:CorreoSMTP"]!;
            var contrasennaSMTP = _configuration["Valores:ContrasennaSMTP"]!;

            if (string.IsNullOrEmpty(contrasennaSMTP))
                return;

            var mensaje = new MailMessage
            {
                From = new MailAddress(correoSMTP),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoSMTP, contrasennaSMTP),
                EnableSsl = true
            };

            smtp.Send(mensaje);
        }
    }
}
