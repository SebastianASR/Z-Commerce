using EcommerceApp.Data; // Para el sembrador de roles
using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity; // Para el sistema de usuarios
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. RESERVAMOS MEMORIA RAM PARA GUARDAR EL CARRITO DE CADA USUARIO:
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // El carrito dura 30 mins si el usuario se va a hacer un café
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 2. CONEXIÓN A POSTGRESQL EN NEON
var connectionString = builder.Configuration.GetConnectionString("NeonConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. CONFIGURACIÓN DE REGLAS DE ACCESO (IDENTITY)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Bajamos los requisitos de Microsoft para poder hacer pruebas rápidas en desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. CONFIGURACIÓN DE COOKIES Y REDIRECCIÓN DE SEGURIDAD
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Si intentan comprar sin loguearse, rebotan acá
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// 5. DISPARAR EL SEMBRADOR DE ROLES AUTOMÁTICO AL ENCENDER
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Llama a la clase que creaste en Data/DbInitializer.cs para inyectar al Admin
    await DbInitializer.SeedRolesAndAdmin(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 6. ENCENDEMOS LOS GUARDIAS DE SEGURIDAD (Estrictamente en este orden)
app.UseAuthentication(); // <- 1ro: Verifica QUIÉN eres (Login)
app.UseAuthorization();  // <- 2do: Verifica QUÉ puedes hacer (Permisos)

// 7. ENCENDEMOS EL MOTOR DE SESIONES
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();