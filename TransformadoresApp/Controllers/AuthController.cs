using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransformadoresApp.Models;

namespace TransformadoresApp.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) {
                TempData["Error"] = "Debe ingresar correo y contraseña.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, true, lockoutOnFailure: false);

            if (result.Succeeded) return RedirectToAction("Index", "Home");

            TempData["Error"] = "Credenciales inválidas.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword) {
                TempData["Error"] = "Las contraseñas no coinciden.";
                return RedirectToAction("Index", "Home");
            }

            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded) {
                TempData["Success"] = "Cuenta creada correctamente. Ahora podés iniciar sesión.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
