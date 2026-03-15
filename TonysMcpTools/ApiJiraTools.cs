using ModelContextProtocol.Server;
using Newtonsoft.Json;
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
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace TonysMcpTools
{
    [McpServerToolType]
    public static class ApiJiraTools
    {
        /// <summary>
        /// Obtiene un resumen ejecutivo de un issue de Jira a partir de su clave.
        /// Solo trae los campos esenciales para no sobrecargar el contexto.
        /// </summary>
        /// <param name="issueKey">La clave del issue (ej: PROJ-123)</param>
        /// <returns>JSON con los campos resumidos del issue</returns>
        [McpServerTool, Description(
            "Obtiene un resumen ejecutivo de un issue de Jira a partir de su clave (ej: PROJ-123). " +
            "Retorna los campos esenciales: clave, resumen, estado, tipo de issue, prioridad, asignado, " +
            "fechas de creación y actualización, descripción y comentarios. " +
            "Usar cuando se necesita una visión general rápida del issue sin sobrecargar el contexto con campos innecesarios. " +
            "Preferir este método sobre ObtenerDetalleIssueAsync cuando no se requieren campos personalizados.")]
        public static async Task<string> JiraObtenerDetalleResumidoIssueAsync(
            [Description("La clave del issue de Jira. Ejemplo: PROJ-123")] string issueKey)
        {
            string fields = "summary,status,issuetype,priority,assignee,created,updated,description,comment,attachment";
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}?fields={fields}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                string jsonRaw = await ProcesarRespuestaAsync(response, nameof(JiraObtenerDetalleResumidoIssueAsync));

                JiraIssueResume? issue = JsonSerializer.Deserialize<JiraIssueResume>(jsonRaw, _jsonOptions);


                if (issue is null)
                    return JsonSerializer.Serialize(new { error = "No se pudo obtener información del issue." });

                // Construimos el objeto que queremos retornar como JSON
                var resultado = new
                {
                    key = issue.Key ?? "N/A",
                    summary = issue.Fields?.Summary ?? "Sin resumen",
                    status = issue.Fields?.Status?.Name ?? "Sin estado",
                    issueType = issue.Fields?.IssueType?.Name ?? "Sin tipo",
                    priority = issue.Fields?.Priority?.Name ?? "Sin prioridad",
                    assignee = issue.Fields?.Assignee?.DisplayName ?? "Sin asignar",
                    created = FormatearFecha(issue.Fields?.Created),
                    updated = FormatearFecha(issue.Fields?.Updated),
                    description = ExtraerTextoADF(issue.Fields?.Description),
                    attachments = (issue.Fields?.Attachment ?? [])
                    .Select(a => new
                    {
                        filename = a.Filename ?? "Sin nombre",
                        mimeType = a.MimeType ?? "N/A",
                        size = FormatearTamanio(a.Size),
                        created = FormatearFecha(a.Created),
                        uploadedBy = a.Author?.DisplayName ?? "Desconocido",
                        url = a.ContentUrl ?? "N/A"
                    }),
                    comments = (issue.Fields?.Comment?.Comments ?? [])
                    .Select(c => new
                    {
                        author = c.Author?.DisplayName ?? "Desconocido",
                        date = FormatearFecha(c.Created),
                        body = ExtraerTextoADF(c.Body),
                        //Adjuntos del comentario
                        attachments = c.Attachment
                                       .Select(a => new
                                       {
                                           filename = a.Filename ?? "Sin nombre",
                                           mimeType = a.MimeType ?? "N/A",
                                           size = FormatearTamanio(a.Size),
                                           created = FormatearFecha(a.Created),
                                           uploadedBy = a.Author?.DisplayName ?? "Desconocido",
                                           url = a.ContentUrl ?? "N/A"
                                       })
                    })
                    };

                return JsonSerializer.Serialize(resultado, _jsonOptions);

            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(JiraObtenerDetalleResumidoIssueAsync)}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(mensajeError);
                return JsonSerializer.Serialize(new { error = mensajeError });
            }
        }


        


        /// <summary>
        /// Obtiene el detalle completo de un issue de Jira, incluyendo campos personalizados.
        /// La respuesta puede ser extensa. Usar solo cuando se necesiten campos personalizados.
        /// </summary>
        /// <param name="issueKey">La clave del issue (ej: PROJ-123)</param>
        /// <returns>JSON con el detalle completo del issue</returns>
        [McpServerTool, Description(
            "Obtiene el detalle completo de un issue de Jira a partir de su clave (ej: PROJ-123). " +
            "Retorna TODOS los campos estándar y personalizados del issue. " +
            "Usar cuando se necesita acceder a campos personalizados (custom fields) o información extendida " +
            "que no está disponible en el resumen. " +
            "Advertencia: la respuesta puede ser muy extensa. Preferir ObtenerDetalleResumidoIssueAsync " +
            "si solo se necesitan los campos básicos.")]
        public static async Task<string> JiraObtenerDetalleIssueAsync(
            [Description("La clave del issue de Jira. Ejemplo: PROJ-123")] string issueKey)
        {
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            return await ProcesarRespuestaAsync(response, nameof(JiraObtenerDetalleIssueAsync));
        }


        /// <summary>
        /// Busca issues en Jira usando una consulta JQL.
        /// Permite filtrar por proyecto, estado, asignado, fechas, etiquetas, etc.
        /// </summary>
        /// <param name="jqlQuery">Consulta JQL (ej: 'project = PROJ AND status = Open')</param>
        /// <param name="maxResults">Cantidad máxima de resultados (por defecto 5000)</param>
        /// <returns>JSON con la lista de issues que coinciden con el filtro</returns>
        [McpServerTool, Description(
            "Busca y retorna issues de Jira usando una consulta JQL (Jira Query Language). " +
            "Parámetros: 'jqlQuery' es la consulta JQL (ej: 'project = PROJ AND status = Open'), " +
            "'maxResults' limita la cantidad de resultados (por defecto 5000). " +
            "Retorna todos los campos de cada issue encontrado. " +
            "Usar cuando se necesita obtener múltiples issues según criterios de filtrado como proyecto, " +
            "estado, asignado, fechas, etiquetas u otros campos. " +
            "Ejemplos de JQL válido: 'assignee = currentUser()', 'created >= -7d AND project = PROJ'.")]
        public static async Task<string> JiraObtenerIssueByJqlAsync(
            [Description("Consulta JQL (ej: 'project = PROJ AND status = Open')")] string jqlQuery,
            [Description("Cantidad máxima de resultados(por defecto 5000)")] int maxResults = 5000)
        {
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/search/jql";

            // Construimos el body con el JQL y los campos requeridos
            var requestBody = new
            {
                jql = jqlQuery,
                fields = new[] { "*all" },
                maxResults
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Este endpoint usa POST en lugar de GET para soportar queries largas
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            return await ProcesarRespuestaAsync(response, nameof(JiraObtenerIssueByJqlAsync));
        }


        /// <summary>
        /// Obtiene los work logs de un issue de Jira.
        /// Útil para analizar horas registradas, autores y fechas de trabajo.
        /// </summary>
        /// <param name="issueKey">La clave del issue (ej: PROJ-123)</param>
        /// <param name="maxResults">Cantidad máxima de work logs a retornar (por defecto 1000)</param>
        /// <returns>JSON con los work logs del issue</returns>
        [McpServerTool, Description(
            "Obtiene el registro de trabajo (work logs) de un issue de Jira a partir de su clave (ej: PROJ-123). " +
            "Retorna todos los work logs con información de autor, tiempo registrado, fecha y comentario. " +
            "Parámetro 'maxResults' limita la cantidad de registros retornados (por defecto 1000). " +
            "Usar cuando se necesita conocer las horas cargadas, quién trabajó en el issue " +
            "o analizar el tiempo invertido en una tarea.")]
        public static async Task<string> JiraObtenerWorkLogsAsync(
            [Description("La clave del issue de Jira. Ejemplo: PROJ-123")] string issueKey,
            [Description("Cantidad máxima de work logs a retornar (por defecto 1000)")] int maxResults = 1000)
        {
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}/worklog?maxResults={maxResults}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            return await ProcesarRespuestaAsync(response, nameof(JiraObtenerWorkLogsAsync));
        }



        /// <summary>
        /// Busca múltiples usuarios en Jira por nombre y retorna el mapeo nombre → accountId.
        /// </summary>
        [McpServerTool, Description(
            "Busca múltiples usuarios en Jira por nombre y retorna un array con el mapeo de cada nombre a su accountId. " +
            "Parámetro: 'nombres' es la lista de nombres parciales o completos a buscar (ej: ['Ascaravilli', 'AShokida']). " +
            "Retorna por cada nombre: el texto buscado, accountId, nombre completo y email del mejor resultado encontrado. " +
            "Usar como paso previo cuando se necesitan los accountIds de varios usuarios para " +
            "consultar sus worklogs o timesheets en Tempo. " +
            "Ejemplo: 'buscar usuarios Ascaravilli y AShokida'.")]
        public static async Task<string> JiraBuscarUsuariosJiraAsync(
            [Description("Lista de nombres de usuario a buscar. Ejemplo: [\"Ascaravilli\", \"AShokida\"]")]
    List<string> nombres)
        {
            try
            {
                // Buscamos todos en paralelo, igual que ObtenerResumenSemanaUsuariosAsync
                var tareas = nombres.Select(nombre => BuscarUnUsuario(nombre));
                var resultados = await Task.WhenAll(tareas);

                return JsonSerializer.Serialize(resultados, _jsonOptions);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error en {nameof(JiraBuscarUsuariosJiraAsync)}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(mensajeError);
                return JsonSerializer.Serialize(new { error = mensajeError });
            }
        }

    //    [McpServerTool, Description(
    //"Obtiene el resumen de horas registradas en la semana laboral actual (lunes a viernes) " +
    //"para uno o varios usuarios de Tempo. " +
    //"Parámetro: 'accountIds' es la lista de accountIds obtenida previamente con BuscarUsuariosJiraAsync. " +
    //"Retorna por cada usuario: total de horas registradas, desglose por día e issues trabajados. " +
    //"Usar siempre después de BuscarUsuariosJiraAsync cuando se necesitan las horas de varios usuarios. " +
    //"Ejemplo: 'horas de Ascaravilli y AShokida esta semana'.")]
    //    public static async Task<string> JiraObtenerResumenSemanaUsuariosAsync(
    //[Description("Lista de accountIds de Jira obtenidos con BuscarUsuariosJiraAsync.")]
    //List<string> accountIds)
    //    {
    //        // Mismo cálculo de fechas que en ObtenerResumenSemanaActualAsync
    //        int diasDesdeElLunes = ((int)DateTime.Today.DayOfWeek + 6) % 7;
    //        DateTime lunes = DateTime.Today.AddDays(-diasDesdeElLunes);
    //        DateTime viernes = lunes.AddDays(4);

    //        string fechaDesde = lunes.ToString(GlobalConfig.FormatoFechaTempo);
    //        string fechaHasta = viernes.ToString(GlobalConfig.FormatoFechaTempo);

    //        try
    //        {
    //            // Lanzamos todas las consultas en paralelo para no esperar una por una
    //            var tareas = accountIds.Select(accountId => TempoObtenerWorklogsUsuario(accountId, fechaDesde, fechaHasta));
    //            var resultadosPorUsuario = await Task.WhenAll(tareas);

    //            var resultado = new
    //            {
    //                semana = new { desde = fechaDesde, hasta = fechaHasta },
    //                usuarios = resultadosPorUsuario
    //            };

    //            return JsonSerializer.Serialize(resultado, _jsonOptions);
    //        }
    //        catch (Exception ex)
    //        {
    //            string mensajeError = $"Error en {nameof(JiraObtenerResumenSemanaUsuariosAsync)}: {ex.Message}";
    //            System.Diagnostics.Debug.WriteLine(mensajeError);
    //            return JsonSerializer.Serialize(new { error = mensajeError });
    //        }
    //    }


        #region MetodosPrivadosAuxiliares

        // -------------------------------------------------------------------------
        // HttpClient estático y compartido: evita el agotamiento de sockets (socket
        // exhaustion) que ocurre cuando se instancia HttpClient dentro de cada método.
        // La autenticación se configura una sola vez acá.
        // -------------------------------------------------------------------------
        private static readonly HttpClient _httpClient = CrearHttpClient();

        private static HttpClient CrearHttpClient()
        {
            var client = new HttpClient();

            // Construimos el header de autenticación Basic una sola vez
            string usuarioMasToken = $"{GlobalConfig.UsuarioJira}:{GlobalConfig.TokenDeAcceso}";
            string base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(usuarioMasToken));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64}");

            return client;
        }

        // -------------------------------------------------------------------------
        // MÉTODO PRIVADO AUXILIAR: centraliza el manejo del response HTTP.
        // Si la respuesta es exitosa, retorna el body como string.
        // Si falla, loguea el error y lanza una HttpRequestException con detalle.
        // De esta forma, los métodos públicos quedan limpios y sin lógica repetida.
        // -------------------------------------------------------------------------
        private static async Task<string> ProcesarRespuestaAsync(HttpResponseMessage response, string nombreMetodo)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync() ?? string.Empty;
            }

            string errorBody = await response.Content.ReadAsStringAsync();
            string mensaje = $"Error en {nombreMetodo}: {(int)response.StatusCode} {response.ReasonPhrase}. Detalle: {errorBody}";

            System.Diagnostics.Debug.WriteLine(mensaje);

            throw new HttpRequestException(mensaje, null, response.StatusCode);
        }



        // Se declara una sola vez a nivel de clase, se reutiliza siempre
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };


        /// <summary>
        /// Extrae el texto plano de un objeto en formato ADF (Atlassian Document Format)
        /// </summary>
        private static string ExtraerTextoADF(JiraDescriptionResume? descripcion)
        {
            if (descripcion is null) return "Sin contenido";

            // Recorremos el árbol de contenido y extraemos solo los textos
            var textos = descripcion.Content
                .SelectMany(bloque => bloque.Content)
                .Where(inner => inner.Type == "text" && !string.IsNullOrWhiteSpace(inner.Text))
                .Select(inner => inner.Text!);

            string resultado = string.Join(" ", textos).Trim();
            return string.IsNullOrEmpty(resultado) ? "Sin contenido" : resultado;
        }

        /// <summary>
        /// Convierte una fecha ISO 8601 de Jira a formato legible dd/MM/yyyy HH:mm
        /// </summary>
        private static string FormatearFecha(string? fechaIso)
        {
            if (string.IsNullOrWhiteSpace(fechaIso))
                return "N/A";

            return DateTimeOffset.TryParse(
                        fechaIso,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTimeOffset fecha)
                ? fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)  // ✅ format provider al formatear
                : "N/A";
        }


        /// <summary>
        /// Helper formatear tamaño de archivos adjuntos en formato legible (B, KB, MB)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string FormatearTamanio(long? bytes)
        {
            if (bytes is null) return "N/A";
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024):F1} MB";
        }

        // Método privado auxiliar: busca un usuario y retorna el mejor resultado
        private static async Task<object> BuscarUnUsuario(string nombre)
        {
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/user/search" +
                            $"?query={Uri.EscapeDataString(nombre)}&maxResults=1";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            string jsonRaw = await ProcesarRespuestaAsync(response, nameof(BuscarUnUsuario));

            var usuarios = JsonSerializer.Deserialize<List<JiraUsuario>>(jsonRaw, _jsonOptions);
            var usuario = usuarios?.FirstOrDefault();

            if (usuario is null)
                return new
                {
                    nombreBuscado = nombre,
                    encontrado = false,
                    accountId = (string?)null,
                    displayName = (string?)null,
                    email = (string?)null
                };

            return new
            {
                nombreBuscado = nombre,
                encontrado = true,
                accountId = usuario.AccountId ?? "N/A",
                displayName = usuario.DisplayName ?? "N/A",
                email = usuario.EmailAddress ?? "N/A"
            };
        }

        #endregion

    }
}
