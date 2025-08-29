using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Proyectos
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;

        public IndexModel(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("API");
        }

        public List<ProyectoDto> Lista { get; private set; } = new();

       
        public async Task OnGetAsync(int? ongId)
        {
            var data = await _http.GetFromJsonAsync<List<ProyectoDto>>("/api/Proyectos") ?? new();
            if (ongId.HasValue)
                data = data.Where(p => p.IdOng == ongId.Value).ToList();

         
            Lista = data.OrderByDescending(p => p.FechaInicio).ToList();
        }

        public record ProyectoDto(
            int IdProyecto,
            int IdOng,
            string NombreProyecto,
            string Descripcion,
            DateTime FechaInicio,
            DateTime FechaFin,
            string EstadoProyecto,  
            int IdResponsable
        );
    }
}
