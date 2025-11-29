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
