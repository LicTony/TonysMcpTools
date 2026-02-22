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
    public  static class ApiJiraTools
    {

        /// <summary>
        /// Obtiene un resumen del issue de Jira con solo los campos clave:
        /// clave, resumen, estado, tipo de issue, prioridad, asignado, fecha de creación, fecha de actualización, descripcion y comentarios.
        /// </summary>
        /// <param name="issueKey">La clave del issue (ej: PROJ-123)</param>
        /// <returns>JSON con los campos resumidos del issue</returns>
        /// <exception cref="HttpRequestException"></exception>
        [McpServerTool, Description("Obtiene un resumen del issue de Jira con los campos clave: clave, resumen, estado, tipo, prioridad, asignado, fecha de creación, actualización, descripcion y comentarios.")]
        public static async Task<string?> ObtenerDetalleResumidoIssueAsync(string issueKey)
        {
            // Campos solicitados: summary, status, issuetype, priority, assignee, created, updated
            string fields = "summary,status,issuetype,priority,assignee,created,updated,description,comment";
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}?fields={fields}";

            using HttpClient client = new();

            // Autenticación
            string usuarioMasToken = $"{GlobalConfig.UsuarioJira}:{GlobalConfig.TokenDeAcceso}";
            client.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(usuarioMasToken)));

            // Envía la solicitud GET y obtén la respuesta
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            // Procesa la respuesta
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync() ?? "";
                return responseBody;
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error Body: {errorBody}");
                throw new HttpRequestException(
                        $"Error ObtenerDetalleResumidoIssueAsync: {response.StatusCode} - {response.ReasonPhrase}. Detalle: {errorBody}",
                        null,
                        response.StatusCode
                );
            }
        }


        /// <summary>
        /// Obtiene el detalle completo de un issue de Jira, incluyendo campos personalizados.
        /// </summary>
        /// <param name="issueKey">La clave del issue (ej: PROJ-123)</param>
        /// <returns>JSON con el detalle completo del issue</returns>
        /// <exception cref="HttpRequestException"></exception>
        [McpServerTool, Description("Obtiene el detalle completo de un issue de Jira, incluyendo todos los campos y campos personalizados.")]
        public static async Task<string?> ObtenerDetalleIssueAsync(string issueKey)
        {
            // Endpoint para obtener el detalle de un issue
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{issueKey}";

            using HttpClient client = new();

            // Autenticación
            string usuarioMasToken = $"{GlobalConfig.UsuarioJira}:{GlobalConfig.TokenDeAcceso}";
            client.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(usuarioMasToken)));

            // Envía la solicitud GET y obtén la respuesta
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            // Procesa la respuesta
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync() ?? "";
                return responseBody;
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error Body: {errorBody}");
                throw new HttpRequestException(
                        $"Error ObtenerDetalleIssueAsync: {response.StatusCode} - {response.ReasonPhrase}. Detalle: {errorBody}",
                        null,
                        response.StatusCode
                );
            }
        }


        /// <summary>
        /// Obtiene todos los issues del jqlQuery especificado.
        /// </summary>
        /// <param name="jqlQuery"></param>        
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [McpServerTool, Description("Obtiene todos los issues del jqlQuery especificado.")]
        public static async Task<string?>  ObtenerIssueByJqlAsync(string jqlQuery, int maxResults = 5000)
        {
            // Nuevo endpoint
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/search/jql";

            // Body de la petición
            var requestBody = new
            {
                jql = jqlQuery,
                fields = new[] { "*all" },
                maxResults
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);

            using HttpClient client = new();

            // Autenticación
            string usuarioMasToken = $"{GlobalConfig.UsuarioJira}:{GlobalConfig.TokenDeAcceso}";
            client.DefaultRequestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(usuarioMasToken)));

            // Crear contenido JSON
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Usar POST en lugar de GET
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            // Procesa la respuesta
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync() ?? "";

                return responseBody;
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error Body: {errorBody}");
                throw new HttpRequestException(
                        $"Error ObtenerIssueKeysFechaExactaAsync: {response.StatusCode} - {response.ReasonPhrase}. Detalle: {errorBody}",
                        null,
                        response.StatusCode
                );
            }
        }



        [McpServerTool, Description("Obtiene todos los work log del IssueKey especificado.")]
        public static async Task<string?> ObtenerWorkLogsAsync(string IssueKey, int maxResults = 1000)
        {
            // Configura la URL de la API de Jira con tu instancia y consulta JQL                
            string apiUrl = $"{GlobalConfig.JiraBaseUrl}/rest/api/3/issue/{IssueKey}/worklog?maxResults={maxResults}";

            // Configura la solicitud HTTP
            using HttpClient client = new();
            // Puedes configurar la autenticación si es necesaria
            string usuarioMasToken = $"{GlobalConfig.UsuarioJira}:{GlobalConfig.TokenDeAcceso}";
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(usuarioMasToken)));

            // Envía la solicitud GET y obtén la respuesta
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            // Procesa la respuesta
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync() ?? "";


                return responseBody;
            }
            else
            {
                throw new HttpRequestException($"Error ObtenerWorkLogsAsync: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }


    }
}
