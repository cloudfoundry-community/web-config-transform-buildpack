using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class WebConfigManager : IWebConfigReader, IWebConfigWriter, IDisposable
    {
        private readonly IFileWrapper _fileWrapper;
        private readonly IXmlDocumentWrapper _xmlDocumentWrapper;

        private const string APPSETTINGS_SELECTOR = "/configuration/appSettings/add";
        private const string CONNECTIONSTRINGS_SELECTOR = "/configuration/connectionStrings/add";

        private string _configFile;
        private XmlDocument _configXmlDoc;
        private string _configXmlString;

        public WebConfigManager(
            IFileWrapper fileWrapper, 
            IXmlDocumentWrapper xmlDocumentWrapper, 
            string configFile = "web.config")
        {
            _fileWrapper = fileWrapper ?? throw new ArgumentNullException(nameof(fileWrapper), "File wrapper is required");
            _xmlDocumentWrapper = xmlDocumentWrapper ?? throw new ArgumentNullException(nameof(xmlDocumentWrapper), "Xml document wrapper is required");
            
            if (!_fileWrapper.Exists(configFile))
                throw new ArgumentNullException(nameof(configFile), "Web config file does not exist. Exiting the program.");

            _configFile = configFile;
            _configXmlDoc = _xmlDocumentWrapper.CreateXmlDocFromFile(_configFile);
            BackupConfig();
        }

        public void Dispose()
        {
            _xmlDocumentWrapper.SaveXmlDocAsFile(_configXmlDoc, _configFile);
        }

        private void BackupConfig()
        {
            _fileWrapper.Copy(_configFile, $"{_configFile}.orig");
        }

        public void SetAppSetting(string key, string value)
        {
            if (_configXmlDoc == null) throw new ArgumentNullException(nameof(_configXmlDoc), "Config file is not loaded as xml document");
            var setting = _configXmlDoc.SelectSingleNode($"{APPSETTINGS_SELECTOR}[@key='{key}']");

            if (setting != null)
            {
                Console.WriteLine($"-----> Replacing value for matching appSetting key `{key}` in web.config");
                setting.Attributes["value"].Value = value;
            }
        }

        public string GetAppSetting(string key)
        {
            //var setting = _ConfigXmlDoc.SelectSingleNode($"{APPSETTINGS_SELECTOR}[@key='{key}']/@value");
            //return setting?.Value;

            var settings = GetAppSettings();
            var setting = settings?.FirstOrDefault(s => s.Key == key);
            return setting?.Value;
        }

        public List<KeyValuePair<string, string>> GetAppSettings()
        {
            if (_configXmlDoc == null) throw new ArgumentNullException(nameof(_configXmlDoc), "Config file is not loaded as xml document");

            var section = _configXmlDoc.SelectNodes(APPSETTINGS_SELECTOR).OfType<XmlElement>();
            // TODO: check if this is really required. 
            //if (settings == null) throw new ArgumentNullException(nameof(section), "AppSettings section not found in config file");
            if (section == null) return null;

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
            // TODO: check if this is really required. 
            //if (section == null) throw new ArgumentNullException(nameof(connectionStrings), "ConnectionStrings section not found in config file");
            if (section == null) return null;

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

            if (setting != null)
            {
                Console.WriteLine($"-----> Replacing connectionString for matching connectionString name `{name}` in web.config");
                setting.Attributes["connectionString"].Value = connectionString;
            }
        }

        public void InitializeWebConfigForTokenReplacements()
        {
            _configXmlString = _xmlDocumentWrapper.ConvertXmlDocToString(_configXmlDoc);
        }

        public void FinalizeWebConfigTokenReplacements()
        {
            _configXmlDoc = _xmlDocumentWrapper.CreateXmlDocFromString(_configXmlString);
        }

        public void ReplaceToken(string token, string value)
        {
            var replaceToken = "#{" + token + "}";

            if (ValueExistsInXmlDoc(replaceToken))
            {
                Console.WriteLine($"-----> Replacing token `{replaceToken}` in web.config");
                _configXmlString = _configXmlString.Replace(replaceToken, value);
            }
        }

        public bool ValueExistsInXmlDoc(string value)
        {
            return _configXmlString.Contains(value);
        }
    }
}
