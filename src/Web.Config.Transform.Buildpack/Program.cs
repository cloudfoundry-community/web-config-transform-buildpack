using Microsoft.Extensions.DependencyInjection;
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
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<IEnvironmentWrapper, EnvironmentWrapper>()
                .AddSingleton<IConfigurationFactory, ConfigurationFactory>()
                .AddSingleton<ITracer, ConfigurationTracer>()
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