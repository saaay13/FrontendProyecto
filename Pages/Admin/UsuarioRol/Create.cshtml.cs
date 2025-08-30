using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
        private readonly HttpClient _http;
        public CreateModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        [BindProperty]
        public UsuarioRolInput Input { get; set; } = new();

        public UsuarioRolFormModel Form { get; set; } = new();

        public async Task OnGetAsync()
        {
            var usuarios = await _http.GetFromJsonAsync<List<UsuarioItem>>("/api/Auth") ?? new();
            var roles = await _http.GetFromJsonAsync<List<RolItem>>("/api/Rol") ?? new();

            Form = new UsuarioRolFormModel
            {
                Input = Input,
                Usuarios = usuarios,
                Roles = roles
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var resp = await _http.PostAsJsonAsync("/api/UsuarioRol", Input);
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo asignar el rol." : error);

            await OnGetAsync();
            return Page();
        }
    }
}
