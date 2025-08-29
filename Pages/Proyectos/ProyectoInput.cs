// Pages/Proyectos/ProyectoInput.cs
using System.ComponentModel.DataAnnotations;

namespace FrontendProyecto.Pages.Proyectos
{
    public enum EstadoProyectoEnum
    {
        Activo,
        Finalizado,
        Cancelado
    }

    public class ProyectoInput : IValidatableObject
    {
        [Required(ErrorMessage = "Debe seleccionar una ONG")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una ONG válida")]
        public int IdOng { get; set; }

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio")]
        [StringLength(150)]
        public string NombreProyecto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; } = DateTime.Today;

        [Required]
        public EstadoProyectoEnum EstadoProyecto { get; set; } = EstadoProyectoEnum.Activo;

        [Required(ErrorMessage = "Debe seleccionar un responsable")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un responsable válido")]
        public int IdResponsable { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin < FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha de fin no puede ser anterior a la fecha de inicio",
                    new[] { nameof(FechaFin) }
                );
            }
        }
    }
}
