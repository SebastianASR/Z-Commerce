using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Webpay Plus NORMAL - Ambiente de integración / sandbox
        private const string CommerceCode = "597055555532";
        private const string ApiKey = "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C";

        // Host oficial integración: https://webpay3gint.transbank.cl
        // Endpoint oficial API REST Webpay Plus v1.2:
        // POST /rswebpaytransaction/api/webpay/v1.2/transactions
        // PUT  /rswebpaytransaction/api/webpay/v1.2/transactions/{token}
        private const string TransbankBaseUrl = "https://webpay3gint.transbank.cl";
        private const string TransbankTransactionsPath = "/rswebpaytransaction/api/webpay/v1.2/transactions";
        private const string TransbankUrl = TransbankBaseUrl + TransbankTransactionsPath;

        public CheckoutController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");

            if (string.IsNullOrEmpty(carritoBolsa))
                return RedirectToAction("Catalogo", "Home");

            var listaIds = JsonSerializer.Deserialize<List<int>>(carritoBolsa) ?? new List<int>();

            if (!listaIds.Any())
                return RedirectToAction("Catalogo", "Home");

            var resumenBolsa = ObtenerBolsaAgrupada(listaIds);
            int totalBolsa = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);

            var modeloPedido = new Pedido
            {
                Total = totalBolsa
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario != null)
                {
                    modeloPedido.UsuarioId = usuario.Id;
                    modeloPedido.Correo = usuario.Email ?? "";
                }
            }

            ViewBag.ResumenBolsa = resumenBolsa;
            return View(modeloPedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Pedido modelo)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");

            if (string.IsNullOrEmpty(carritoBolsa))
                return RedirectToAction("Catalogo", "Home");

            var listaIds = JsonSerializer.Deserialize<List<int>>(carritoBolsa) ?? new List<int>();

            if (!listaIds.Any())
                return RedirectToAction("Catalogo", "Home");

            var resumenBolsa = ObtenerBolsaAgrupada(listaIds);

            ModelState.Remove("Detalles");
            ModelState.Remove("UsuarioId");
            ModelState.Remove("Total");

            if (!ModelState.IsValid)
            {
                ViewBag.ResumenBolsa = resumenBolsa;
                return View(modelo);
            }

            modelo.Total = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);

            if (modelo.Total <= 0)
            {
                ModelState.AddModelError("", "El total del pedido debe ser mayor a cero.");
                ViewBag.ResumenBolsa = resumenBolsa;
                return View(modelo);
            }

            HttpContext.Session.SetString("DatosDespachoTemporal", JsonSerializer.Serialize(modelo));

            string buyOrder = GenerarBuyOrder();
            string sessionId = GenerarSessionId();

            string? returnUrl = Url.Action(
                action: "RetornoWebpay",
                controller: "Checkout",
                values: null,
                protocol: Request.Scheme
            );

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                ModelState.AddModelError("", "No se pudo generar la URL de retorno para Webpay.");
                ViewBag.ResumenBolsa = resumenBolsa;
                return View(modelo);
            }

            var payload = new
            {
                buy_order = buyOrder,
                session_id = sessionId,
                amount = modelo.Total,
                return_url = returnUrl
            };

            using var client = CrearClienteTransbank();

            try
            {
                var response = await client.PostAsJsonAsync(TransbankUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    string detalleError = await response.Content.ReadAsStringAsync();

                    ModelState.AddModelError(
                        "",
                        $"Transbank rechazó la transacción. Código HTTP: {response.StatusCode}. Detalle: {detalleError}"
                    );

                    ViewBag.ResumenBolsa = resumenBolsa;
                    return View(modelo);
                }

                var jsonResult = await response.Content.ReadFromJsonAsync<WebpayCreateResponse>();

                if (jsonResult == null ||
                    string.IsNullOrWhiteSpace(jsonResult.Token) ||
                    string.IsNullOrWhiteSpace(jsonResult.Url))
                {
                    ModelState.AddModelError("", "Transbank respondió, pero no entregó token o URL de pago.");

                    ViewBag.ResumenBolsa = resumenBolsa;
                    return View(modelo);
                }

                ViewBag.TokenWs = jsonResult.Token;
                ViewBag.UrlWebpay = jsonResult.Url;

                return View("IrAPagar");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al conectar con Webpay: " + ex.Message);
            }

            ViewBag.ResumenBolsa = resumenBolsa;
            return View(modelo);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> RetornoWebpay(string? token_ws)
        {
            if (string.IsNullOrWhiteSpace(token_ws))
                return RedirectToAction("PagoRechazado");

            using var client = CrearClienteTransbank();

            try
            {
                string commitUrl = $"{TransbankUrl}/{Uri.EscapeDataString(token_ws)}";

                var response = await client.PutAsync(commitUrl, null);

                if (!response.IsSuccessStatusCode)
                    return RedirectToAction("PagoRechazado");

                var resultado = await response.Content.ReadFromJsonAsync<WebpayCommitResponse>();

                if (resultado == null)
                    return RedirectToAction("PagoRechazado");

                if (resultado.Status == "AUTHORIZED" && resultado.ResponseCode == 0)
                {
                    string? datosTemporales = HttpContext.Session.GetString("DatosDespachoTemporal");
                    string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");

                    if (string.IsNullOrEmpty(datosTemporales) || string.IsNullOrEmpty(carritoBolsa))
                        return RedirectToAction("PagoRechazado");

                    var modeloPedido = JsonSerializer.Deserialize<Pedido>(datosTemporales);
                    var listaIds = JsonSerializer.Deserialize<List<int>>(carritoBolsa) ?? new List<int>();

                    if (modeloPedido == null || !listaIds.Any())
                        return RedirectToAction("PagoRechazado");

                    var resumenBolsa = ObtenerBolsaAgrupada(listaIds);

                    modeloPedido.Total = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);
                    modeloPedido.FechaPedido = DateTime.UtcNow;

                    modeloPedido.Detalles = new List<PedidoDetalle>();

                    foreach (var item in resumenBolsa)
                    {
                        modeloPedido.Detalles.Add(new PedidoDetalle
                        {
                            ProductoId = item.Producto.Id,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.Producto.Precio
                        });
                    }

                    _context.Pedidos.Add(modeloPedido);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove("MiBolsa");
                    HttpContext.Session.Remove("DatosDespachoTemporal");

                    return RedirectToAction("Confirmacion", new { id = modeloPedido.Id });
                }
            }
            catch
            {
                return RedirectToAction("PagoRechazado");
            }

            return RedirectToAction("PagoRechazado");
        }

        [HttpGet]
        public IActionResult PagoRechazado()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Confirmacion(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        private List<CarritoItem> ObtenerBolsaAgrupada(List<int> listaIds)
        {
            if (!listaIds.Any())
                return new List<CarritoItem>();

            var idsDistintos = listaIds.Distinct().ToList();

            var productosDb = _context.Productos
                .Where(p => idsDistintos.Contains(p.Id))
                .ToList();

            return listaIds
                .GroupBy(id => id)
                .Select(g =>
                {
                    var prod = productosDb.FirstOrDefault(p => p.Id == g.Key);

                    return prod != null
                        ? new CarritoItem
                        {
                            Producto = prod,
                            Cantidad = g.Count()
                        }
                        : null;
                })
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();
        }

        private static HttpClient CrearClienteTransbank()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Tbk-Api-Key-Id", CommerceCode);
            client.DefaultRequestHeaders.Add("Tbk-Api-Key-Secret", ApiKey);

            return client;
        }

        private static string GenerarBuyOrder()
        {
            // Transbank pide buy_order único y de largo máximo 26.
            string buyOrder = "ORD" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return buyOrder.Length <= 26
                ? buyOrder
                : buyOrder[..26];
        }

        private string GenerarSessionId()
        {
            // Transbank pide session_id máximo 61 caracteres.
            string sessionId = HttpContext.Session.Id;

            if (string.IsNullOrWhiteSpace(sessionId))
                sessionId = Guid.NewGuid().ToString("N");

            return sessionId.Length <= 61
                ? sessionId
                : sessionId[..61];
        }

        private sealed class WebpayCreateResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = "";

            [JsonPropertyName("url")]
            public string Url { get; set; } = "";
        }

        private sealed class WebpayCommitResponse
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("response_code")]
            public int ResponseCode { get; set; }
        }
    }
}