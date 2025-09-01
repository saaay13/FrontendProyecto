using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace FrontendProyecto.Pages.Coordinador.Actividades
{
    [Authorize(Roles = "Coordinador,Administrador")]
    public class CoordinadorActividadesCreateAliasModel : PageModel
    {
        public IActionResult OnGet()
        {
            var url = Url.Page("/Admin/Actividades/Create") + Request.QueryString.Value;
            return Redirect(url);
        }
    }
}
