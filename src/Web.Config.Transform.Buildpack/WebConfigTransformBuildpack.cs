using System;
using System.IO;

namespace Web.Config.Transform.Buildpack
{
    public class WebConfigTransformBuildpack : SupplyBuildpack
    {
        IEnvironmentWrapper _environmentWrapper;
        ITracer _tracer;
        IConfigurationFactory _configurationFactory;
        IFileWrapper _fileWrapper;
        IXmlDocumentWrapper _xmlDocumentWrapper;
        ILogger _logger;

        public WebConfigTransformBuildpack(
            IEnvironmentWrapper environmentWrapper,
            ITracer tracer,
            IConfigurationFactory configurationFactory,
            IFileWrapper fileWrapper,
            IXmlDocumentWrapper xmlDocumentWrapper,
            ILogger logger)
        {
            _environmentWrapper = environmentWrapper;
            _tracer = tracer;
            _configurationFactory = configurationFactory;
            _fileWrapper = fileWrapper;
            _xmlDocumentWrapper = xmlDocumentWrapper;
            _logger = logger;
        }

        protected override bool Detect(string buildPath)
        {
            return false;
        }

        private void LogHeader(string message)
        {
            _logger.WriteLog("".PadRight(message.Length + 10, '='));
            _logger.WriteLog("==== " + (message.Trim() + " ").PadRight(message.Length - 4, '='));
            _logger.WriteLog("".PadRight(message.Length + 10, '='));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            LogHeader(".NET Config Transform Buildpack execution started");

            using (var configManager =
                new ConfigReaderWriterBuilder(options =>
                {
                    options.BuildPath = buildPath;
                    options.ConfigurationFactory = _configurationFactory;
                    options.EnvironmentWrapper = _environmentWrapper;
                    options.FileWrapper = _fileWrapper;
                    options.Logger = _logger;
                    options.XmlDocumentWrapper = _xmlDocumentWrapper;
                })
                .Build())
            {
                _tracer.FlushEnvironmentVariables();

                var transformer =
                    new ConfigTransformHandler(
                        configManager.ConfigSettings.SourceConfigRoot, configManager as IConfigReader, configManager as IConfigWriter);
                transformer.ApplyXmlTransformation(configManager.ConfigSettings.TransformConfigPath);
                transformer.CopyExternalAppSettings();
                transformer.CopyExternalConnectionStrings();
                transformer.CopyExternalTokens();
            }

            LogHeader(".NET Config Transform Buildpack execution completed");
        }
    }
}
