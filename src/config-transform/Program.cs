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
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: config-transform.exe <appDir>");
                Environment.Exit(1);
            }
            var appPath = args[0];
            
            var config = new ConfigurationBuilder().AddCloudFoundry().Build();
            var isConfigServerBound = config.AsEnumerable().Select(x => x.Key).Any(x => x == "vcap:services:p-config-server:0");
            
            var webConfig = Path.Combine(appPath, "web.config");
            if(!File.Exists(webConfig))
                Environment.Exit(0);
            
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";
            var xdt = Path.Combine(appPath, $"web.{environment}.config");
            if (!string.IsNullOrEmpty(environment) && File.Exists(xdt))
            {
                Console.WriteLine($"Applying {xdt} to web.config");
                var transform = new Microsoft.Web.XmlTransform.XmlTransformation(xdt);
                var doc = new XmlDocument();
                
                doc.Load(webConfig);
                transform.Apply(doc);

                var adds = doc.SelectNodes("/configuration/appSettings/add").OfType<XmlElement>();
	
                foreach(var add in adds)
                {
		
                    var key = add.GetAttribute("name");
                    if(key == null)
                        continue;
                    var envVal = Environment.GetEnvironmentVariable(key);
                    if(envVal != null)
                        add.SetAttribute("value", envVal);
                }
                
                if (!File.Exists(webConfig + ".orig")) // backup original web.config as we're gonna transform into it's place
                    File.Move(webConfig, webConfig + ".orig");
                doc.Save(webConfig);
            }

            if (isConfigServerBound)
            {
                config = new ConfigurationBuilder().AddConfigServer().Build();
                Console.WriteLine("Config server binding found - replacing matching variables in web.config");
                        
                var webConfigContent = File.ReadAllText(webConfig);
                foreach (var configEntry in config.AsEnumerable())
                {
                    webConfigContent = webConfigContent.Replace("#{" + configEntry.Key + "}", configEntry.Value);
                }
                File.WriteAllText(webConfig, webConfigContent);
            }
        }
    }
}
