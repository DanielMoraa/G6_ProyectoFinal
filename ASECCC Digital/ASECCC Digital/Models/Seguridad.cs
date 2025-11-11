using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASECCC_Digital.Models
{
    public class Seguridad : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetInt32("UsuarioId") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
