using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Web.Config.Transform.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {
            return GetBuildpackInstance().Run(args);
        }

        public static ServiceProvider RegisterServices()
        {
            // setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IEnvironmentWrapper, EnvironmentWrapper>()
                .AddSingleton<IConfigurationFactory, ConfigurationFactory>()
                .AddSingleton<IFileWrapper, FileWrapper>()
                .AddSingleton<IXmlDocumentWrapper, XmlDocumentWrapper>()
                .AddSingleton<WebConfigTransformBuildpack>()
                .BuildServiceProvider();

            return serviceProvider;
        }

        public static WebConfigTransformBuildpack GetBuildpackInstance()
        {
            var serviceProvider = RegisterServices();

            // get services from DI container
            var buildpack = serviceProvider.GetService<WebConfigTransformBuildpack>();
            Console.WriteLine("Resolved services and starting buildpack");

            return buildpack;
        }
    }
}