using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TonysMcpTools.Classes;
using TonysMcpTools.Utiles;

namespace TonysMcpTools
{    
    [McpServerToolType]
    public static class ApiTempoTools
    {
        /// <summary>
        /// Obtiene los worklogs registrados por el usuario configurado en un rango de fechas.
        /// </summary>
        [McpServerTool, Description(
            "Obtiene los worklogs (horas registradas) del usuario en Tempo para un rango de fechas. " +
            "Parámetros: 'fechaDesde' y 'fechaHasta' en formato YYYY-MM-DD. " +
            "Retorna por cada worklog: el issue de Jira asociado, las horas registradas, " +
            "si son horas facturables, la fecha, hora de inicio y descripción. " +
            "Usar cuando se necesita saber qué horas cargó el usuario, en qué issues trabajó " +
            "o calcular el total de horas en un período.")]
        public static async Task<string> TempoObtenerMisWorklogsAsync(
            [Description("Fecha de inicio del rango. Formato: YYYY-MM-DD. Ejemplo: 2025-03-01")] string fechaDesde,
            [Description("Fecha de fin del rango. Formato: YYYY-MM-DD. Ejemplo: 2025-03-15")] string fechaHasta)
        {
            string accountId = GlobalConfig.TempoAccountId;
            string apiUrl = $"{GlobalConfig.TempoBaseUrl}/worklogs/user/{accountId}" +
                            $"?from={fechaDesde}&to={fechaHasta}&limit=1000";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                string jsonRaw = await ProcesarRespuestaAsync(response, nameof(TempoObtenerMisWorklogsAsync));

                var paged = JsonSerializer.Deserialize<TempoPagedResponse<TempoWorklog>>(jsonRaw, _jsonOptions);

                if (paged is null || paged.Results.Count == 0)
                    return JsonSerializer.Serialize(new { mensaje = "No se encontraron worklogs para el período indicado." });

                // Calculamos el total de horas para darle un resumen útil al modelo
                int totalSegundos = paged.Results.Sum(w => w.TimeSpentSeconds);

                var resultado = new
                {
                    periodo = new { desde = fechaDesde, hasta = fechaHasta },
                    totalWorklogs = paged.Results.Count,
                    totalHorasFormateadas = FormatearHoras(totalSegundos),
                    worklogs = paged.Results.Select(w => new
                    {
                        id = w.TempoWorklogId,
                        issue = ObtenerIssueLabel(w.Issue),
                        fecha = w.StartDate ?? "N/A",
                        horaInicio = w.StartTime ?? "N/A",
                        horasRegistradas = FormatearHoras(w.TimeSpentSeconds),
                        horasFacturables = FormatearHoras(w.BillableSeconds),
                        descripcion = w.Description ?? "Sin descripción"
                    })
                };

                return JsonSerializer.Serialize(resultado, _jsonOptions);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(TempoObtenerMisWorklogsAsync)}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(mensajeError);
                return JsonSerializer.Serialize(new { error = mensajeError });
            }
        }


        /// <summary>
        /// Obtiene el estado de aprobación del timesheet del usuario para un período.
        /// </summary>
        [McpServerTool, Description(
            "Obtiene el estado de aprobación del timesheet del usuario en Tempo para un período. " +
            "Parámetros: 'fechaDesde' y 'fechaHasta' en formato YYYY-MM-DD. " +
            "Retorna: estado del timesheet (OPEN, SUBMITTED, APPROVED, REJECTED), " +
            "horas requeridas vs horas registradas y comentario del aprobador si existe. " +
            "Usar cuando se necesita saber si el timesheet está aprobado, pendiente o rechazado, " +
            "o verificar si el usuario cumplió con las horas requeridas en el período.")]
        public static async Task<string> TempoObtenerMiTimesheetAsync(
            [Description("Fecha de inicio del período. Formato: YYYY-MM-DD. Ejemplo: 2025-03-01")] string fechaDesde,
            [Description("Fecha de fin del período. Formato: YYYY-MM-DD. Ejemplo: 2025-03-31")] string fechaHasta)
        {
            string accountId = GlobalConfig.TempoAccountId;
            string apiUrl = $"{GlobalConfig.TempoBaseUrl}/timesheet-approvals/user/{accountId}" +
                            $"?from={fechaDesde}&to={fechaHasta}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                string jsonRaw = await ProcesarRespuestaAsync(response, nameof(TempoObtenerMiTimesheetAsync));

                var approval = JsonSerializer.Deserialize<TempoTimesheetApproval>(jsonRaw, _jsonOptions);

                if (approval is null)
                    return JsonSerializer.Serialize(new { mensaje = "No se encontró información del timesheet para el período indicado." });

                var resultado = new
                {
                    periodo = new
                    {
                        desde = approval.Period?.From ?? fechaDesde,
                        hasta = approval.Period?.To ?? fechaHasta
                    },
                    estado = approval.Status?.Key ?? "N/A",
                    comentarioAprobador = approval.Status?.Comment ?? "Sin comentario",
                    horasRequeridas = FormatearHoras(approval.RequiredSeconds),
                    horasRegistradas = FormatearHoras(approval.TimeSpentSeconds),
                    diferencia = FormatearHoras(approval.TimeSpentSeconds - approval.RequiredSeconds)
                };

                return JsonSerializer.Serialize(resultado, _jsonOptions);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(TempoObtenerMiTimesheetAsync)}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(mensajeError);
                return JsonSerializer.Serialize(new { error = mensajeError });
            }
        }

        /// <summary>
        /// Testea la conexión a la API de Tempo.
        /// </summary>
        [McpServerTool, Description(
            "Testea la conexión a la API de Tempo de forma rápida. " +
            "Retorna éxito si la conexión funciona correctamente. " +
            "Si falla, provee información de diagnóstico (usuario, url, snippet de la API key).")]
        public static async Task<string> TempoTestearConexionAsync()
        {
            string accountId = GlobalConfig.TempoAccountId;
            string token = GlobalConfig.TempoToken ?? string.Empty;
            string apiUrl = $"{GlobalConfig.TempoBaseUrl}/worklogs/user/{accountId}?limit=1";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Serialize(new { mensaje = "Conexión a la API de Tempo exitosa.", url = apiUrl }, _jsonOptions);
                }
                
                string errorBody = await response.Content.ReadAsStringAsync();                
                throw new HttpRequestException(
                    $"{(int)response.StatusCode} {response.ReasonPhrase}. Detalle: {errorBody}",
                    null,   // inner exception
                    response.StatusCode);


            }
            catch (Exception ex)
            {
                string tokenSnippet = "N/A";
                if (!string.IsNullOrEmpty(token))
                {
                    if (token.Length >= 5)
                        tokenSnippet = $"{token[..4]}...{token.Substring(token.Length - 1, 1)}";
                    else
                        tokenSnippet = new string('*', token.Length);
                }

                var result = new
                {
                    error = "Falla de conexión a la API de Tempo",
                    mensaje = ex.Message,
                    usuario = accountId,
                    url = apiUrl,
                    apiKey = tokenSnippet
                };

                System.Diagnostics.Debug.WriteLine($"Error en {nameof(TempoTestearConexionAsync)}: {ex.Message}");
                return JsonSerializer.Serialize(result, _jsonOptions);
            }
        }


        #region MetodosPrivadosAuxiliares

        // Bearer token para Tempo (distinto al Basic Auth de Jira)
        private static readonly HttpClient _httpClient = CrearHttpClient();

        private static HttpClient CrearHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GlobalConfig.TempoToken}");
            return client;
        }

        private static async Task<string> ProcesarRespuestaAsync(HttpResponseMessage response, string nombreMetodo)
        {
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync() ?? string.Empty;

            string errorBody = await response.Content.ReadAsStringAsync();
            string mensaje = $"Error en {nombreMetodo}: {(int)response.StatusCode} {response.ReasonPhrase}. Detalle: {errorBody}";

            System.Diagnostics.Debug.WriteLine(mensaje);
            throw new HttpRequestException(mensaje, null, response.StatusCode);
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // Convierte segundos a formato legible "Xh Ym"
        // Tempo siempre trabaja en segundos internamente
        private static string FormatearHoras(int segundos)
        {
            if (segundos == 0) return "0h 0m";

            bool negativo = segundos < 0;
            int abs = Math.Abs(segundos);
            int horas = abs / 3600;
            int minutos = (abs % 3600) / 60;

            string resultado = $"{horas}h {minutos}m";
            return negativo ? $"-{resultado}" : resultado;
        }

        /// <summary>
        /// Genera un resumen de las horas registradas en la semana laboral actual (lunes a viernes).
        /// No requiere parámetros, calcula las fechas automáticamente.
        /// </summary>
        [McpServerTool, Description(
            "Genera un resumen de las horas registradas por el usuario en la semana laboral actual (lunes a viernes). " +
            "No requiere ningún parámetro: calcula automáticamente el lunes y viernes de la semana en curso. " +
            "Retorna: rango de fechas de la semana, total de horas registradas, desglose por día " +
            "y listado de issues trabajados con sus horas. " +
            "Usar cuando el usuario pregunta '¿cuántas horas cargué esta semana?', " +
            "'¿en qué trabajé esta semana?' o cualquier consulta sobre la semana actual sin especificar fechas.")]
        public static async Task<string> TempoObtenerResumenSemanaActualAsync()
        {
            // --- Cálculo automático de lunes y viernes de la semana actual ---
            int diasDesdeElLunes = ((int)DateTime.Today.DayOfWeek + 6) % 7;
            DateTime lunes = DateTime.Today.AddDays(-diasDesdeElLunes);
            DateTime viernes = lunes.AddDays(4);

            return await ProcesarResumenSemanaAsync(lunes, viernes);
        }

        /// <summary>
        /// Genera un resumen de las horas registradas en la semana laboral de una fecha específica.
        /// </summary>
        [McpServerTool, Description(
            "Genera un resumen de las horas registradas por el usuario en una semana específica. " +
            "Requiere un parámetro: fechaInicio (cualquier día de la semana) y calculará automáticamente el lunes y viernes de esa semana. " +
            "Retorna: rango de fechas de la semana, total de horas registradas, desglose por día " +
            "y listado de issues trabajados con sus horas. " +
            "Usar cuando el usuario pregunta '¿cuántas horas cargué la semana que empezó el X?' " +
            "o 'resumen de la semana del Y'.")]
        public static async Task<string> TempoObtenerResumenSemanaAsync(
            [Description("Fecha dentro de la semana a consultar. Formato: YYYY-MM-DD. Ejemplo: 2025-03-01")] string fechaInicio)
        {
            if (!DateTime.TryParseExact(
                    fechaInicio,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fecha))
            {
                return JsonSerializer.Serialize(new { error = $"Formato de fecha inválido '{fechaInicio}'. Debe ser YYYY-MM-DD." }, _jsonOptions);
            }

            int diasDesdeElLunes = ((int)fecha.DayOfWeek + 6) % 7;
            DateTime lunes = fecha.AddDays(-diasDesdeElLunes);
            DateTime viernes = lunes.AddDays(4);

            return await ProcesarResumenSemanaAsync(lunes, viernes);
        }

        private static async Task<string> ProcesarResumenSemanaAsync(DateTime lunes, DateTime viernes)
        {
            string fechaDesde = lunes.ToString(GlobalConfig.FormatoFechaTempo);
            string fechaHasta = viernes.ToString(GlobalConfig.FormatoFechaTempo);

            string accountId = GlobalConfig.TempoAccountId;
            string apiUrl = $"{GlobalConfig.TempoBaseUrl}/worklogs/user/{accountId}" +
                            $"?from={fechaDesde}&to={fechaHasta}&limit=1000";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                string jsonRaw = await ProcesarRespuestaAsync(response, nameof(ProcesarResumenSemanaAsync));

                var paged = JsonSerializer.Deserialize<TempoPagedResponse<TempoWorklog>>(jsonRaw, _jsonOptions);

                if (paged is null || paged.Results.Count == 0)
                    return JsonSerializer.Serialize(new
                    {
                        semana = new { desde = fechaDesde, hasta = fechaHasta },
                        mensaje = "No se registraron horas en la semana seleccionada."
                    }, _jsonOptions);

                // --- Desglose por día ---
                var desglosePorDia = paged.Results
                    .GroupBy(w => w.StartDate ?? "N/A")
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        fecha = g.Key,
                        diaSemana = ObtenerNombreDia(g.Key),
                        horasRegistradas = FormatearHoras(g.Sum(w => w.TimeSpentSeconds)),
                        cantidadWorklogs = g.Count()
                    });

                // --- Desglose por issue ---
                var desglosePorIssue = paged.Results
                    .GroupBy(w => ObtenerIssueLabel(w.Issue))
                    .OrderByDescending(g => g.Sum(w => w.TimeSpentSeconds))
                    .Select(g => new
                    {
                        issue = g.Key,
                        horasRegistradas = FormatearHoras(g.Sum(w => w.TimeSpentSeconds)),
                        descripcionesUnicas = g
                            .Where(w => !string.IsNullOrWhiteSpace(w.Description))
                            .Select(w => w.Description!)
                            .Distinct()
                            .ToList()
                    });

                var resultado = new
                {
                    semana = new { desde = fechaDesde, hasta = fechaHasta },
                    hoy = DateTime.Today.ToString(GlobalConfig.FormatoFechaTempo),
                    totalHoras = FormatearHoras(paged.Results.Sum(w => w.TimeSpentSeconds)),
                    totalHorasFacturables = FormatearHoras(paged.Results.Sum(w => w.BillableSeconds)),
                    totalWorklogs = paged.Results.Count,
                    desglosePorDia,
                    desglosePorIssue
                };

                return JsonSerializer.Serialize(resultado, _jsonOptions);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(ProcesarResumenSemanaAsync)}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(mensajeError);
                return JsonSerializer.Serialize(new { error = mensajeError });
            }
        }

        // La API v4 de Tempo no devuelve el campo "key" en el issue, solo "id" y "self".
        // Este helper muestra la key si existe (ej: PROJ-123), o el ID entre corchetes como fallback.
        private static string ObtenerIssueLabel(TempoIssue? issue)
        {
            if (issue is null) return "Sin issue";
            if (!string.IsNullOrWhiteSpace(issue.Key)) return issue.Key;
            if (issue.Id > 0) return $"[Issue#{issue.Id}]";
            return "Sin issue";
        }

        // Convierte una fecha "yyyy-MM-dd" al nombre del día en español
        private static string ObtenerNombreDia(string fechaStr)
        {
            if (!DateTime.TryParseExact(
                    fechaStr,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fecha))
                return "N/A";

            return fecha.DayOfWeek switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => "N/A"
            };
        }


        // Método privado auxiliar: encapsula la consulta y el procesamiento de un usuario
        internal static async Task<object> ObtenerWorklogsUsuario(
            string accountId,
            string fechaDesde,
            string fechaHasta,
            int hsSemanales = 0)
        {
            string apiUrl = $"{GlobalConfig.TempoBaseUrl}/worklogs/user/{accountId}" +
                            $"?from={fechaDesde}&to={fechaHasta}&limit=1000";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            string jsonRaw = await ProcesarRespuestaAsync(response, nameof(ObtenerWorklogsUsuario));

            var paged = JsonSerializer.Deserialize<TempoPagedResponse<TempoWorklog>>(jsonRaw, _jsonOptions);

            if (paged is null || paged.Results.Count == 0)
            {
                var sinDatos = new Dictionary<string, object?>
                {
                    ["accountId"] = accountId,
                    ["mensaje"]   = "Sin horas registradas en la semana actual."
                };
                if (hsSemanales > 0)
                    sinDatos["totalSegundosSemanales"] = hsSemanales * 3600;

                return sinDatos;
            }

            var desglosePorDia = paged.Results
                .GroupBy(w => w.StartDate ?? "N/A")
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    fecha             = g.Key,
                    diaSemana         = ObtenerNombreDia(g.Key),
                    horasRegistradas  = FormatearHoras(g.Sum(w => w.TimeSpentSeconds))
                });

            var desglosePorIssue = paged.Results
                .GroupBy(w => ObtenerIssueLabel(w.Issue))
                .OrderByDescending(g => g.Sum(w => w.TimeSpentSeconds))
                .Select(g => new
                {
                    issue            = g.Key,
                    horasRegistradas = FormatearHoras(g.Sum(w => w.TimeSpentSeconds))
                });

            var resultado = new Dictionary<string, object?>
            {
                ["accountId"]                      = accountId,
                ["totalSegundos"]                  = paged.Results.Sum(w => w.TimeSpentSeconds),
                ["totalHorasFormateadas"]           = FormatearHoras(paged.Results.Sum(w => w.TimeSpentSeconds)),
                ["totalHorasFacturablesFormateadas"] = FormatearHoras(paged.Results.Sum(w => w.BillableSeconds)),
                ["desglosePorDia"]                 = desglosePorDia,
                ["desglosePorIssue"]               = desglosePorIssue
            };

            if (hsSemanales > 0)
                resultado["totalSegundosSemanales"] = hsSemanales * 3600;

            return resultado;
        }

        #endregion

    }

}
