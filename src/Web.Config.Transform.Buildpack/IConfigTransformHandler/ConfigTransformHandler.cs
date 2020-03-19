using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Web.Config.Transform.Buildpack
{
    public class ConfigTransformHandler : IConfigTransformHandler
    {
        private readonly IConfigurationRoot _config;
        private readonly IConfigReader _webConfigReader;
        private readonly IConfigWriter _webConfigWriter;

        public ConfigTransformHandler(
            IConfigurationRoot config,
            IConfigReader webConfigReader,
            IConfigWriter webConfigWriter)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "Application configuration is required");
            _webConfigReader = webConfigReader ?? throw new ArgumentNullException(nameof(webConfigReader), "WebConfig reader is required");
            _webConfigWriter = webConfigWriter ?? throw new ArgumentNullException(nameof(webConfigWriter), "WebConfig writer is required");
        }

        public void ApplyXmlTransformation(string transformConfigPath)
        {
            if (File.Exists(transformConfigPath))
            {
                _webConfigWriter.ExecuteXmlTransformation(transformConfigPath);
            }
        }

        public void CopyExternalAppSettings()
        {
            foreach (var appSetting in _webConfigReader.GetAppSettings())
            {
                var value = _config[$"appSettings:{appSetting.Key}"];

                if (!string.IsNullOrEmpty(value))
                    _webConfigWriter.SetAppSetting(appSetting.Key, value);
            }
        }

        public void CopyExternalConnectionStrings()
        {
            foreach (var connectionString in _webConfigReader.GetConnectionStrings())
            {
                var value = _config[$"connectionStrings:{connectionString.Key}"];

                if (!string.IsNullOrEmpty(value))
                    _webConfigWriter.SetConnectionString(connectionString.Key, value);
            }
        }

        public void CopyExternalTokens()
        {
            _webConfigWriter.InitializeWebConfigForTokenReplacements();

            foreach (var configEntry in _config.AsEnumerable())
                _webConfigWriter.ReplaceToken(configEntry.Key, configEntry.Value);

            _webConfigWriter.FinalizeWebConfigTokenReplacements();
        }
    }
}
