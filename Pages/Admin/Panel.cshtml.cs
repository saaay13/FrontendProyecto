using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin
{
    [Authorize(Roles = "Administrador")]
    public class PanelModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        public PanelModel(IHttpClientFactory httpClientFactory) => _http = httpClientFactory;

        public int Usuarios { get; set; }
        public int Ongs { get; set; }
        public int Proyectos { get; set; }
        public int Actividades { get; set; }

        public async Task OnGet()
        {
            var client = _http.CreateClient("API");

            var me = await client.GetAsync("/api/Auth/me");
            if (me.StatusCode == HttpStatusCode.Unauthorized)
            {
                Response.Redirect($"/Login/Index?ReturnUrl={Uri.EscapeDataString(Request.Path)}");
                return;
            }

            // 2) Cargar contadores (cada uno tolera 401/403)
            Usuarios = await CountAsync(client, "/api/Usuarios");         
            Ongs = await CountAsync(client, "/api/Ongs");          
            Proyectos = await CountAsync(client, "/api/Proyectos");    
            Actividades = await CountAsync(client, "/api/Actividades");  
        }

        private static async Task<int> CountAsync(HttpClient c, string url)
        {
            try
            {
                var resp = await c.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return 0; 
                var list = await resp.Content.ReadFromJsonAsync<List<object>>();
                return list?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
