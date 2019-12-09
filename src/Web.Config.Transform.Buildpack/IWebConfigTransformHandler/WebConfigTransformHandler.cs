using Microsoft.Extensions.Configuration;
using System;

namespace Web.Config.Transform.Buildpack
{
    public class WebConfigTransformHandler : IWebConfigTransformHandler
    {
        private readonly IConfigurationRoot _config;
        private readonly IWebConfigReader _webConfigReader;

        public WebConfigTransformHandler(
            IConfigurationRoot config,
            IWebConfigReader webConfigReader)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "Application configuration is required");
            _webConfigReader = webConfigReader ?? throw new ArgumentNullException(nameof(webConfigReader), "WebConfig reader is required");
        }

        public void CopyExternalAppSettings(IWebConfigWriter webConfigWriter)
        {
            if(webConfigWriter == null)
                throw new ArgumentNullException(nameof(webConfigWriter), "WebConfig writer is required");

            foreach(var appSetting in _webConfigReader.GetAppSettings())
            {
                var value = _config[$"appSettings:{appSetting.Key}"];

                if (!string.IsNullOrEmpty(value))
                    webConfigWriter.SetAppSetting(appSetting.Key, value);
            }
        }

        public void CopyExternalConnectionStrings(IWebConfigWriter webConfigWriter)
        {
            if (webConfigWriter == null)
                throw new ArgumentNullException(nameof(webConfigWriter), "WebConfig writer is required");

            foreach (var connectionString in _webConfigReader.GetConnectionStrings())
            {
                var value = _config[$"connectionStrings:{connectionString.Key}"];

                if (!string.IsNullOrEmpty(value))
                    webConfigWriter.SetConnectionString(connectionString.Key, value);
            }
        }

        public void CopyExternalTokens(IWebConfigWriter webConfigWriter)
        {
            if (webConfigWriter == null)
                throw new ArgumentNullException(nameof(webConfigWriter), "WebConfig writer is required");

            webConfigWriter.InitializeWebConfigForTokenReplacements();

            foreach (var configEntry in _config.AsEnumerable())
                webConfigWriter.ReplaceToken(configEntry.Key, configEntry.Value);

            webConfigWriter.FinalizeWebConfigTokenReplacements();
        }
    }
}
