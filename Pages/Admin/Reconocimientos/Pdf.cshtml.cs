using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendProyecto.Pages.Admin.Reconocimientos
{
    public class PdfModel : PageModel
    {
        private readonly HttpClient _http;
        public PdfModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Proxys: llama a tu API que devuelve PDF real
            var resp = await _http.GetAsync($"/api/Carnets/{id}/pdf", HttpCompletionOption.ResponseHeadersRead);
            if (!resp.IsSuccessStatusCode)
                return NotFound("Carnet no encontrado o error generando PDF.");

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            // Devuelve el archivo. La vista no renderiza nada.
            return File(bytes, "application/pdf", $"carnet-{id}.pdf");
        }
    }
}
