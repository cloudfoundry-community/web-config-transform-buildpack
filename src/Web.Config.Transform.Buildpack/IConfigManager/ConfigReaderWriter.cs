using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Web.Config.Transform.Buildpack
{
    public class ConfigReaderWriter : IConfigReader, IConfigWriter, IDisposable
    {
        private const string APPSETTINGS_SELECTOR = "/configuration/appSettings/add";
        private const string CONNECTIONSTRINGS_SELECTOR = "/configuration/connectionStrings/add";

        private XmlDocument _configXmlDoc;
        private string _configXmlString;

        public ConfigReaderWriter(ConfigManagerSettings settings)
        {
            ConfigSettings = settings;
            _configXmlDoc = settings.XmlDocumentWrapper.CreateXmlDocFromFile(ConfigSettings.SourceConfigPath);

            BackupConfig();
        }

        public ConfigManagerSettings ConfigSettings { get; }


        private void BackupConfig()
        {
            ConfigSettings.FileWrapper.Copy(ConfigSettings.SourceConfigPath, $"{ConfigSettings.SourceConfigPath}.orig");
        }

        public void ExecuteXmlTransformation(string transformFilePath)
        {
            ConfigSettings.Logger.WriteLog($"-----> Applying {transformFilePath} transform to {ConfigSettings.SourceConfigPath}");
            var transform = new Microsoft.Web.XmlTransform.XmlTransformation(transformFilePath);
            transform.Apply(_configXmlDoc);
        }

        public void SetAppSetting(string key, string value)
        {
            if (_configXmlDoc == null) throw new ArgumentNullException(nameof(_configXmlDoc), "Config file is not loaded as xml document");
            var setting = _configXmlDoc.SelectSingleNode($"{APPSETTINGS_SELECTOR}[@key='{key}']");

            if (setting != null)
            {
                ConfigSettings.Logger.WriteLog($"-----> Replacing value for matching appSetting key `{key}` in {ConfigSettings.SourceConfigPath}");
                setting.Attributes["value"].Value = value;
            }
        }

        public string GetAppSetting(string key)
        {
            var settings = GetAppSettings();
            var setting = settings?.FirstOrDefault(s => s.Key == key);
            return setting?.Value;
        }

        public List<KeyValuePair<string, string>> GetAppSettings()
        {
            if (_configXmlDoc == null) throw new ArgumentNullException(nameof(_configXmlDoc), "Config file is not loaded as xml document");

            var section = _configXmlDoc.SelectNodes(APPSETTINGS_SELECTOR).OfType<XmlElement>();

            if (section == null) return default;

            var appSettings = new List<KeyValuePair<string, string>>();
            foreach (var setting in section)
            {
                appSettings.Add(new KeyValuePair<string, string>(setting.GetAttribute("key"), setting.GetAttribute("value")));
            }
            return appSettings;
        }

        public string GetConnectionString(string name)
        {
            var connectionStrings = GetConnectionStrings();
            var connectionString = connectionStrings?.FirstOrDefault(s => s.Key == name);
            return connectionString?.Value;
        }

        public List<KeyValuePair<string, string>> GetConnectionStrings()
        {
            if (_configXmlDoc == null) throw new ArgumentNullException(nameof(_configXmlDoc), "Config file is not loaded as xml document");

            var section = _configXmlDoc.SelectNodes(CONNECTIONSTRINGS_SELECTOR).OfType<XmlElement>();

            if (section == null) return default;

            var connectionStrings = new List<KeyValuePair<string, string>>();
            foreach (var connectionString in section)
            {
                connectionStrings.Add(new KeyValuePair<string, string>(connectionString.GetAttribute("name"), connectionString.GetAttribute("connectionString")));
            }
            return connectionStrings;
        }

        public void SetConnectionString(string name, string connectionString)
        {
            if (_configXmlDoc == null) throw new ArgumentNullException(nameof(_configXmlDoc), "Config file is not loaded as xml document");
            var setting = _configXmlDoc.SelectSingleNode($"{CONNECTIONSTRINGS_SELECTOR}[@name='{name}']");

            if (setting == null) { return; }

            ConfigSettings.Logger.WriteLog($"-----> Replacing connectionString for matching connectionString name `{name}` in {ConfigSettings.SourceConfigPath}");
            setting.Attributes["connectionString"].Value = connectionString;
        }

        public void InitializeWebConfigForTokenReplacements()
        {
            _configXmlString = ConfigSettings.XmlDocumentWrapper.ConvertXmlDocToString(_configXmlDoc);
        }

        public void FinalizeWebConfigTokenReplacements()
        {
            _configXmlDoc = ConfigSettings.XmlDocumentWrapper.CreateXmlDocFromString(_configXmlString);
        }

        public void ReplaceToken(string token, string value)
        {
            var replaceToken = $"#{{{token}}}";

            if (ValueExistsInXmlDoc(replaceToken))
            {
                ConfigSettings.Logger.WriteLog($"-----> Replacing token `{replaceToken}` in {ConfigSettings.SourceConfigPath}");
                _configXmlString = _configXmlString.Replace(replaceToken, value);
            }
            else
            {
                if (IsTraceEnabled())
                {
                    ConfigSettings.Logger.WriteLog($"-----> TRACE: Token `{replaceToken}` not found in {ConfigSettings.SourceConfigPath}");
                }
            }
        }

        public bool ValueExistsInXmlDoc(string value)
        {
            return _configXmlString.Contains(value);
        }

        public bool IsTraceEnabled()
        {
            return Convert.ToBoolean(Environment.GetEnvironmentVariable(Constants.TRACE_ENABLED_NM) ?? "false");
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) { return; }

            if (disposing)
            {
                // Use dispose as a save opportunity
                ConfigSettings.XmlDocumentWrapper.SaveXmlDocAsFile(_configXmlDoc, ConfigSettings.SourceConfigPath);
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
