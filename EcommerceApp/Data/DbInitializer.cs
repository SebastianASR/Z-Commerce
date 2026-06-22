using Microsoft.AspNetCore.Identity;

namespace EcommerceApp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Roles base del sistema.
            // Admin existe para tu cuenta real, pero NO se crea ningún usuario Admin automático.
            string[] roles = { "Admin", "DemoAdmin", "Cliente" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Usuario DemoAdmin público para reclutadores.
            // Puede revisar el panel de inventario, pero no modificar datos críticos.
            var demoAdminEmail = "demo.admin@zcommerce.cl";
            var demoAdminUser = await userManager.FindByEmailAsync(demoAdminEmail);

            if (demoAdminUser == null)
            {
                var demoAdmin = new IdentityUser
                {
                    UserName = demoAdminEmail,
                    Email = demoAdminEmail,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(demoAdmin, "DemoAdmin123!");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoAdmin, "DemoAdmin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(demoAdminUser, "DemoAdmin"))
                {
                    await userManager.AddToRoleAsync(demoAdminUser, "DemoAdmin");
                }
            }

            // Usuario cliente demo para probar carrito, checkout y flujo normal.
            var clienteEmail = "cliente@zcommerce.cl";
            var clienteUser = await userManager.FindByEmailAsync(clienteEmail);

            if (clienteUser == null)
            {
                var cliente = new IdentityUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(cliente, "Cliente123!");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(cliente, "Cliente");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(clienteUser, "Cliente"))
                {
                    await userManager.AddToRoleAsync(clienteUser, "Cliente");
                }
            }
        }
    }
}