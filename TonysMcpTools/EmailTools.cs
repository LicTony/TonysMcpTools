using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TonysMcpTools.Utiles;

namespace TonysMcpTools
{
    [McpServerToolType]
    public static class EmailTools
    {
        /// <summary>
        /// Testea los parámetros de configuración de email.
        /// </summary>
        [McpServerTool, Description(
            "Testea los parámetros de configuración de email (EmailBaseUrl, EmailApiKey, EmailFrom) " +
            "y verifica que los valores estén correctamente configurados.")]
        public static async Task<string> TestearConfiguracionEmailAsync()
        {
            try
            {
                var maskedApiKey = MaskApiKey(GlobalConfig.EmailApiKey);
                
                var configDetails = new
                {
                    EmailBaseUrl = GlobalConfig.EmailBaseUrl,
                    EmailFrom = GlobalConfig.EmailFrom,
                    EmailApiKey = maskedApiKey
                };

                if (string.IsNullOrWhiteSpace(GlobalConfig.EmailBaseUrl) ||
                    string.IsNullOrWhiteSpace(GlobalConfig.EmailApiKey) ||
                    string.IsNullOrWhiteSpace(GlobalConfig.EmailFrom))
                {
                    return JsonSerializer.Serialize(new
                    {
                        ok = false,
                        mensaje = "Faltan parámetros de configuración.",
                        configuracion = configDetails
                    }, _jsonOptions);
                }

                return JsonSerializer.Serialize(new
                {
                    ok = true,
                    mensaje = "Parámetros de configuración validados correctamente.",
                    configuracion = configDetails
                }, _jsonOptions);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(TestearConfiguracionEmailAsync)}: {ex.Message}";
                return JsonSerializer.Serialize(new { ok = false, error = mensajeError }, _jsonOptions);
            }
        }

        private static string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
                return "****";

            return $"{apiKey.Substring(0, 4)}...{apiKey.Substring(apiKey.Length - 4)}";
        }


        /// <summary>
        /// Envía un email usando la Plataforma de Notificaciones.
        /// </summary>
        [McpServerTool, Description(
            "Envía un email a los destinatarios configurados usando la Plataforma de Notificaciones. " +
            "Parámetros: 'mensaje' es el cuerpo del email (texto plano), 'subject' es el asunto, y 'emailTo' como destinatario específico. " +
            "Usar cuando se necesita notificar por email.")]
        public static async Task<string> EnviarEmailAsync(
            [Description("Cuerpo del email en texto plano.")] string mensaje,
            [Description("Asunto del email.")] string subject,
            [Description("Destinatario del email.")] string emailTo)
        {
            try
            {
                var client = new NotificationClient(GlobalConfig.EmailBaseUrl, GlobalConfig.EmailApiKey);

                var emailNotification = new EmailNotification
                {
                    NotificationList =
                    [
                        new EmailItem
                        {
                            From    = GlobalConfig.EmailFrom,
                            To      = [emailTo],
                            Subject = subject,
                            TextBody = mensaje
                        }
                    ]
                };

                await client.SendEmailV1Async(emailNotification);

                return JsonSerializer.Serialize(new
                {
                    ok = true,
                    mensaje = "Email enviado correctamente.",
                    subject,
                    destinatarios = new List<string> { emailTo }
                }, _jsonOptions);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(EnviarEmailAsync)}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(mensajeError);
                return JsonSerializer.Serialize(new { error = mensajeError });
            }
        }

        #region MetodosPrivadosAuxiliares

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        #endregion
    }

    #region Notifications (replicado del ejemplo XcomAutoridades)

    /// <summary>
    /// Cliente HTTP para la Plataforma de Notificaciones.
    /// </summary>
    internal class NotificationClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _producerBaseUrl;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public NotificationClient(string producerBaseUrl, string apiKey)
        {
            _producerBaseUrl = producerBaseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
        }

        /// <summary>
        /// V1: Envío de Email (204 No Content).
        /// </summary>
        public async Task SendEmailV1Async(EmailNotification notification)
        {
            var url = $"{_producerBaseUrl}/v1/notifications/email/send";
            var response = await PostAsync(url, notification);
            response.EnsureSuccessStatusCode();
        }

        private async Task<HttpResponseMessage> PostAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(url, content);
        }
    }

    internal class EmailNotification
    {
        [JsonPropertyName("notificationList")]
        public List<EmailItem> NotificationList { get; set; } = [];
    }

    internal class EmailItem
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("nameSender")]
        public string NameSender { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public List<string> To { get; set; } = [];

        [JsonPropertyName("cc")]
        public List<string> Cc { get; set; } = [];

        [JsonPropertyName("bcc")]
        public List<string> Bcc { get; set; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("textBody")]
        public string TextBody { get; set; } = string.Empty;

        [JsonPropertyName("htmlBody")]
        public string HtmlBody { get; set; } = string.Empty;

        [JsonPropertyName("importance")]
        public string Importance { get; set; } = string.Empty;
    }

    #endregion
}
