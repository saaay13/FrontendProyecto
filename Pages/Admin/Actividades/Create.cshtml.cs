using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class CreateModel : PageModel
    {
        private readonly HttpClient _http;
        public CreateModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        [BindProperty] public ActividadInput Input { get; set; } = new();
        public ActividadFormModel Form { get; set; } = new();

        public async Task OnGetAsync()
        {
            var proy = await _http.GetFromJsonAsync<List<ProyectoItem>>("/api/Proyectos") ?? new();
            Form = new ActividadFormModel { Input = Input, Proyectos = proy };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var resp = await _http.PostAsJsonAsync("/api/Actividades", Input);
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo crear la actividad." : error);
            await OnGetAsync();
            return Page();
        }
    }
}
