using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FrontendProyecto.Pages.Login
{
    [AllowAnonymous]
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

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador")) return RedirectToPage("/Admin/Panel");
                if (User.IsInRole("Coordinador")) return RedirectToPage("/Coordinador/Panel");
                return RedirectToPage("Voluntario/Panel"); // Voluntario
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CorreoUsuario) || string.IsNullOrWhiteSpace(Password))
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

            if (!response.IsSuccessStatusCode)
            {
                MensajeError = "Credenciales inválidas. Intente de nuevo.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            if (string.IsNullOrEmpty(token))
            {
                MensajeError = "No se recibió token.";
                return Page();
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            string? userId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid")?.Value;
            string? email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
            string? roleName = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value
                               ?? "Voluntario";

            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(userId))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

            if (!string.IsNullOrEmpty(email))
                claims.Add(new Claim(ClaimTypes.Email, email));

            claims.Add(new Claim(ClaimTypes.Name, email ?? CorreoUsuario));

            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });

            HttpContext.Session.SetString("JWT", token);

            Response.Cookies.Append("AuthToken", token!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(2)
            });

            return roleName switch
            {
                "Administrador" => RedirectToPage("/Admin/Panel"),
                "Coordinador" => RedirectToPage("/Coordinador/Panel"),
                _ => RedirectToPage("/Voluntario/Panel") 
            };
        }
    }
}
