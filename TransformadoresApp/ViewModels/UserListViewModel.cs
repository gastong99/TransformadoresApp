using static TransformadoresApp.Controllers.AdminController;

namespace TransformadoresApp.ViewModels
{
    public class UserListViewModel
    {
        public List<UserWithRolesViewModel> Users { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
