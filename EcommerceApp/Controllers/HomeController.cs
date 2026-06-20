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

        // 1. SACAMOS LA BASE DE DATOS AFUERA (Ahora cualquier método de este archivo puede leerla)
        private List<Producto> ObtenerBaseDeDatos()
        {
            return new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Procesador Server-X Pro", Descripcion = "Máxima potencia para bases de datos y virtualización con excelente gestión térmica.", Precio = 349990, ImagenUrl = "https://placehold.co/400x250/212529/FFFFFF?text=Procesador+Server-X" },
                new Producto { Id = 2, Nombre = "Kit Memoria RAM 32GB", Descripcion = "Velocidad extrema y baja latencia, ideal para cargas de trabajo pesadas y gaming.", Precio = 129990, ImagenUrl = "https://placehold.co/400x250/212529/FFFFFF?text=Memoria+RAM+32GB" },
                new Producto { Id = 3, Nombre = "Compuesto Térmico Pro-Cool", Descripcion = "Conductividad superior para mantener tu hardware al máximo rendimiento sin sobrecalentamiento.", Precio = 14990, ImagenUrl = "https://placehold.co/400x250/212529/FFFFFF?text=Pasta+Termica" }
            };
        }

        public IActionResult Index()
        {
            // El index ahora simplemente llama a la base de datos de arriba
            return View(ObtenerBaseDeDatos());
        }

        public IActionResult AgregarAlCarrito(int id)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            List<int> listaIds;

            if (string.IsNullOrEmpty(carritoBolsa))
            {
                listaIds = new List<int>();
            }
            else
            {
                listaIds = JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;
            }

            listaIds.Add(id);
            HttpContext.Session.SetString("MiBolsa", JsonSerializer.Serialize(listaIds));

            return RedirectToAction("Index");
        }

        // --- NUEVO: EL MÉTODO QUE ABRE LA BOLSA ---
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
                    // LINQ: "Busca en la base de datos el primer producto que coincida con este ID"
                    var productoReal = baseDeDatos.FirstOrDefault(p => p.Id == id);
                    if (productoReal != null)
                    {
                        productosEnCarrito.Add(productoReal);
                    }
                }
            }

            return View(productosEnCarrito);
        }
        // ------------------------------------------

        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}