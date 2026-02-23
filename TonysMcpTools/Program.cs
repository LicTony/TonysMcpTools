using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using TonysMcpTools.Utiles; // Para Assembly

namespace TonysMcpTools
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure Serilog to write logs in a 'logs' subdirectory of the executable's directory
            var executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var logDirectory = Path.Combine(executableDirectory, "logs");
            // Ensure the 'logs' directory exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            var logFilePath = Path.Combine(logDirectory, "TonysMcpTools-.log");
            //retainedFileCountLimit: 30 acumulo solo los ultimos 30 dias de logs
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(logFilePath
                    ,rollingInterval: RollingInterval.Day
                    ,retainedFileCountLimit: 30 )
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);


            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            string MCP_Entropia = Util.GetStringNotNull("MCP_Entropia", "");
            byte[] MCP_EntropiaByte = Encoding.UTF8.GetBytes(MCP_Entropia);
            string ENCRYPT_MCP_UsuarioJira = Util.GetStringNotNull("MCP_UsuarioJira", "");
            string ENCRYPT_MCP_TokenJira = Util.GetStringNotNull("MCP_TokenJira", "");
            string ENCRYPT_MCP_JiraBaseUrl = Util.GetStringNotNull("MCP_JiraBaseUrl", "");

            GlobalConfig.UsuarioJira = Utiles.DpapiHelper.DescifrarSeguro(ENCRYPT_MCP_UsuarioJira, MCP_EntropiaByte);
            GlobalConfig.TokenDeAcceso = Utiles.DpapiHelper.DescifrarSeguro(ENCRYPT_MCP_TokenJira, MCP_EntropiaByte);
            GlobalConfig.JiraBaseUrl = Utiles.DpapiHelper.DescifrarSeguro(ENCRYPT_MCP_JiraBaseUrl, MCP_EntropiaByte);

            Log.Information("MCP Server is starting...");
            Log.Information("Jira Usuario: {UsuarioJira} URL: {JiraBaseUrl}", GlobalConfig.UsuarioJira, GlobalConfig.JiraBaseUrl);            
            
            try
            {
                Log.Information("Starting host...");
                await builder.Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
