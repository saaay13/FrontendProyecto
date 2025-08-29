using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FrontendProyecto.Pages.Login
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;

            [BindProperty] public string Nombre { get; set; } = string.Empty;
            [BindProperty] public string Apellido { get; set; } = string.Empty;
            [BindProperty] public string CorreoUsuario { get; set; } = string.Empty;
            [BindProperty] public string Password { get; set; } = string.Empty;
            [BindProperty] public string Telefono { get; set; } = string.Empty;

            public string MensajeError { get; set; } = string.Empty;

            public RegisterModel(IHttpClientFactory httpClientFactory)
            {
                _httpClient = httpClientFactory.CreateClient("API");
            }

            public void OnGet() { }

            public async Task<IActionResult> OnPostAsync()
            {
                var registerData = new
                {
                    Nombre,
                    Apellido,
                    CorreoUsuario,
                    Password,
                    Telefono
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(registerData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/Auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Login/Index");
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    MensajeError = json;
                    return Page();
                }
            }
        
    }
}
