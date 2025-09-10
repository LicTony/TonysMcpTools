using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TonysMcpTools
{

    [McpServerToolType]
    public class NuGetServiceAdvanced
    {
        private readonly HttpClient _httpClient;

        public NuGetServiceAdvanced()
        {
            _httpClient = new HttpClient();
        }

        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // Obtener lista de versiones
        [McpServerTool, Description("Obtiene la lista de todas las versiones disponibles para un paquete NuGet específico.")]
        public async Task<NuGetVersions> GetPackageVersionsAsync(string packageName)
        {
            string url = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLower()}/index.json";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Paquete '{packageName}' no encontrado");
            }

            string json = await response.Content.ReadAsStringAsync();

            var  nugetVersions = JsonSerializer.Deserialize<NuGetVersions>(json, GetOptions());

            if(nugetVersions == null || nugetVersions.Versions.Length == 0)
            {
                throw new Exception($"No se encontraron versiones para el paquete {packageName}.");
            }   

            return nugetVersions;
        }

        // Obtener información detallada de una versión específica
        [McpServerTool, Description("Obtiene los detalles (contenido del .nuspec) de una versión específica de un paquete NuGet.")]
        public async Task<NuGetPackageDetails> GetPackageDetailsAsync(string packageName, string version)
        {
            string url = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLower()}/{version}/{packageName.ToLower()}.nuspec";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string nuspecContent = await response.Content.ReadAsStringAsync();

            // Aquí podrías parsear el XML del .nuspec para obtener más detalles
            return new NuGetPackageDetails
            {
                PackageName = packageName,
                Version = version,
                NuspecContent = nuspecContent
            };
        }

        // Buscar paquetes (usa la API de búsqueda)
        [McpServerTool, Description("Busca paquetes NuGet que coincidan con un término de búsqueda y devuelve los resultados.")]
        public async Task<NuGetSearchResult> SearchPackagesAsync(string searchTerm)
        {
            string url = $"https://api-v2v3search-0.nuget.org/query?q={searchTerm}&take=20";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var nuGetSearchResult = JsonSerializer.Deserialize<NuGetSearchResult>(json,  GetOptions());

            return nuGetSearchResult ?? throw new Exception("Error al deserializar la respuesta de búsqueda.");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Modelos para las respuestas
    public class NuGetVersions
    {
        [JsonPropertyName("versions")]
        public string[] Versions { get; set; } = [];
    }

    public class NuGetPackageDetails
    {
        public string PackageName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string NuspecContent { get; set; } = string.Empty;
    }

    public class NuGetSearchResult
    {
        [JsonPropertyName("totalHits")]
        public int TotalHits { get; set; }
        [JsonPropertyName("data")]
        public NuGetSearchData[] Data { get; set; } = [];
    }

    public class NuGetSearchData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("versions")]
        public string[] Versions { get; set; } = [];
        [JsonPropertyName("authors")]
        public string[] Authors { get; set; } = [];
        [JsonPropertyName("totalDownloads")]
        public long TotalDownloads { get; set; }
    }
}
