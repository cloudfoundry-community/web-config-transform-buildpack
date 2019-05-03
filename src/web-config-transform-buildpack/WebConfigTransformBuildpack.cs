using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace WebConfigTransformBuildpack
{
    public class WebConfigTransformBuildpack : SupplyBuildpack
    {

        protected override bool Detect(string buildPath)
        {
            return File.Exists(Path.Combine(buildPath, "web.config"));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            Console.WriteLine("=== WebConfig Transform Buildpack ===");
            var webConfig = Path.Combine(buildPath, "web.config");
            if (!File.Exists(webConfig))
            {
                Console.WriteLine("Web.config not detected");
                Environment.Exit(0);
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";

            Console.WriteLine($"Using Environment: {environment}");

            var config = new ConfigurationBuilder().AddCloudFoundry().Build();
            var isConfigServerBound = config.AsEnumerable().Select(x => x.Key).Any(x => x == "vcap:services:p-config-server:0");
            var configBuilder = new ConfigurationBuilder();

            if (isConfigServerBound)
            {
                var applicationName = Environment.GetEnvironmentVariable("spring:cloud:config:name")
                                        ?? Environment.GetEnvironmentVariable("spring:application:name")
                                        ?? Environment.GetEnvironmentVariable("vcap:application:application_name");

                Console.WriteLine($"Application Name: {applicationName}, used by config server..");

                configBuilder.AddConfigServer(applicationName, environment);

                Console.WriteLine("Config server binding found...");
            }

            configBuilder.AddCloudFoundry();
            configBuilder.AddEnvironmentVariables();
            config = configBuilder.Build();

            var xdt = Path.Combine(buildPath, $"web.{environment}.config");
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
                var replaceToken = "#{" + configEntry.Key + "}";
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
