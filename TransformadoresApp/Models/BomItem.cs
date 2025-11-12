using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransformadoresApp.Models
{
    public class BomItem
    {
        public int BomItemId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un transformador.")]
        [Display(Name = "Transformador")]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un material.")]
        [Display(Name = "Material")]
        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material? Material { get; set; }

        [Required(ErrorMessage = "La cantidad por unidad es obligatoria.")]
        [Range(0.0001, 999999, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Display(Name = "Cantidad por unidad")]
        public decimal? QuantityPerUnit { get; set; }
    }
}

