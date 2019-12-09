using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace Web.Config.Transform.Buildpack
{
    public class WebConfigTransformBuildpack : SupplyBuildpack
    {
        IEnvironmentWrapper _environmentWrapper;
        IConfigurationFactory _configurationFactory;
        IFileWrapper _fileWrapper;
        IXmlDocumentWrapper _xmlDocumentWrapper;

        public WebConfigTransformBuildpack(
            IEnvironmentWrapper environmentWrapper, 
            IConfigurationFactory configurationFactory,
            IFileWrapper fileWrapper,
            IXmlDocumentWrapper xmlDocumentWrapper)
        {
            _environmentWrapper = environmentWrapper;
            _configurationFactory = configurationFactory;
            _fileWrapper = fileWrapper;
            _xmlDocumentWrapper = xmlDocumentWrapper;
        }

        protected override bool Detect(string buildPath)
        {
            return false;
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("=============== WebConfig Transform Buildpack execution started ================");
            Console.WriteLine("================================================================================");

            ApplyTransformations(Path.Combine(buildPath, "web.config"));

            Console.WriteLine("================================================================================");
            Console.WriteLine("============== WebConfig Transform Buildpack execution completed ===============");
            Console.WriteLine("================================================================================");
        }

        private void ApplyTransformations(string webConfig)
        {
            using (var webConfigManager = new WebConfigManager(_fileWrapper, _xmlDocumentWrapper, webConfig))
            {
                var environment = _environmentWrapper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";
                Console.WriteLine($"-----> Using Environment: {environment}");
                var config = _configurationFactory.GetConfiguration(environment);

                var transform = new WebConfigTransformHandler(config, webConfigManager);

                transform.CopyExternalAppSettings(webConfigManager);
                transform.CopyExternalConnectionStrings(webConfigManager);
                transform.CopyExternalTokens(webConfigManager);
            }
        }
    }
        
}
