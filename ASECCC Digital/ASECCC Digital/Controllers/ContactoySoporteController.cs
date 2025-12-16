using Microsoft.AspNetCore.Mvc;
using ASECCC_Digital.Models;

namespace ASECCC_Digital.Controllers
{
    public class ContactoySoporteController : Controller
    {
        [HttpGet]
        public IActionResult Contacto() => View(new ContactoSoporteVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contacto(ContactoSoporteVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            TempData["Msg"] = "Mensaje enviado correctamente. Pronto te contactaremos.";
            return RedirectToAction(nameof(Confirmacion));
        }

        [HttpGet]
        public IActionResult Confirmacion() => View();

        [HttpGet]
        public IActionResult Soporte() => View();
    }
}
