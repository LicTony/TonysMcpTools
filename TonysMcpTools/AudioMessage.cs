using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TonysMcpTools
{
    [McpServerToolType]
    public class AudioMessage
    {
        [McpServerTool, Description("Retorna ultimo mensaje de audio")]
        public static async Task<string> GetLastAudioMessage()
        {
            string url = $"https://firestore.googleapis.com/v1/projects/notalk/databases/notalk/documents/transcriptions/latest?key=AIzaSyDIoqxnp_WLvMFoJ1ILIh5Nhbz5aR-e2pk";
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var firestoreDocument = JsonSerializer.Deserialize<FirestoreDocument>(responseBody);

                return firestoreDocument?.Fields?.Text?.Value ?? "No message found.";
            }
            catch (HttpRequestException e)
            {
                return $"Error fetching data: {e.Message}";
            }
        }

        // Helper classes for JSON deserialization
        private sealed class FirestoreDocument
        {
            [JsonPropertyName("fields")]
            public Fields? Fields { get; set; }
        }

        private sealed class Fields
        {
            [JsonPropertyName("text")]
            public StringValue? Text { get; set; } 
        }

        private sealed class StringValue
        {
            [JsonPropertyName("stringValue")]
            public string Value { get; set; } = string.Empty;
        }
    }
}
