using Microsoft.Extensions.DependencyInjection;
using System;

namespace Web.Config.Transform.Buildpack
{
    public class Program
    {
        static ServiceProvider container;

        static int Main(string[] args)
        {
            return GetBuildpackInstance().Run(args);
        }

        public static T GetService<T>()
        {
            if (container == null)
                container = RegisterServices();

            return container.GetService<T>();
        }

        public static ServiceProvider RegisterServices()
        {
            // setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<IEnvironmentWrapper, EnvironmentWrapper>()
                .AddSingleton<ITracer, ConfigurationTracer>()
                .AddSingleton<IConfigurationFactory, ConfigurationFactory>()
                .AddSingleton<IFileWrapper, FileWrapper>()
                .AddSingleton<IXmlDocumentWrapper, XmlDocumentWrapper>()
                .AddSingleton<WebConfigTransformBuildpack>()
                .BuildServiceProvider();
            return container = serviceProvider;
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