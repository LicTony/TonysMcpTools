using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace TonysMcpTools
{
    //[McpServerToolType]
    //public class AudioMessageTools
    //{

    //    [McpServerTool, Description("Retorna ultimo mensaje de audio")]
    //    public static async Task<string> GetLastAudioMessage()
    //    {
    //        string url = $"https://firestore.googleapis.com/v1/projects/notalk/databases/notalk/documents/transcriptions/latest?key=AIzaSyDIoqxnp_WLvMFoJ1ILIh5Nhbz5aR-e2pk";

    //        try
    //        {
    //            // 1. Cargar credenciales usando CredentialFactory (método nuevo y seguro)
    //            GoogleCredential credential;
    //            string credPath = $"C:\\_credenciales\\notalk\\notalk-firebase-adminsdk-fbsvc-96f70fc401.json";

    //            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
    //            {
    //                // Usamos ServiceAccountCredential a través de CredentialFactory
    //                var serviceAccountCredential = ServiceAccountCredential
    //                    .FromServiceAccountData(stream);

    //                // Convertimos a GoogleCredential y agregamos los scopes
    //                credential = serviceAccountCredential
    //                    .ToGoogleCredential()
    //                    .CreateScoped(
    //                        "https://www.googleapis.com/auth/datastore",
    //                        "https://www.googleapis.com/auth/cloud-platform"
    //                    );
    //            }

    //            // 2. Obtener el token
    //            var token = await credential.UnderlyingCredential
    //                .GetAccessTokenForRequestAsync(
    //                    cancellationToken: System.Threading.CancellationToken.None);

    //            // 3. Hacer la petición
    //            using HttpClient client = new();
    //            client.DefaultRequestHeaders.Authorization =
    //                new AuthenticationHeaderValue("Bearer", token);

    //            HttpResponseMessage response = await client.GetAsync(url);
    //            response.EnsureSuccessStatusCode();

    //            string responseBody = await response.Content.ReadAsStringAsync();
    //            var firestoreDocument = JsonSerializer.Deserialize<FirestoreDocument>(responseBody);

    //            return firestoreDocument?.Fields?.Text?.Value ?? "No message found.";
    //        }
    //        catch (FileNotFoundException)
    //        {
    //            return "Error: No se encontró el archivo de credenciales.";
    //        }
    //        catch (HttpRequestException e)
    //        {
    //            return $"Error en petición HTTP: {e.Message}";
    //        }
    //        catch (Exception e)
    //        {
    //            return $"Error: {e.Message}";
    //        }
    //    }
      

    //    // Helper classes for JSON deserialization
    //    private sealed class FirestoreDocument
    //    {
    //        [JsonPropertyName("fields")]
    //        public Fields? Fields { get; set; }
    //    }

    //    private sealed class Fields
    //    {
    //        [JsonPropertyName("text")]
    //        public StringValue? Text { get; set; } 
    //    }

    //    private sealed class StringValue
    //    {
    //        [JsonPropertyName("stringValue")]
    //        public string Value { get; set; } = string.Empty;
    //    }
    //}
}
