﻿using Xunit;
using FluentAssertions;
using System;
using Moq;
using Web.Config.Transform.Buildpack;
using System.Xml;
using System.Collections.Generic;

namespace UnitTests
{
    public class WebConfigManagerTests
    {
        private Mock<IFileWrapper> _fileWrapperMock;
        private Mock<IXmlDocumentWrapper> _xmlDocumentWrapperMock;
        private Mock<ILogger> _consoleLoggerMock;
        private Mock<IEnvironmentWrapper> _environmentWrapperMock;
        private Mock<IConfigurationFactory> _configurationFactoryMock;
        private Mock<ConfigManagerSettingsBuilder> _configManagerSettingsBuilder;

        private ConfigReaderWriter _writer;
        private const string WEB_CONFIG_FILE_NAME = "web.config";

        public WebConfigManagerTests()
        {
            _fileWrapperMock = new Mock<IFileWrapper>();
            _xmlDocumentWrapperMock = new Mock<IXmlDocumentWrapper>();
            _consoleLoggerMock = new Mock<ILogger>();
            _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
            _configurationFactoryMock = new Mock<IConfigurationFactory>();
            _configManagerSettingsBuilder = new Mock<ConfigManagerSettingsBuilder>();
        }

        [Fact]
        public void When_WebConfigFileDoesNotExist_Constructor_Should_ThrowError()
        {
            _fileWrapperMock = new Mock<IFileWrapper>();
            _fileWrapperMock.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);

            Assert.Throws<ArgumentNullException>(() =>
                new Web.Config.Transform.Buildpack.ConfigReaderWriterBuilder(options =>
                {
                    options.FileWrapper = _fileWrapperMock.Object;
                    options.XmlDocumentWrapper = _xmlDocumentWrapperMock.Object;
                    options.Logger = _consoleLoggerMock.Object;
                    options.BuildPath = "file_that_doesnot_exist";
                }).Build());
        }

