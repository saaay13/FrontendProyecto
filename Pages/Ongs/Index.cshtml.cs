using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Ongs
{
    [AllowAnonymous] // Esta página es pública
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }

        public List<OngPublicDto> Lista { get; private set; } = new();

        public async Task OnGet()
        {
            // Llamada pública (sin token). Si tienes JwtSessionHandler,
            // solo adxjunta Authorization cuando hay JWT en Session; aquí no habrá.
            Lista = await _httpClient.GetFromJsonAsync<List<OngPublicDto>>("/api/Ongs/public")
                    ?? new List<OngPublicDto>();
        }

        public record OngPublicDto(
            int IdOng,
            string NombreOng,
            string? Descripcion,
            string? Direccion,
            string? Telefono
        );
    }
}
