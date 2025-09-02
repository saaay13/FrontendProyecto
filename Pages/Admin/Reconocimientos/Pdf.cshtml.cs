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
            
            var resp = await _http.GetAsync($"/api/Carnets/{id}/pdf", HttpCompletionOption.ResponseHeadersRead);
            if (!resp.IsSuccessStatusCode)
                return NotFound("Carnet no encontrado o error generando PDF.");

            var bytes = await resp.Content.ReadAsByteArrayAsync();
           
            return File(bytes, "application/pdf", $"carnet-{id}.pdf");
        }
    }
}