        [Fact]
        public void When_WebConfigExists_Constructor_Should_LoadConfigAsXmlDocument()
        {
            _fileWrapperMock.Setup(f => f.Exists(It.IsAny<string>())).Returns(true); 
            _configurationFactoryMock.Setup(f => f.GetConfiguration(It.IsAny<string>())).Returns(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
            _configManagerSettingsBuilder.Setup(f => f.GetConfigPaths()).Returns(("", WEB_CONFIG_FILE_NAME));

            _writer = new ConfigReaderWriterBuilder(_configManagerSettingsBuilder.Object, options =>
            {
                options.ConfigurationFactory = _configurationFactoryMock.Object;
                options.FileWrapper = _fileWrapperMock.Object;
                options.XmlDocumentWrapper = _xmlDocumentWrapperMock.Object;
                options.Logger = _consoleLoggerMock.Object;
                options.EnvironmentWrapper = _environmentWrapperMock.Object;
                options.BuildPath = WEB_CONFIG_FILE_NAME;
            }).Build();

            _xmlDocumentWrapperMock.Verify(x => x.CreateXmlDocFromFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void When_WebConfigExists_Constructor_Should_BackupConfigFile()
        {
            _fileWrapperMock.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _configurationFactoryMock.Setup(f => f.GetConfiguration(It.IsAny<string>())).Returns(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
            _configManagerSettingsBuilder.Setup(f => f.GetConfigPaths()).Returns(("", WEB_CONFIG_FILE_NAME));

            _writer = new ConfigReaderWriterBuilder(_configManagerSettingsBuilder.Object, options =>
            {
                options.ConfigurationFactory = _configurationFactoryMock.Object;
                options.FileWrapper = _fileWrapperMock.Object;
                options.XmlDocumentWrapper = _xmlDocumentWrapperMock.Object;
                options.Logger = _consoleLoggerMock.Object;
                options.EnvironmentWrapper = _environmentWrapperMock.Object;
                options.BuildPath = WEB_CONFIG_FILE_NAME;
            }).Build();

            _fileWrapperMock.Verify(f => f.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void When_ConfigIsLoaded_GetAppSetting_Should_ReturnSpecificAppSetting()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new KeyValuePair<string, string>("BP_AppSettings_Key1", "AppSettings_Value1");

            var actual = _writer.GetAppSetting(expected.Key);

            actual.Should().NotBeNullOrEmpty();
            expected.Value.Should().BeEquivalentTo(actual);
        }

        [Fact]
        public void When_ConfigIsLoaded_GetAppSettings_Should_ReturnAllAppSettings()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new List<KeyValuePair<string, string>>();
            expected.Add(new KeyValuePair<string, string>("BP_AppSettings_Key1", "AppSettings_Value1"));
            expected.Add(new KeyValuePair<string, string>("BP.AppSettings.Key1", "AppSettings_Value1_For_Dotted_Key"));

            var actual = _writer.GetAppSettings();

            actual.Should().NotBeNullOrEmpty().And.BeAssignableTo<List<KeyValuePair<string, string>>>();
            expected.Should().Equals(actual);
        }

        [Fact]
        public void When_AppSettingIsPassed_SetAppSetting_Should_UpdateConfigXml()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new KeyValuePair<string, string>("BP_AppSettings_Key1", "TestValue");

            _writer.SetAppSetting(expected.Key, expected.Value);

            var actual = _writer.GetAppSetting(expected.Key);
            actual.Should().NotBeNullOrEmpty();
            expected.Value.Should().BeEquivalentTo(actual);
        }

        [Fact]
        public void When_AppSettingIsPassed_SetDottedAppSetting_Should_UpdateConfigXml()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new KeyValuePair<string, string>("BP.AppSettings.Key1", "AppSettings_Value1_For_Dotted_Key");

            _writer.SetAppSetting(expected.Key, expected.Value);

            var actual = _writer.GetAppSetting(expected.Key);
            actual.Should().NotBeNullOrEmpty();
            expected.Value.Should().BeEquivalentTo(actual);
        }

        [Fact]
        public void When_ConfigIsLoaded_GetConnectionString_Should_ReturnSpecificConnectionString()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new KeyValuePair<string, string>("BP_ConnectionStrings_Key1", "ConnectionStrings_Value1");

            var actual = _writer.GetConnectionString(expected.Key);

            actual.Should().NotBeNullOrEmpty();
            expected.Value.Should().BeEquivalentTo(actual);
        }

        [Fact]
        public void When_ConfigIsLoaded_GetConnectionStrings_Should_ReturnAllConnectionStrings()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new List<KeyValuePair<string, string>>();
            expected.Add(new KeyValuePair<string, string>("BP_ConnectionStrings_Key1", "ConnectionStrings_Value1"));

            var actual = _writer.GetConnectionStrings();

            actual.Should().NotBeNullOrEmpty().And.BeAssignableTo<List<KeyValuePair<string, string>>>();
            expected.Should().Equals(actual);
        }

        [Fact]
        public void When_ConnectionStringIsPassed_SetConnectionString_Should_UpdateConfigXml()
        {
            LoadWebConfigAsXmlDocument();
            var expected = new KeyValuePair<string, string>("BP_ConnectionStrings_Key1", "TestValue");

            //Action writer = () => { _writer.SetAppSetting(key, value); };
            //writer.Should().Throw<ArgumentNullException>();

            _writer.SetConnectionString(expected.Key, expected.Value);

            var actual = _writer.GetConnectionString(expected.Key);
            actual.Should().NotBeNullOrEmpty();
            expected.Value.Should().BeEquivalentTo(actual);
        }

