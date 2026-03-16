using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonysMcpTools.Utiles
{
    public static class GlobalConfig
    {
        public const string FormatoFechaTempo = "yyyy-MM-dd";

        public static string UsuarioJira { get; set; } = string.Empty;
        public static string TokenDeAcceso { get; set; } = string.Empty;
        public static string JiraBaseUrl { get; set; } = string.Empty;

        public static string TempoToken { get; set; } = string.Empty;
        public static string TempoAccountId { get; set; } = string.Empty;
        public static string TempoBaseUrl { get; set; } = string.Empty;

        public static string EmailBaseUrl { get; set; } = string.Empty;
        public static string EmailApiKey { get; set; } = string.Empty;
        public static string EmailFrom { get; set; } = string.Empty;        

    }
}
