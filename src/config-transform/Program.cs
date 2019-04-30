using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Configuration;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace config_transform
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: config-transform.exe <appDir>");
                Environment.Exit(1);
            }
            var appPath = args[0];

            var webConfig = Path.Combine(appPath, "web.config");
            if (!File.Exists(webConfig))
            {
                Console.WriteLine("Web.config not detected");
                Environment.Exit(0);
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";

            var config = new ConfigurationBuilder().AddCloudFoundry().Build();
            var isConfigServerBound = config.AsEnumerable().Select(x => x.Key).Any(x => x == "vcap:services:p-config-server:0");
            var configBuilder = new ConfigurationBuilder();

            if (isConfigServerBound)
            {
                configBuilder.AddConfigServer(environment);
                Console.WriteLine("Config server binding found - using values for token replacement");
            }

            configBuilder.AddCloudFoundry();
            configBuilder.AddEnvironmentVariables();
            config = configBuilder.Build();

            var xdt = Path.Combine(appPath, $"web.{environment}.config");
            var doc = new XmlDocument();
            doc.Load(webConfig);

            if (!string.IsNullOrEmpty(environment) && File.Exists(xdt))
            {
                Console.WriteLine($"Applying {xdt} to web.config");
                var transform = new Microsoft.Web.XmlTransform.XmlTransformation(xdt);
                transform.Apply(doc);
            }

            Console.WriteLine("Replacing matching keys in appSettings with values in config provider");
            var adds = doc.SelectNodes("/configuration/appSettings/add").OfType<XmlElement>();

            foreach (var add in adds)
            {

                var key = add.GetAttribute("key");
                if (key == null)
                    continue;
                var envVal = Environment.GetEnvironmentVariable(key);
                if (envVal != null)
                    add.SetAttribute("value", envVal);
            }

            var connStr = doc.SelectNodes("/configuration/connectionStrings/add").OfType<XmlElement>();

            foreach (var add in connStr)
            {

                var key = add.GetAttribute("name");
                if (key == null)
                    continue;
                var envVal = Environment.GetEnvironmentVariable(key);
                if (envVal != null)
                    add.SetAttribute("connectionString", envVal);
            }

            if (!File.Exists(webConfig + ".orig")) // backup original web.config as we're gonna transform into it's place
                File.Move(webConfig, webConfig + ".orig");
            doc.Save(webConfig);

            Console.WriteLine("Replacing matching variable token in web.config");

            var webConfigContent = File.ReadAllText(webConfig);
            foreach (var configEntry in config.AsEnumerable())
            {
                var replaceToken = $"#{configEntry.Key}";
                if (webConfigContent.Contains(replaceToken))
                {
                    Console.WriteLine($"Replacing token `{replaceToken}` in web.config");
                    webConfigContent = webConfigContent.Replace(replaceToken, configEntry.Value);
                }
            }
            File.WriteAllText(webConfig, webConfigContent);
        }
    }
}
