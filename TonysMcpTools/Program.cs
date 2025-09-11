using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Serilog;
using System.IO;
using System.Reflection; // Para Assembly

namespace TonysMcpTools
{
    class Program
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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);


            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

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
                Log.CloseAndFlush();
            }
        }
    }
}
