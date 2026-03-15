using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TonysMcpTools.Classes
{
    public class JiraIssueResume
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("fields")]
        public JiraFieldsResume? Fields { get; set; }
    }

    public class JiraFieldsResume
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("status")]
        public JiraStatusResume? Status { get; set; }

        [JsonPropertyName("issuetype")]
        public JiraIssueTypeResume? IssueType { get; set; }

        [JsonPropertyName("priority")]
        public JiraPriorityResume? Priority { get; set; }

        [JsonPropertyName("assignee")]
        public JiraUserResume? Assignee { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("updated")]
        public string? Updated { get; set; }

        [JsonPropertyName("description")]
        public JiraDescriptionResume? Description { get; set; }

        [JsonPropertyName("comment")]
        public JiraCommentContainerResume? Comment { get; set; }


        [JsonPropertyName("attachment")]
        public List<JiraAttachmentResume> Attachment { get; set; } = new();
    }

    public class JiraStatusResume
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class JiraIssueTypeResume
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class JiraPriorityResume
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class JiraUserResume
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }
    }

    public class JiraDescriptionResume
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("content")]
        public List<JiraDescriptionContentResume> Content { get; set; } = [];
    }

    public class JiraDescriptionContentResume
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("content")]
        public List<JiraDescriptionInnerResume> Content { get; set; } = [];
    }

    public class JiraDescriptionInnerResume
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class JiraCommentContainerResume
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }  // int no necesita ser nullable, por defecto es 0

        [JsonPropertyName("comments")]
        public List<JiraCommentResume> Comments { get; set; } = [];
    }

    public class JiraCommentResume
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("author")]
        public JiraUserResume? Author { get; set; }

        [JsonPropertyName("body")]
        public JiraDescriptionResume? Body { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("updated")]
        public string? Updated { get; set; }

        //Adjuntos dentro del comentario
        [JsonPropertyName("attachment")]
        public List<JiraAttachmentResume> Attachment { get; set; } = new();
    }


    public class JiraAttachmentResume
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("content")]
        public string? ContentUrl { get; set; }

        [JsonPropertyName("author")]
        public JiraUserResume? Author { get; set; }
    }


    public class JiraUsuario
    {
        [JsonPropertyName("accountId")]
        public string? AccountId { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }

}
