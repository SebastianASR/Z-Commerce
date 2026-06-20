using System.Diagnostics;
using System.Text.Json;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private List<Producto> ObtenerBaseDeDatos()
        {
            return new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Procesador Server-X Pro", Descripcion = "Máxima potencia para bases de datos y virtualización con excelente gestión térmica.", Precio = 349990, ImagenUrl = "https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=600&auto=format&fit=crop&q=60" },
                new Producto { Id = 2, Nombre = "Kit Memoria RAM 32GB", Descripcion = "Velocidad extrema y baja latencia, ideal para cargas de trabajo pesadas y gaming.", Precio = 129990, ImagenUrl = "https://images.unsplash.com/photo-1562976540-1502c2145186?w=600&auto=format&fit=crop&q=60" },
                new Producto { Id = 3, Nombre = "Compuesto Térmico Pro-Cool", Descripcion = "Conductividad superior para mantener tu hardware al máximo rendimiento sin sobrecalentamiento.", Precio = 14990, ImagenUrl = "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?w=600&auto=format&fit=crop&q=60" }
            };
        }

        public IActionResult Index()
        {
            return View(ObtenerBaseDeDatos());
        }

        public IActionResult AgregarAlCarrito(int id)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            List<int> listaIds = string.IsNullOrEmpty(carritoBolsa) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;

            listaIds.Add(id);
            HttpContext.Session.SetString("MiBolsa", JsonSerializer.Serialize(listaIds));

            return RedirectToAction("Index");
        }

        public IActionResult VerCarrito()
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            var productosEnCarrito = new List<Producto>();

            if (!string.IsNullOrEmpty(carritoBolsa))
            {
                var idsGuardados = JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;
                var baseDeDatos = ObtenerBaseDeDatos();

                foreach (var id in idsGuardados)
                {
                    var productoReal = baseDeDatos.FirstOrDefault(p => p.Id == id);
                    if (productoReal != null) productosEnCarrito.Add(productoReal);
                }
            }

            return View(productosEnCarrito);
        }

        // --- NUEVO: ENDPOINT ASÍNCRONO PARA EL CAJÓN LATERAL ---
        public IActionResult GetCarritoOffcanvas(int? idProducto)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            List<int> listaIds = string.IsNullOrEmpty(carritoBolsa) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;

            if (idProducto.HasValue)
            {
                listaIds.Add(idProducto.Value);
                HttpContext.Session.SetString("MiBolsa", JsonSerializer.Serialize(listaIds));
            }

            var baseDeDatos = ObtenerBaseDeDatos();
            var productosEnCarrito = new List<Producto>();

            foreach (var id in listaIds)
            {
                var prod = baseDeDatos.FirstOrDefault(p => p.Id == id);
                if (prod != null) productosEnCarrito.Add(prod);
            }

            return PartialView("_CarritoLateral", productosEnCarrito);
        }

        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}