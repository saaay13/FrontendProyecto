using System.Collections.Generic;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    public class ActividadFormModel
    {
        public ActividadInput Input { get; set; } = new();
        public List<ProyectoItem> Proyectos { get; set; } = new();
    }
}
