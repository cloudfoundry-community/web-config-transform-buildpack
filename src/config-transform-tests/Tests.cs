using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Xunit;
using FluentAssertions;

namespace config_transform_tests
{
    public class Tests : IDisposable
    {
        private string _tempFixtureFolder;
        private string _webConfigPath;

        public Tests()
        {
            _tempFixtureFolder = Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFixtureFolder);
            _webConfigPath = Path.Combine(_tempFixtureFolder, "web.config");
            var fixtureFolder = Path.Combine(Directory.GetCurrentDirectory(), "Fixture");
            foreach(var file in Directory.GetFiles(fixtureFolder))
                File.Copy(file, Path.Combine(_tempFixtureFolder, Path.GetFileName(file)));

        }

        [Fact]
        public void TransformTests()
        {
            
            Environment.SetEnvironmentVariable("MyConnectionString","Replaced");
            Environment.SetEnvironmentVariable("MyKey","Replaced");
            Environment.SetEnvironmentVariable("configuration__customkey__somevalue","Replaced");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT","Debug");
            
            config_transform.Program.Main(_tempFixtureFolder);
            
            var doc = new XmlDocument();
            doc.Load(_webConfigPath);

            var node = (XmlElement) doc.SelectSingleNode("/configuration/appSettings/add[@name=\"MyKey\"]");
            node.Should().NotBeNull();
            var nodeValue = node.GetAttribute("value");
            nodeValue.Should().Be("Replaced");
            
            node = (XmlElement) doc.SelectSingleNode("/configuration/connectionStrings/add[@name=\"MyConnectionString\"]");
            node.Should().NotBeNull();
            nodeValue = node.GetAttribute("connectionString");
            nodeValue.Should().Be("Replaced");
            
            node = (XmlElement) doc.SelectSingleNode("/configuration/CustomKey/add[@name=\"SomeValue\"]");
            node.Should().NotBeNull();
            nodeValue = node.GetAttribute("value");
            nodeValue.Should().Be("Replaced");
        }


        public void Dispose()
        {
            Directory.Delete(_tempFixtureFolder);
        }
    }
}