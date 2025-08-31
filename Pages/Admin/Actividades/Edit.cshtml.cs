using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    [Authorize(Roles = "Administrador,Coordinador")]
    public class EditModel : PageModel
    {
        private readonly HttpClient _http;
        public EditModel(IHttpClientFactory factory) => _http = factory.CreateClient("API");

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty] public ActividadInput Input { get; set; } = new();
        public ActividadFormModel Form { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var dto = await _http.GetFromJsonAsync<ActividadDetalleDto>($"/api/Actividades/{Id}");
            if (dto is null) return NotFound();

            Input = new ActividadInput
            {
                IdProyecto = dto.IdProyecto,
                NombreActividad = dto.NombreActividad,
                Descripcion = dto.Descripcion,
                FechaActividad = dto.FechaActividad,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                Lugar = dto.Lugar,
                CupoMaximo = dto.CupoMaximo,
                EstadoActividad = dto.EstadoActividad
            };

            var proy = await _http.GetFromJsonAsync<List<ProyectoItem>>("/api/Proyectos") ?? new();
            Form = new ActividadFormModel { Input = Input, Proyectos = proy };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var proy = await _http.GetFromJsonAsync<List<ProyectoItem>>("/api/Proyectos") ?? new();
                Form = new ActividadFormModel { Input = Input, Proyectos = proy };
                return Page();
            }

            var resp = await _http.PutAsJsonAsync($"/api/Actividades/{Id}", Input);
            if (resp.IsSuccessStatusCode) return RedirectToPage("Index");

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(error) ? "No se pudo actualizar la actividad." : error);

            var proy2 = await _http.GetFromJsonAsync<List<ProyectoItem>>("/api/Proyectos") ?? new();
            Form = new ActividadFormModel { Input = Input, Proyectos = proy2 };
            return Page();
        }
    }
}
