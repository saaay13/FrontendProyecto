using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendProyecto.Pages.Admin.Reconocimientos
{
    public class PdfCertModel : PageModel
    {
        private readonly HttpClient _http;
        public PdfCertModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var resp = await _http.GetAsync($"/api/Certificados/{id}/pdf", HttpCompletionOption.ResponseHeadersRead);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                return StatusCode((int)resp.StatusCode, string.IsNullOrWhiteSpace(body) ? "Error generando PDF" : body);
            }

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/pdf", $"certificado-{id}.pdf");
        }
    }
}
