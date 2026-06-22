using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EcommerceApp.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(
            HttpClient httpClient,
            IOptions<EmailSettings> settings,
            ILogger<BrevoEmailService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException("La API Key de Brevo no está configurada.");
            }

            if (string.IsNullOrWhiteSpace(_settings.From))
            {
                throw new InvalidOperationException("El correo remitente de Brevo no está configurado.");
            }

            var payload = new
            {
                sender = new
                {
                    name = _settings.DisplayName,
                    email = _settings.From
                },
                to = new[]
                {
                    new
                    {
                        email = to
                    }
                },
                subject = subject,
                htmlContent = htmlBody
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email"
            );

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            request.Headers.Add("api-key", _settings.ApiKey);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Brevo no pudo enviar el correo. StatusCode: {StatusCode}. Response: {Response}",
                    response.StatusCode,
                    responseBody
                );

                throw new InvalidOperationException(
                    $"Brevo no pudo enviar el correo. Código: {(int)response.StatusCode}. Respuesta: {responseBody}"
                );
            }
        }
    }
}