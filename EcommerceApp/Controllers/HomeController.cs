using System.Diagnostics;
using System.Text.Json;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Productos.ToList());
        }

        public IActionResult Catalogo(string? buscar, int? categoria, int? orden)
        {
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string termino = buscar.ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(termino) ||
                                         p.Descripcion.ToLower().Contains(termino));
            }

            if (categoria.HasValue && categoria.Value > 0)
                query = query.Where(p => p.CategoriaId == categoria.Value);

            if (orden.HasValue)
            {
                query = orden.Value switch
                {
                    1 => query.OrderBy(p => p.Precio),
                    2 => query.OrderByDescending(p => p.Precio),
                    _ => query
                };
            }

            return View(query.ToList());
        }

        // --- EL MOTOR AGRUPADOR DE LINQ ---
        private List<CarritoItem> ObtenerBolsaAgrupada(List<int> listaIds)
        {
            if (!listaIds.Any()) return new List<CarritoItem>();

            var idsDistintos = listaIds.Distinct().ToList();
            var productosDb = _context.Productos.Where(p => idsDistintos.Contains(p.Id)).ToList();

            return listaIds
                .GroupBy(id => id)
                .Select(grupo => {
                    var prod = productosDb.FirstOrDefault(p => p.Id == grupo.Key);
                    return prod != null ? new CarritoItem { Producto = prod, Cantidad = grupo.Count() } : null;
                })
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();
        }

        public IActionResult AgregarAlCarrito(int id)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            List<int> listaIds = string.IsNullOrEmpty(carritoBolsa) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;
            listaIds.Add(id);
            HttpContext.Session.SetString("MiBolsa", JsonSerializer.Serialize(listaIds));
            return RedirectToAction("Index");
        }

        public IActionResult GetCarritoOffcanvas(int? idProducto)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            List<int> listaIds = string.IsNullOrEmpty(carritoBolsa) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;

            if (idProducto.HasValue)
            {
                listaIds.Add(idProducto.Value);
                HttpContext.Session.SetString("MiBolsa", JsonSerializer.Serialize(listaIds));
            }

            return PartialView("_CarritoLateral", ObtenerBolsaAgrupada(listaIds));
        }

        [HttpGet]
        public IActionResult AsesoriaVip()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AsesoriaVip(SolicitudVip modelo)
        {
            if (ModelState.IsValid)
            {
                modelo.FechaSolicitud = DateTime.UtcNow;
                _context.SolicitudesVip.Add(modelo);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "¡Solicitud guardada con éxito! Nuestro equipo se pondrá en contacto al correo indicado.";
                return RedirectToAction("AsesoriaVip");
            }
            return View(modelo);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult VerSolicitudes()
        {
            var lista = _context.SolicitudesVip.OrderByDescending(x => x.FechaSolicitud).ToList();
            return View(lista);
        }

        // --- MÓDULO DE INVENTARIO (CRUD DE PRODUCTOS) ---
        [Authorize(Roles = "Admin")]
        public IActionResult GestionProductos()
        {
            var productos = _context.Productos.OrderByDescending(p => p.Id).ToList();
            return View(productos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CrearProducto()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CrearProducto(Producto modelo)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Add(modelo);
                await _context.SaveChangesAsync();
                return RedirectToAction("GestionProductos");
            }
            return View(modelo);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult EditarProducto(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditarProducto(Producto modelo)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Update(modelo);
                await _context.SaveChangesAsync();
                return RedirectToAction("GestionProductos");
            }
            return View(modelo);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto != null)
            {
                _context.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("GestionProductos");
        }

        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}