using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        public ConfigurationFactory() { }

        public IConfigurationRoot GetConfiguration(string environment)
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddEnvironmentVariables();

            if (IsConfigServerBound())
            {
                Console.WriteLine("-----> Config server binding found...");

                configBuilder.AddConfigServer(environment,
#pragma warning disable CS0618 // Type or member is obsolete
                    new LoggerFactory(new[] { new ConsoleLoggerProvider((name, level) => { level = LogLevel.Information; return true; }, false) }));
#pragma warning restore CS0618 // Type or member is obsolete
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
