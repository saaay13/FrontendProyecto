using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendProyecto.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class PanelModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public PanelModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public int Usuarios { get; set; }
        public int Ongs { get; set; }
        public int Proyectos { get; set; }
        public int Actividades { get; set; }

        public async Task OnGet()
        {
            var client = _httpClientFactory.CreateClient("API");
            Usuarios = (await client.GetFromJsonAsync<List<object>>("/api/Auth") ?? new()).Count;
            Ongs = (await client.GetFromJsonAsync<List<object>>("/api/Ongs") ?? new()).Count;
            Proyectos = (await client.GetFromJsonAsync<List<object>>("/api/Proyectos") ?? new()).Count;
            Actividades = (await client.GetFromJsonAsync<List<object>>("/api/Actividades") ?? new()).Count;
        }

    }
}
