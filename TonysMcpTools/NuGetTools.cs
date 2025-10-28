using ModelContextProtocol.Server;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TonysMcpTools.Excepctions;

namespace TonysMcpTools
{

    [McpServerToolType]
    public class NuGetService(HttpClient httpClient)
    {

        private readonly HttpClient _httpClient = httpClient;

        public NuGetService() : this(new HttpClient())
        {
        }

        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }


        [McpServerTool, Description("Retorna informacion sobre un paquete Nuget")]
        public async Task<NuGetPackageInfo> GetNugetInfoAsync(string packageName)
        {
            try
            {
                // API de NuGet para obtener todas las versiones
                string url = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLower()}/index.json";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonContent = await response.Content.ReadAsStringAsync();
                var packageInfo = JsonSerializer.Deserialize<NuGetPackageInfo>(jsonContent, GetOptions());

                if (packageInfo == null || packageInfo.Versions.Length == 0)
                {
                    throw new InvalidOperationException($"No se encontraron versiones para el paquete {packageName}.");
                }


                return packageInfo;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error al consultar NuGet para {PackageName}", packageName);
                throw new NuGetPackageException(packageName, $"Error al consultar NuGet para {packageName}: {ex.Message}");
            }
        }

    }

    // Modelo para deserializar la respuesta
    public class NuGetPackageInfo
    {
        [JsonPropertyName("versions")]
        public string[] Versions { get; set; } = [];
    }


}
