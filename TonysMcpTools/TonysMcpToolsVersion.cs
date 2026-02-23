using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonysMcpTools
{
    [McpServerToolType]
    public static class TonysMcpToolsVersion
    {

        [McpServerTool, Description("Retorna la version actual de TonysMcpTools.")]
        public static string GetCurrentTonysMcpToolsVersion()
        {
            return $"1.0.1 2025-02-23 18:25";
        }
    }
}
