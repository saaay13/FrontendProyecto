using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        public IndexModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        public List<ActividadListDto> Lista { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Espera que la API devuelva Proyecto.NombreProyecto embebido o un DTO similar
            Lista = await _http.GetFromJsonAsync<List<ActividadListDto>>("/api/Actividades")
                ?? new List<ActividadListDto>();
        }
    }
}
