using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Ongs
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public EditModel(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("API");
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public OngInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<OngDto>($"/api/Ongs/{Id}");
            if (data is null) return NotFound();

            Input = new OngInput
            {
                NombreOng = data.NombreOng,
                Descripcion = data.Descripcion,
                Direccion = data.Direccion,
                Telefono = data.Telefono
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var body = new
            {
                IdOng = Id,
                Input.NombreOng,
                Input.Descripcion,
                Input.Direccion,
                Input.Telefono
               
            };

            var resp = await _httpClient.PutAsJsonAsync($"/api/Ongs/{Id}", body);
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error al actualizar: {error}");
            return Page();
        }

        public record OngDto(
            int IdOng,
            string NombreOng,
            string Descripcion,
            string Direccion,
            string Telefono,
            string Correo
        );
    }
}
