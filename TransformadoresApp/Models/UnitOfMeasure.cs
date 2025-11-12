using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models
{
    public class UnitOfMeasure
    {
        public int UnitOfMeasureId { get; set; }

        [Required(ErrorMessage = "El nombre de la unidad es obligatorio.")]
        [StringLength(20, ErrorMessage = "El nombre de la unidad no puede tener más de 20 caracteres.")]
        [Display(Name = "Unidad de medida")]
        public string Name { get; set; } = null!;
    }
}
