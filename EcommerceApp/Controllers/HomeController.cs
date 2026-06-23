using System.Diagnostics;
using System.Text;
using System.Text.Json;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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

        // --- MOTOR DE SESIÓN DEL CARRITO ---
        private List<int> ObtenerIdsCarrito()
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");

            if (string.IsNullOrEmpty(carritoBolsa))
                return new List<int>();

            try
            {
                return JsonSerializer.Deserialize<List<int>>(carritoBolsa) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        private void GuardarIdsCarrito(List<int> listaIds)
        {
            if (listaIds.Any())
            {
                HttpContext.Session.SetString("MiBolsa", JsonSerializer.Serialize(listaIds));
            }
            else
            {
                HttpContext.Session.Remove("MiBolsa");
            }
        }

        private List<CarritoItem> ObtenerBolsaAgrupada(List<int> listaIds)
        {
            if (!listaIds.Any()) return new List<CarritoItem>();

            var idsDistintos = listaIds.Distinct().ToList();
            var productosDb = _context.Productos.Where(p => idsDistintos.Contains(p.Id)).ToList();

            return listaIds
                .GroupBy(id => id)
                .Select(grupo =>
                {
                    var prod = productosDb.FirstOrDefault(p => p.Id == grupo.Key);
                    return prod != null ? new CarritoItem { Producto = prod, Cantidad = grupo.Count() } : null;
                })
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();
        }

        public IActionResult AgregarAlCarrito(int id)
        {
            List<int> listaIds = ObtenerIdsCarrito();

            var productoExiste = _context.Productos.Any(p => p.Id == id);

            if (productoExiste)
            {
                listaIds.Add(id);
                GuardarIdsCarrito(listaIds);
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetCarritoOffcanvas(int? idProducto)
        {
            List<int> listaIds = ObtenerIdsCarrito();

            if (idProducto.HasValue)
            {
                var productoExiste = _context.Productos.Any(p => p.Id == idProducto.Value);

                if (productoExiste)
                {
                    listaIds.Add(idProducto.Value);
                    GuardarIdsCarrito(listaIds);
                }
            }

            return PartialView("_CarritoLateral", ObtenerBolsaAgrupada(listaIds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarUnidadCarrito(int id)
        {
            List<int> listaIds = ObtenerIdsCarrito();

            var productoExiste = _context.Productos.Any(p => p.Id == id);

            if (productoExiste)
            {
                listaIds.Add(id);
                GuardarIdsCarrito(listaIds);
            }

            return PartialView("_CarritoLateral", ObtenerBolsaAgrupada(listaIds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuitarUnidadCarrito(int id)
        {
            List<int> listaIds = ObtenerIdsCarrito();

            int indiceProducto = listaIds.FindIndex(x => x == id);

            if (indiceProducto >= 0)
            {
                listaIds.RemoveAt(indiceProducto);
                GuardarIdsCarrito(listaIds);
            }

            return PartialView("_CarritoLateral", ObtenerBolsaAgrupada(listaIds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarProductoCarrito(int id)
        {
            List<int> listaIds = ObtenerIdsCarrito();

            listaIds.RemoveAll(x => x == id);
            GuardarIdsCarrito(listaIds);

            return PartialView("_CarritoLateral", ObtenerBolsaAgrupada(listaIds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VaciarCarrito()
        {
            HttpContext.Session.Remove("MiBolsa");

            return PartialView("_CarritoLateral", new List<CarritoItem>());
        }

        [HttpGet]
        public IActionResult AsesoriaVip()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsesoriaVip(SolicitudVip modelo)
        {
            if (ModelState.IsValid)
            {
                modelo.FechaSolicitud = DateTime.UtcNow;
                modelo.Estado = "Nuevo";
                modelo.Archivada = false;

                _context.SolicitudesVip.Add(modelo);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "¡Solicitud guardada con éxito! Nuestro equipo se pondrá en contacto al correo indicado.";
                return RedirectToAction("AsesoriaVip");
            }

            return View(modelo);
        }

        // --- PANEL DE SOLICITUDES / LEADS ---
        [Authorize(Roles = "Admin,DemoAdmin")]
        public IActionResult VerSolicitudes(bool incluirArchivadas = false)
        {
            var query = _context.SolicitudesVip.AsQueryable();

            if (!incluirArchivadas)
            {
                query = query.Where(x => !x.Archivada);
            }

            var lista = query
                .OrderByDescending(x => x.FechaSolicitud)
                .ToList();

            ViewBag.IncluirArchivadas = incluirArchivadas;

            return View(lista);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarSolicitudContactada(int id)
        {
            var solicitud = await _context.SolicitudesVip.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            solicitud.Estado = "Contactado";
            solicitud.FechaUltimaGestion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La solicitud #{id} fue marcada como contactada.";
            return RedirectToAction("VerSolicitudes");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoSolicitud(int id, string estado)
        {
            var estadosPermitidos = new[] { "Nuevo", "Contactado", "En evaluación", "Cotizado", "Cerrado", "Descartado" };

            if (!estadosPermitidos.Contains(estado))
            {
                TempData["MensajeError"] = "El estado seleccionado no es válido.";
                return RedirectToAction("VerSolicitudes");
            }

            var solicitud = await _context.SolicitudesVip.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            solicitud.Estado = estado;
            solicitud.FechaUltimaGestion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"El estado de la solicitud #{id} fue actualizado.";
            return RedirectToAction("VerSolicitudes");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarNotaSolicitud(int id, string? notaInterna)
        {
            var solicitud = await _context.SolicitudesVip.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            solicitud.NotaInterna = notaInterna;
            solicitud.FechaUltimaGestion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La nota interna de la solicitud #{id} fue guardada.";
            return RedirectToAction("VerSolicitudes");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivarSolicitud(int id)
        {
            var solicitud = await _context.SolicitudesVip.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            solicitud.Archivada = true;
            solicitud.FechaUltimaGestion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La solicitud #{id} fue archivada.";
            return RedirectToAction("VerSolicitudes");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestaurarSolicitud(int id)
        {
            var solicitud = await _context.SolicitudesVip.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            solicitud.Archivada = false;
            solicitud.FechaUltimaGestion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La solicitud #{id} fue restaurada.";
            return RedirectToAction("VerSolicitudes", new { incluirArchivadas = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarSolicitud(int id)
        {
            var solicitud = await _context.SolicitudesVip.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            _context.SolicitudesVip.Remove(solicitud);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La solicitud #{id} fue eliminada permanentemente.";
            return RedirectToAction("VerSolicitudes");
        }

        [Authorize(Roles = "Admin,DemoAdmin")]
        public IActionResult ExportarSolicitudesExcel()
        {
            var solicitudes = _context.SolicitudesVip
                .OrderByDescending(x => x.FechaSolicitud)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Solicitudes");

            string[] encabezados =
            {
        "ID",
        "Empresa",
        "Correo",
        "Tipo de Proyecto",
        "Estado",
        "Archivada",
        "Fecha Solicitud",
        "Fecha Última Gestión",
        "Detalles",
        "Nota Interna"
    };

            for (int i = 0; i < encabezados.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = encabezados[i];
            }

            int fila = 2;

            foreach (var s in solicitudes)
            {
                worksheet.Cell(fila, 1).Value = s.Id;
                worksheet.Cell(fila, 2).Value = s.NombreEmpresa;
                worksheet.Cell(fila, 3).Value = s.Correo;
                worksheet.Cell(fila, 4).Value = s.TipoProyecto;
                worksheet.Cell(fila, 5).Value = string.IsNullOrWhiteSpace(s.Estado) ? "Nuevo" : s.Estado;
                worksheet.Cell(fila, 6).Value = s.Archivada ? "Sí" : "No";
                worksheet.Cell(fila, 7).Value = s.FechaSolicitud.ToLocalTime();

                if (s.FechaUltimaGestion.HasValue)
                {
                    worksheet.Cell(fila, 8).Value = s.FechaUltimaGestion.Value.ToLocalTime();
                }
                else
                {
                    worksheet.Cell(fila, 8).Value = "Sin gestión";
                }

                worksheet.Cell(fila, 9).Value = s.Detalles;
                worksheet.Cell(fila, 10).Value = string.IsNullOrWhiteSpace(s.NotaInterna) ? "Sin nota interna" : s.NotaInterna;

                fila++;
            }

            var rango = worksheet.Range(1, 1, Math.Max(fila - 1, 1), encabezados.Length);
            var tabla = rango.CreateTable("TablaSolicitudesZCommerce");
            tabla.Theme = XLTableTheme.TableStyleMedium2;

            worksheet.SheetView.FreezeRows(1);

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 28;
            worksheet.Column(3).Width = 32;
            worksheet.Column(4).Width = 32;
            worksheet.Column(5).Width = 18;
            worksheet.Column(6).Width = 14;
            worksheet.Column(7).Width = 22;
            worksheet.Column(8).Width = 24;
            worksheet.Column(9).Width = 45;
            worksheet.Column(10).Width = 45;

            worksheet.Columns(7, 8).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

            worksheet.Columns(9, 10).Style.Alignment.WrapText = true;
            worksheet.Columns().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var contenido = stream.ToArray();

            return File(
                contenido,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"solicitudes-zcommerce-{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }

        // --- MÓDULO DE INVENTARIO (CRUD DE PRODUCTOS) ---
        [Authorize(Roles = "Admin,DemoAdmin")]
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
        [ValidateAntiForgeryToken]
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

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        [HttpGet]
        public IActionResult QuienesSomos()
        {
            return View();
        }
    }
}

