using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models;

namespace TransformadoresApp.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Crear roles si no existen 
            string[] roles = new[] { "Administrador", "Operario" };
            foreach (var role in roles) {
                if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Crear usuario administrador por defecto
            var adminEmail = "admin@rostagno.com";
            var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (adminUser == null) {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded) {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                }
                else {
                    Console.WriteLine("❌ Error al crear el usuario administrador: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else {
                var rolesUser = await userManager.GetRolesAsync(adminUser);

                if (!rolesUser.Contains("Administrador")) await userManager.AddToRoleAsync(adminUser, "Administrador");
            }

            // Crear usuario operario de prueba
            var operarioEmail = "operario@rostagno.com";
            var operarioUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == operarioEmail);

            if (operarioUser != null) {
                var rolesUser = await userManager.GetRolesAsync(operarioUser);

                if (!rolesUser.Contains("Operario")) {
                    await userManager.AddToRoleAsync(operarioUser, "Operario");
                    Console.WriteLine("Usuario operario asignado al rol correctamente.");
                }
            }
        }
    }
}
