using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FrontendProyecto.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;

        public IndexModel(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("API");
        }

        [BindProperty] public string CorreoUsuario { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
        public string MensajeError { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CorreoUsuario) || string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Debe ingresar correo y contraseña.";
                return Page();
            }

            var content = new StringContent(
                JsonSerializer.Serialize(new { CorreoUsuario, Password }),
                Encoding.UTF8, "application/json");

            var resp = await _http.PostAsync("/api/Auth/login", content);
            if (!resp.IsSuccessStatusCode)
            {
                MensajeError = "Credenciales inválidas.";
                return Page();
            }

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();

            // 1) Guardar JWT en Session (para HttpClient -> Authorization: Bearer ...)
            HttpContext.Session.SetString("JWT", token!);

            // 2) Crear cookie de autenticación con claims del JWT (roles incluidos)
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            var nameId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type.EndsWith("/nameidentifier"))?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type.EndsWith("/emailaddress"))?.Value;
            var roles = jwt.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type.EndsWith("/role")).Select(c => c.Value);

            if (!string.IsNullOrEmpty(nameId)) identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nameId));
            if (!string.IsNullOrEmpty(email)) identity.AddClaim(new Claim(ClaimTypes.Email, email));
            foreach (var r in roles.Distinct()) identity.AddClaim(new Claim(ClaimTypes.Role, r));
            if (!string.IsNullOrEmpty(email)) identity.AddClaim(new Claim(ClaimTypes.Name, email));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) }
            );

            // Redirección
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            // Si el token tiene Admin, al panel admin; si es coord, al de coord; si no, al de voluntario
            if (roles.Contains("Administrador")) return RedirectToPage("/Admin/Panel");
            if (roles.Contains("Coordinador")) return RedirectToPage("/Coordinador/Panel");
            return RedirectToPage("/Voluntario/Panel");
        }
    }
}
