using ModelContextProtocol.Server;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TonysMcpTools.Excepctions;

namespace TonysMcpTools
{

    [McpServerToolType]
    public class NuGetToolsAdvanced:IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public NuGetToolsAdvanced()
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
            try { 
                string url = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLower()}/index.json";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new NuGetPackageException(packageName, $"Paquete '{packageName}' no encontrado");
                }

                string json = await response.Content.ReadAsStringAsync();

                var  nugetVersions = JsonSerializer.Deserialize<NuGetVersions>(json, GetOptions());

                if(nugetVersions == null || nugetVersions.Versions.Length == 0)
                {
                    throw new NuGetPackageException(packageName,$"No se encontraron versiones para el paquete {packageName}.");
                }   

                return nugetVersions;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error al optener la lista de NuGet para {PackageName}", packageName);
                throw new NuGetPackageException(packageName, $"Error al optener la lista de NuGet para {packageName}: {ex.Message}",ex);
            }
        }

        // Obtener información detallada de una versión específica
        [McpServerTool, Description("Obtiene los detalles (contenido del .nuspec) de una versión específica de un paquete NuGet.")]
        public async Task<NuGetPackageDetails> GetPackageDetailsAsync(string packageName, string version)
        {
            try { 
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
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error al obtener los detalle del NuGet para {PackageName} version {Version}", packageName, version);
                throw new NuGetPackageException(packageName, $"Error al obtener los detalle del NuGet para {packageName}: {ex.Message}",ex);
            }
        }

        // Buscar paquetes (usa la API de búsqueda)
        [McpServerTool, Description("Busca paquetes NuGet que coincidan con un término de búsqueda y devuelve los resultados.")]
        public async Task<NuGetSearchResult> SearchPackagesAsync(string searchTerm)
        {
            try { 
            string url = $"https://api-v2v3search-0.nuget.org/query?q={searchTerm}&take=20";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var nuGetSearchResult = JsonSerializer.Deserialize<NuGetSearchResult>(json,  GetOptions());

            return nuGetSearchResult ?? throw new NuGetPackageException("","Error al deserializar la respuesta de búsqueda.");
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error al buscar el NuGet para {SearchTerm}", searchTerm);
                throw new NuGetPackageException("",$"Error al buscar el NuGet para {searchTerm}: {ex.Message}",ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Liberar recursos administrados (managed)
                    _httpClient?.Dispose();
                }

                // Aquí liberarías recursos no administrados (unmanaged) si los tuvieras
                // Por ejemplo: handles nativos, punteros, etc.

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Solo si tenés recursos no administrados
        ~NuGetToolsAdvanced()
        {
            Dispose(false);
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

    public class NuGetVersionInfo
    {
        [JsonPropertyName("@id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        [JsonPropertyName("downloads")]
        public long Downloads { get; set; }
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
        public NuGetVersionInfo[] Versions { get; set; } = [];
        [JsonPropertyName("authors")]
        public string[] Authors { get; set; } = [];
        [JsonPropertyName("totalDownloads")]
        public long TotalDownloads { get; set; }
    }



    
}
