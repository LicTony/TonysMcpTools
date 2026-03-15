using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TonysMcpTools.Classes
{
    // Respuesta paginada que devuelve Tempo en todos sus endpoints de lista
    public class TempoPagedResponse<T>
    {
        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = [];

        [JsonPropertyName("metadata")]
        public TempoMetadata? Metadata { get; set; }
    }

    public class TempoMetadata
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }
    }

    // Un worklog individual de Tempo
    public class TempoWorklog
    {
        [JsonPropertyName("tempoWorklogId")]
        public long TempoWorklogId { get; set; }

        [JsonPropertyName("issue")]
        public TempoIssue? Issue { get; set; }

        [JsonPropertyName("timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }

        [JsonPropertyName("billableSeconds")]
        public int BillableSeconds { get; set; }

        [JsonPropertyName("startDate")]
        public string? StartDate { get; set; }

        [JsonPropertyName("startTime")]
        public string? StartTime { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("author")]
        public TempoAuthor? Author { get; set; }
    }

    // La API v4 de Tempo devuelve solo "id" y "self" — NO devuelve "key"
    // El campo Key queda para compatibilidad pero siempre llegará null desde v4
    public class TempoIssue
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }
    }

    public class TempoAuthor
    {
        [JsonPropertyName("accountId")]
        public string? AccountId { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }

    // Estado de aprobación del timesheet
    public class TempoTimesheetApproval
    {
        [JsonPropertyName("period")]
        public TempoPeriod? Period { get; set; }

        [JsonPropertyName("requiredSeconds")]
        public int RequiredSeconds { get; set; }

        [JsonPropertyName("timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }

        [JsonPropertyName("status")]
        public TempoApprovalStatus? Status { get; set; }
    }

    public class TempoPeriod
    {
        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("to")]
        public string? To { get; set; }
    }

    public class TempoApprovalStatus
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }
}
