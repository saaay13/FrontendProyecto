using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendProyecto.Pages.Ongs
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class DeleteModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DeleteModel(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<OngDto>($"/api/Ongs/{Id}");
            if (data is null) return NotFound();

            Nombre = data.NombreOng;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resp = await _httpClient.DeleteAsync($"/api/Ongs/{Id}");
            if (resp.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error al eliminar: {error}");
            return Page();
        }

        public record OngDto(
            int IdOng,
            string NombreOng,
            string Descripcion,
            string Direccion,
            string Telefono
        );
    }
}
