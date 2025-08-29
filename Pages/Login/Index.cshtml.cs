using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FrontendProyecto.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public string CorreoUsuario { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string MensajeError { get; set; } = string.Empty;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(CorreoUsuario) || string.IsNullOrEmpty(Password))
            {
                MensajeError = "Debe ingresar correo y contraseña.";
                return Page();
            }

            var loginData = new
            {
                CorreoUsuario,
                Password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var token = doc.RootElement.GetProperty("token").GetString();

                // Guardar token en cookie segura
                Response.Cookies.Append("AuthToken", token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddHours(2)
                });

                return RedirectToPage("/Index"); // redirigir a inicio
            }
            else
            {
                MensajeError = "Credenciales inválidas. Intente de nuevo.";
                return Page();
            }
        }
    }
}
