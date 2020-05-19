using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Web.Config.Transform.Buildpack
{
    public struct ConfigManagerSettings
    {
        public ConfigManagerSettings(
            string buildPath,
            string transformConfigPath,
            string sourceConfigPath,
            IConfigurationRoot sourceConfigRoot,
            IFileWrapper fileWrapper,
            IXmlDocumentWrapper xmlDocumentWrapper,
            ILogger logger)
        {
            BuildPath = buildPath;
            TransformConfigPath = transformConfigPath;
            SourceConfigPath = sourceConfigPath;
            SourceConfigRoot = sourceConfigRoot;
            FileWrapper = fileWrapper;
            XmlDocumentWrapper = xmlDocumentWrapper;
            Logger = logger;
        }

        public string BuildPath { get; }
        public string TransformConfigPath { get; }
        public string SourceConfigPath { get; }
        public IConfigurationRoot SourceConfigRoot { get; }
        public IFileWrapper FileWrapper { get; }
        public IXmlDocumentWrapper XmlDocumentWrapper { get; }
        public ILogger Logger { get; }
    }

    public class ConfigManagerSettingsBuilder : IConfigManagerSettingsBuilder
    {
        public IFileWrapper FileWrapper { get; set; }
        public IXmlDocumentWrapper XmlDocumentWrapper { get; set; }
        public IEnvironmentWrapper EnvironmentWrapper { get; set; }
        public IConfigurationFactory ConfigurationFactory { get; set; }
        public ILogger Logger { get; set; }
        public string BuildPath { get; set; }

        /// <summary>
        /// Get application config file and transformation file paths during runtime.
        /// </summary>
        /// <returns></returns>
        public virtual (string transformationConfigPath, string sourceConfigPath) GetConfigPaths()
        {
            // default to web.config
            var transformationKey = EnvironmentWrapper.GetEnvironmentVariable(Constants.XML_TRANSFORM_KEY_NM) ?? "Release";
            var transformConfigPath = FileWrapper?.Combine(BuildPath, $"web.{transformationKey}.config");
            string webConfigPath = FileWrapper?.Combine(BuildPath, "web.config");
            var webConfigExists = (FileWrapper?.Exists(webConfigPath)).GetValueOrDefault();
            var sourceConfigPath = webConfigPath;

            string appConfigPath = FileWrapper?.GetFiles(BuildPath, "*.exe.config")?.FirstOrDefault();
            var appConfigExists = (FileWrapper?.Exists(appConfigPath) as bool?).GetValueOrDefault();

            // Fail if no config was found
            if (!webConfigExists && !appConfigExists)
            {
                throw new ApplicationException("[Web|App] config file does not exist. Exiting the program.");
            }

            // Replace based on app.config existance
            if (appConfigExists)
            {
                transformConfigPath = Path.Combine(BuildPath, $"app.{transformationKey}.config");
                sourceConfigPath = appConfigPath;
            }

            return (transformConfigPath, sourceConfigPath);
        }

        public ConfigManagerSettings Build()
        {
            if (EnvironmentWrapper is null)
                throw new ArgumentNullException(nameof(EnvironmentWrapper), "Environment wrapper is required");
            if (FileWrapper is null)
                throw new ArgumentNullException(nameof(FileWrapper), "File wrapper is required");
            if (XmlDocumentWrapper is null)
                throw new ArgumentNullException(nameof(XmlDocumentWrapper), "Xml document wrapper is required");
            if (ConfigurationFactory is null)
                throw new ArgumentNullException(nameof(ConfigurationFactory), "Configuration factory wrapper is required");

            var environment = EnvironmentWrapper.GetEnvironmentVariable(Constants.ASPNETCORE_ENVIRONMENT_NM) ?? "Release";
            Logger.WriteLog($"-----> Using Environment: {environment}");

            var sourceConfigRoot = ConfigurationFactory.GetConfiguration(environment);
            if (sourceConfigRoot is null)
            {
                throw new ArgumentNullException(nameof(sourceConfigRoot), "Application configuration is required");
            }

            var (transformConfigPath, sourceConfigPath) = GetConfigPaths();

            return new ConfigManagerSettings(
                BuildPath,
                transformConfigPath,
                sourceConfigPath,
                sourceConfigRoot,
                FileWrapper,
                XmlDocumentWrapper,
                Logger
                );
        }
    }
}