        [Fact]
        public void InitializeWebConfigForTokenReplacements_Should_CallXmlDocumentWrapperConvertXmlDocToString()
        {
            LoadWebConfigAsXmlDocument();
            _writer.InitializeWebConfigForTokenReplacements();

            _xmlDocumentWrapperMock.Verify(
                x => x.ConvertXmlDocToString(It.IsAny<XmlDocument>()), Times.Once);
        }

        [Fact]
        public void FinalizeWebConfigTokenReplacements_Should_CallXmlDocumentWrapperCreateXmlDocFromString()
        {
            LoadWebConfigAsXmlDocument();
            _writer.InitializeWebConfigForTokenReplacements();
            _writer.FinalizeWebConfigTokenReplacements();

            _xmlDocumentWrapperMock.Verify(
                x => x.CreateXmlDocFromString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void When_TokenIsPassed_ReplaceToken_Should_ReplaceTokenInConfigXml()
        {
            LoadWebConfigAsXmlDocument();
            var token = new KeyValuePair<string, string>("BP_Token1", "TestValue");

            var xmlContent = @"
                <?xml version='1.0' encoding='utf - 8' ?>
                <configuration>
                    <foo>
                        <bar baz='#{BP_Token1}'></bar>
                      </ foo >
                </configuration>
            ";

            _xmlDocumentWrapperMock
                .Setup(x => x.ConvertXmlDocToString(It.IsAny<XmlDocument>()))
                .Returns(xmlContent);

            _writer.InitializeWebConfigForTokenReplacements();
            _writer.ReplaceToken(token.Key, token.Value);

            var actual = _writer.ValueExistsInXmlDoc(token.Key);
            actual.Should().BeFalse();

            actual = _writer.ValueExistsInXmlDoc(token.Value);
            actual.Should().BeTrue();
        }

        [Fact]
        public void When_TokenIsPassed_ReplaceToken_Should_ReplaceTokenInConfigXml_WhenTheRootElementContainsDot()
        {
            LoadWebConfigAsXmlDocument();
            var token = new KeyValuePair<string, string>("BP_Token1_WithDotInRoot", "TestValue1");

            var xmlContent = @"
                <?xml version='1.0' encoding='utf - 8' ?>
                <configuration>
                    <foo.zuz>
                        <bar baz='#{BP_Token1_WithDotInRoot}'></bar>
                      </ foo.zuz >
                </configuration>
            ";

            _xmlDocumentWrapperMock
                .Setup(x => x.ConvertXmlDocToString(It.IsAny<XmlDocument>()))
                .Returns(xmlContent);

            _writer.InitializeWebConfigForTokenReplacements();
            _writer.ReplaceToken(token.Key, token.Value);

            var actual = _writer.ValueExistsInXmlDoc(token.Key);
            actual.Should().BeFalse();

            actual = _writer.ValueExistsInXmlDoc(token.Value);
            actual.Should().BeTrue();
        }

        #region Private Methods

        private void LoadWebConfigAsXmlDocument()
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(WEB_CONFIG_FILE_NAME);

            _fileWrapperMock.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            _xmlDocumentWrapperMock.Setup(f => f.CreateXmlDocFromFile(It.IsAny<string>())).Returns(xmldoc);
            _configurationFactoryMock.Setup(f => f.GetConfiguration(It.IsAny<string>())).Returns(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

            _configManagerSettingsBuilder.Setup(f => f.GetConfigPaths()).Returns(("", WEB_CONFIG_FILE_NAME));

            _writer = new ConfigReaderWriterBuilder(_configManagerSettingsBuilder.Object, options =>
            {
                options.ConfigurationFactory = _configurationFactoryMock.Object;
                options.EnvironmentWrapper = _environmentWrapperMock.Object;
                options.FileWrapper = _fileWrapperMock.Object;
                options.Logger = _consoleLoggerMock.Object;
                options.XmlDocumentWrapper = _xmlDocumentWrapperMock.Object;
                options.BuildPath = WEB_CONFIG_FILE_NAME;
            }).Build();
        }

        #endregion
    }
}
