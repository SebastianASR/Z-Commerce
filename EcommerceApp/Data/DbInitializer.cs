using Microsoft.AspNetCore.Identity;

namespace EcommerceApp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Crear los roles esenciales en la base de datos si no existen
            string[] roles = { "Admin", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Crear el Administrador del e-commerce por defecto
            var adminEmail = "admin@zcommerce.cl";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Le inyectamos una contraseña fuerte inicial
                var resultado = await userManager.CreateAsync(admin, "ZCommerce2026!");
                if (resultado.Succeeded)
                {
                    // Lo coronamos como Admin oficial
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}