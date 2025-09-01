using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendProyecto.Pages.Coordinador.Proyectos
{
    [Authorize(Roles = "Coordinador,Administrador")]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var url = Url.Page("/Admin/Proyectos/Index") + Request.QueryString.Value;
            return Redirect(url);
        }
    }
}
