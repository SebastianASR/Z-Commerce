using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckoutController> _logger;

        // Webpay Plus NORMAL - Ambiente de integración / sandbox
        private const string CommerceCode = "597055555532";
        private const string ApiKey = "579B532A7440BB0C9079DED94D31EA1615BACEB56610332264630D42D0A36B1C";

        private const string TransbankBaseUrl = "https://webpay3gint.transbank.cl";
        private const string TransbankTransactionsPath = "/rswebpaytransaction/api/webpay/v1.2/transactions";
        private const string TransbankUrl = TransbankBaseUrl + TransbankTransactionsPath;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<CheckoutController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listaIds = ObtenerIdsCarrito();

            if (!listaIds.Any())
                return RedirectToAction("Catalogo", "Home");

            var resumenBolsa = ObtenerBolsaAgrupada(listaIds);

            if (!resumenBolsa.Any())
                return RedirectToAction("Catalogo", "Home");

            var modelo = new CheckoutViewModel
            {
                ResumenBolsa = resumenBolsa,
                Total = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad),
                EsUsuarioAutenticado = User.Identity?.IsAuthenticated == true
            };

            if (modelo.EsUsuarioAutenticado)
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario != null)
                {
                    modelo.Nombre = usuario.Nombre ?? "";
                    modelo.Apellido = usuario.Apellido ?? "";
                    modelo.Correo = usuario.Email ?? "";
                    modelo.Telefono = usuario.TelefonoContacto ?? usuario.PhoneNumber ?? "";
                    modelo.Region = usuario.Region ?? "";
                    modelo.Comuna = usuario.Comuna ?? "";
                    modelo.Calle = usuario.Calle ?? "";
                    modelo.Numero = usuario.Numero ?? "";
                    modelo.DeptoBlockOficina = usuario.DeptoBlockOficina;
                    modelo.GuardarDatosEnCuenta = true;
                }
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel modelo)
        {
            var listaIds = ObtenerIdsCarrito();

            if (!listaIds.Any())
                return RedirectToAction("Catalogo", "Home");

            var resumenBolsa = ObtenerBolsaAgrupada(listaIds);

            if (!resumenBolsa.Any())
                return RedirectToAction("Catalogo", "Home");

            modelo.ResumenBolsa = resumenBolsa;
            modelo.Total = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);
            modelo.EsUsuarioAutenticado = User.Identity?.IsAuthenticated == true;

            ModelState.Remove(nameof(CheckoutViewModel.ResumenBolsa));
            ModelState.Remove(nameof(CheckoutViewModel.Total));
            ModelState.Remove(nameof(CheckoutViewModel.EsUsuarioAutenticado));

            ApplicationUser? usuario = null;
            string? usuarioId = null;
            string tipoCliente = "Invitado";

            if (!modelo.EsUsuarioAutenticado && modelo.CrearCuenta)
            {
                if (string.IsNullOrWhiteSpace(modelo.Password))
                {
                    ModelState.AddModelError(nameof(modelo.Password), "Debes ingresar una contraseña para crear la cuenta.");
                }

                if (string.IsNullOrWhiteSpace(modelo.ConfirmPassword))
                {
                    ModelState.AddModelError(nameof(modelo.ConfirmPassword), "Debes confirmar la contraseña.");
                }

                if (modelo.Password != modelo.ConfirmPassword)
                {
                    ModelState.AddModelError(nameof(modelo.ConfirmPassword), "Las contraseñas no coinciden.");
                }

                var existeUsuario = await _userManager.FindByEmailAsync(modelo.Correo);

                if (existeUsuario != null)
                {
                    ModelState.AddModelError(nameof(modelo.Correo), "Ya existe una cuenta con este correo. Inicia sesión para continuar.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            if (modelo.EsUsuarioAutenticado)
            {
                usuario = await _userManager.GetUserAsync(User);

                if (usuario != null)
                {
                    usuarioId = usuario.Id;
                    tipoCliente = "Cliente registrado";

                    if (modelo.GuardarDatosEnCuenta)
                    {
                        usuario.Nombre = modelo.Nombre;
                        usuario.Apellido = modelo.Apellido;
                        usuario.PhoneNumber = modelo.Telefono;
                        usuario.TelefonoContacto = modelo.Telefono;
                        usuario.Region = modelo.Region;
                        usuario.Comuna = modelo.Comuna;
                        usuario.Calle = modelo.Calle;
                        usuario.Numero = modelo.Numero;
                        usuario.DeptoBlockOficina = modelo.DeptoBlockOficina;

                        await _userManager.UpdateAsync(usuario);
                    }
                }
            }
            else if (modelo.CrearCuenta)
            {
                usuario = new ApplicationUser
                {
                    UserName = modelo.Correo,
                    Email = modelo.Correo,
                    EmailConfirmed = false,

                    Nombre = modelo.Nombre,
                    Apellido = modelo.Apellido,
                    PhoneNumber = modelo.Telefono,
                    TelefonoContacto = modelo.Telefono,

                    Region = modelo.Region,
                    Comuna = modelo.Comuna,
                    Calle = modelo.Calle,
                    Numero = modelo.Numero,
                    DeptoBlockOficina = modelo.DeptoBlockOficina,

                    FechaRegistro = DateTime.UtcNow
                };

                var resultadoCrearUsuario = await _userManager.CreateAsync(usuario, modelo.Password!);

                if (!resultadoCrearUsuario.Succeeded)
                {
                    foreach (var error in resultadoCrearUsuario.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(modelo);
                }

                await _userManager.AddToRoleAsync(usuario, "Cliente");

                try
                {
                    await EnviarCorreoConfirmacionAsync(usuario);

                    TempData["SuccessMessage"] =
                        "Tu cuenta fue creada correctamente. Antes de pagar, confirma tu correo e inicia sesión para continuar con la compra.";

                    return RedirectToAction("RegisterConfirmation", "Account", new { email = usuario.Email });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando correo de confirmación desde checkout para {Email}", usuario.Email);

                    TempData["ErrorMessage"] =
                        "Tu cuenta fue creada, pero no pudimos enviar el correo de confirmación. Solicita un nuevo enlace para activar tu cuenta.";

                    return RedirectToAction("ResendEmailConfirmation", "Account");
                }
            }

            if (modelo.Total <= 0)
            {
                ModelState.AddModelError("", "El total del pedido debe ser mayor a cero.");
                return View(modelo);
            }

            string buyOrder = GenerarBuyOrder();
            string sessionId = GenerarSessionId();

            var pedidoTemporal = new Pedido
            {
                UsuarioId = usuarioId,
                NombreCompleto = $"{modelo.Nombre} {modelo.Apellido}".Trim(),
                Correo = modelo.Correo,
                Telefono = modelo.Telefono,
                Region = modelo.Region,
                Comuna = modelo.Comuna,
                Calle = modelo.Calle,
                Numero = modelo.Numero,
                DeptoBlockOficina = modelo.DeptoBlockOficina,
                ComentarioCliente = modelo.ComentarioCliente,
                TipoCliente = tipoCliente,
                EstadoPago = "Pendiente Webpay",
                EstadoPedido = "Pendiente",
                BuyOrder = buyOrder,
                Total = modelo.Total,
                FechaPedido = DateTime.UtcNow
            };

            HttpContext.Session.SetString("DatosDespachoTemporal", JsonSerializer.Serialize(pedidoTemporal));
            string? returnUrl = Url.Action(
    action: "RetornoWebpay",
    controller: "Checkout",
    values: null,
    protocol: Request.Scheme
);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                ModelState.AddModelError("", "No se pudo generar la URL de retorno para Webpay.");
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

                    return View(modelo);
                }

                var jsonResult = await response.Content.ReadFromJsonAsync<WebpayCreateResponse>();

                if (jsonResult == null ||
                    string.IsNullOrWhiteSpace(jsonResult.Token) ||
                    string.IsNullOrWhiteSpace(jsonResult.Url))
                {
                    ModelState.AddModelError("", "Transbank respondió, pero no entregó token o URL de pago.");
                    return View(modelo);
                }

                pedidoTemporal.WebpayToken = jsonResult.Token;
                HttpContext.Session.SetString("DatosDespachoTemporal", JsonSerializer.Serialize(pedidoTemporal));

                ViewBag.TokenWs = jsonResult.Token;
                ViewBag.UrlWebpay = jsonResult.Url;

                return View("IrAPagar");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al conectar con Webpay: " + ex.Message);
            }

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

                    if (!resumenBolsa.Any())
                        return RedirectToAction("PagoRechazado");

                    modeloPedido.Total = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);
                    modeloPedido.FechaPedido = DateTime.UtcNow;
                    modeloPedido.EstadoPago = "Autorizado";
                    modeloPedido.EstadoPedido = "Pagado";
                    modeloPedido.WebpayToken = token_ws;

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

        private async Task EnviarCorreoConfirmacionAsync(ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("El usuario no tiene correo electrónico.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new
                {
                    userId = user.Id,
                    token
                },
                protocol: Request.Scheme
            );

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new InvalidOperationException("No se pudo generar el enlace de confirmación.");
            }

            var safeUrl = WebUtility.HtmlEncode(callbackUrl);
            var safeName = WebUtility.HtmlEncode(user.Nombre ?? "cliente");

            var html = $@"
                <div style='font-family:Arial,sans-serif;max-width:620px;margin:auto;background:#f8fafc;padding:24px;border-radius:18px;border:1px solid #e2e8f0;'>
                    <div style='background:#0f172a;color:#ffffff;padding:22px;border-radius:16px;text-align:center;'>
                        <h1 style='margin:0;font-size:26px;'>Z-Commerce</h1>
                        <p style='margin:8px 0 0;color:#93c5fd;'>Confirmación de cuenta</p>
                    </div>

                    <div style='padding:24px 4px;color:#0f172a;'>
                        <h2 style='margin-top:0;'>Hola {safeName},</h2>

                        <p style='font-size:15px;line-height:1.6;'>
                            Gracias por registrarte en <strong>Z-Commerce</strong>.
                            Antes de continuar con tu compra, debes confirmar tu correo electrónico.
                        </p>

                        <div style='text-align:center;margin:30px 0;'>
                            <a href='{safeUrl}'
                               style='background:#0d6efd;color:#ffffff;text-decoration:none;padding:14px 24px;border-radius:12px;font-weight:bold;display:inline-block;'>
                                Confirmar mi cuenta
                            </a>
                        </div>

                        <p style='font-size:13px;color:#64748b;line-height:1.6;'>
                            Después de confirmar tu correo, inicia sesión y vuelve al checkout para finalizar tu compra.
                        </p>

                        <p style='font-size:13px;color:#64748b;line-height:1.6;'>
                            Si el botón no funciona, copia y pega este enlace en tu navegador:
                        </p>

                        <p style='font-size:12px;word-break:break-all;color:#2563eb;'>
                            {safeUrl}
                        </p>

                        <hr style='border:none;border-top:1px solid #e2e8f0;margin:24px 0;' />

                        <p style='font-size:12px;color:#94a3b8;margin:0;'>
                            Si no creaste esta cuenta, puedes ignorar este mensaje.
                        </p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(
                user.Email,
                "Confirma tu cuenta en Z-Commerce",
                html
            );
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
            string buyOrder = "ORD" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return buyOrder.Length <= 26
                ? buyOrder
                : buyOrder[..26];
        }

        private string GenerarSessionId()
        {
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