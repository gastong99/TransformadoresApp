using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransformadoresApp.Models;
using TransformadoresApp.ViewModels;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private const int PageSize = 10;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> UserManagement(int page = 1)
        {
            var users = _userManager.Users.ToList();
            int totalUsers = users.Count;

            var pagedUsers = users
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var list = new List<UserWithRolesViewModel>();
            foreach (var user in pagedUsers) {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            var vm = new UserListViewModel
            {
                Users = list,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)PageSize)
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(role)) {
                if (!await _userManager.IsInRoleAsync(user, role)) await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, role)) await _userManager.RemoveFromRoleAsync(user, role);

            return RedirectToAction(nameof(UserManagement));
        }

        public class UserWithRolesViewModel
        {
            public string UserId { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; } = new();
        }
    }
}
