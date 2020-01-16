using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Linq;

namespace Web.Config.Transform.Buildpack
{
    public class ConfigurationTracer : ITracer
    {
        public void FlushEnvironmentVariables()
        {
            var configuration = Program.GetService<IConfiguration>();

            if (Convert.ToBoolean(Environment.GetEnvironmentVariable(Constants.ASPNETCORE_TRACE_ENABLED_NM) ?? "false") 
                && !(Environment.GetEnvironmentVariable(Constants.ASPNETCORE_ENVIRONMENT_NM) ?? "Release").ToLower().Contains("prod"))
            {
                Console.WriteLine($"Flushing out configurations (Non-Prod only)...");
                foreach (var config in configuration.GetChildren().Cast<DictionaryEntry>())
                {
                    Console.WriteLine($"{config.Key}: {config.Value}");
                }
            }
        }
    }
}
