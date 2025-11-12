using TransformadoresApp.Models;

namespace TransformadoresApp.ViewModels
{
    public class ProductionOrderListViewModel
    {
        public List<ProductionOrder> Orders { get; set; } = new();


        public string? Estado { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? ProductId { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
