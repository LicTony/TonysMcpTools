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
    public static class DateTimeTool_yyyymmdd_hhmmss
    {
        [McpServerTool, Description("Retorna la fecha y hora actual en formato yyyyMMdd_HHmmss.")]
        public static string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }
    }

    [McpServerToolType]
    public static class DateTimeTool_yyyyMMdd
    {
        [McpServerTool, Description("Retorna la fecha actual en formato yyyyMMdd.")]
        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }
    }
}
