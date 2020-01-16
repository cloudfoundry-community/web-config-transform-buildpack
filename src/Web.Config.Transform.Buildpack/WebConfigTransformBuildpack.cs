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

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            _logger.WriteLog("================================================================================");
            _logger.WriteLog("=============== WebConfig Transform Buildpack execution started ================");
            _logger.WriteLog("================================================================================");

            ApplyTransformations(Path.Combine(buildPath, "web.config"));

            _logger.WriteLog("================================================================================");
            _logger.WriteLog("============== WebConfig Transform Buildpack execution completed ===============");
            _logger.WriteLog("================================================================================");
        }

        private void ApplyTransformations(string webConfig)
        {
            using (var webConfigManager = new WebConfigManager(_fileWrapper, _xmlDocumentWrapper, _logger, webConfig))
            {
                var environment = _environmentWrapper.GetEnvironmentVariable(Constants.ASPNETCORE_ENVIRONMENT_NM) ?? "Release";
                _logger.WriteLog($"-----> Using Environment: {environment}");
                var config = _configurationFactory.GetConfiguration(environment);

                _tracer.FlushEnvironmentVariables();

                var transform = new WebConfigTransformHandler(config, webConfigManager);

                transform.CopyExternalAppSettings(webConfigManager);
                transform.CopyExternalConnectionStrings(webConfigManager);
                transform.CopyExternalTokens(webConfigManager);
            }
        }
    }
        
}
