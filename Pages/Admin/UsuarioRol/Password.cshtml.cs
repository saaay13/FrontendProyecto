using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.UsuarioRol
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class PasswordModel : PageModel
    {
        private readonly HttpClient _http;
        public PasswordModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public PasswordInput Input { get; set; } = new();

        public string? UsuarioTexto { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _http.GetFromJsonAsync<UsuarioDto>($"/api/Usuarios/{Id}");
            if (user is null) return NotFound();

            UsuarioTexto = $"{user.Nombre} {user.Apellido} ({user.CorreoUsuario})";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // recarga nombre usuario
                return Page();
            }

            var resp = await _http.PutAsJsonAsync($"/api/Usuarios/{Id}/password", new { NewPassword = Input.NewPassword });
            if (resp.IsSuccessStatusCode)
                return RedirectToPage("Edit", new { id = Id }); // volver a Edit después del cambio

            var err = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(err) ? "No se pudo actualizar la contraseña." : err);
            await OnGetAsync();
            return Page();
        }

        public class PasswordInput
        {
            [Required, MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string CorreoUsuario);
    }
}
