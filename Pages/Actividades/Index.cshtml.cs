using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using FrontendProyecto.Pages.Admin.Actividades;

namespace FrontendProyecto.Pages.Actividades
{
    
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        public IndexModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        public List<ActividadListDto> Lista { get; set; } = new();

        public async Task OnGetAsync()
        {

            Lista = await _http.GetFromJsonAsync<List<ActividadListDto>>("/api/Actividades")
                ?? new List<ActividadListDto>();
        }
    }
}
