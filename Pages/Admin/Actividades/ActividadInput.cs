using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Admin.Actividades
{
    public class ActividadInput
    {
        [Required] public int IdProyecto { get; set; }

        [Required, StringLength(120)]
        public string NombreActividad { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime FechaActividad { get; set; } = DateTime.Today;

        [Required, DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; } = new TimeSpan(9, 0, 0);

        [Required, DataType(DataType.Time)]
        public TimeSpan HoraFin { get; set; } = new TimeSpan(11, 0, 0);

        [Required, StringLength(160)]
        public string Lugar { get; set; } = string.Empty;

        [Range(1, 100000)]
        public int CupoMaximo { get; set; } = 20;

        [Required, StringLength(40)]
        public string EstadoActividad { get; set; } = "Programada";
    }

    // Para el combo de proyectos en el form
    public record ProyectoItem(int IdProyecto, string NombreProyecto);

    // Para listar actividades con nombre de proyecto
    public record ActividadListDto(
        int IdActividad,
        int IdProyecto,
        string NombreProyecto,
        string NombreActividad,
        DateTime FechaActividad,
        TimeSpan HoraInicio,
        TimeSpan HoraFin,
        string Lugar,
        int CupoMaximo,
        string EstadoActividad
    );

    // Para detalle/edición
    public record ActividadDetalleDto(
        int IdActividad,
        int IdProyecto,
        string NombreActividad,
        string? Descripcion,
        DateTime FechaActividad,
        TimeSpan HoraInicio,
        TimeSpan HoraFin,
        string Lugar,
        int CupoMaximo,
        string EstadoActividad
    );
}
