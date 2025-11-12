using TransformadoresApp.Models;

namespace TransformadoresApp.ViewModels
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();

        public string? Search { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}

