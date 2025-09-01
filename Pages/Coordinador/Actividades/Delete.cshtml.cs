using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace FrontendProyecto.Pages.Coordinador.Actividades
{
    [Authorize(Roles = "Coordinador,Administrador")]
    public class CoordinadorActividadesDeleteAliasModel : PageModel
    {
        public IActionResult OnGet(int? id)
        {
            var url = Url.Page("/Admin/Actividades/Delete", new { id }) + Request.QueryString.Value;
            return Redirect(url);
        }
    }
}
