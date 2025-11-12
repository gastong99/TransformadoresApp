using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransformadoresApp.Models
{
    public enum ProductionStatus
    {
        Pendiente,
        EnProceso,
        Completada,
        Cancelada
    }

    public class ProductionOrder
    {
        public int ProductionOrderId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un transformador.")]
        [Display(Name = "Transformador")]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required(ErrorMessage = "La cantidad a fabricar es obligatoria.")]
        [Display(Name = "Cantidad a fabricar")]
        [Range(1, 999999, ErrorMessage = "La cantidad debe estar entre 1 y 999.999.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Fecha de orden")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Estado de la orden")]
        public ProductionStatus Status { get; set; } = ProductionStatus.Pendiente;

        [Display(Name = "Eliminada")]
        public bool IsDeleted { get; set; } = false;
    }
}


