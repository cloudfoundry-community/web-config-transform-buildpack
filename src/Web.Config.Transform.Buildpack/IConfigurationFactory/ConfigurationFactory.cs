using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System;
using System.Linq;

namespace Web.Config.Transform.Buildpack
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        ILogger _logger;
        public ConfigurationFactory(ILogger logger)
        {
            if (logger == null)
            {
                logger = new ConsoleLogger();
            }
            _logger = logger;
        }

        public IConfigurationRoot GetConfiguration(string environment)
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddEnvironmentVariables();

            if (IsConfigServerBound())
            {
                _logger.WriteLog("-----> Config server binding found...");

                var logFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole(options => options.IncludeScopes = false);
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                configBuilder.AddConfigServer(environment, logFactory);
            }
            else
                configBuilder.AddCloudFoundry();

            return configBuilder.Build();
        }

        private static bool IsConfigServerBound()
        {
            var configuration = new ConfigurationBuilder()
                                        .AddCloudFoundry()
                                        .Build();

            var cfServicesOptions = new CloudFoundryServicesOptions(configuration);

            foreach (var service in cfServicesOptions.ServicesList)
            {
                if (service.Label == "p-config-server"
                    || service.Label == "p.config-server"
                    || (service.Tags != null && (service.Tags.Contains("spring-cloud") && service.Tags.Contains("configuration"))))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
