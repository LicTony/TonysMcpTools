using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonysMcpTools.Utiles;

namespace TonysMcpTools
{
    [McpServerToolType]
    public static class ApiJiraTools
    {
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
        public static async Task<string> ObtenerDetalleResumidoIssueAsync(string issueKey)
        {
            // Solo pedimos los campos necesarios para mantener la respuesta liviana
            string fields = "summary,status,issuetype,priority,assignee,created,updated,description,comment";
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}?fields={fields}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            return await ProcesarRespuestaAsync(response, nameof(ObtenerDetalleResumidoIssueAsync));
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
        public static async Task<string> ObtenerDetalleIssueAsync(string issueKey)
        {
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            return await ProcesarRespuestaAsync(response, nameof(ObtenerDetalleIssueAsync));
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
        public static async Task<string> ObtenerIssueByJqlAsync(string jqlQuery, int maxResults = 5000)
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

            return await ProcesarRespuestaAsync(response, nameof(ObtenerIssueByJqlAsync));
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
        public static async Task<string> ObtenerWorkLogsAsync(string issueKey, int maxResults = 1000)
        {
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}/worklog?maxResults={maxResults}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            return await ProcesarRespuestaAsync(response, nameof(ObtenerWorkLogsAsync));
        }
    }
}
