using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Web.Config.Transform.Buildpack;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class WebConfigTransformHandlerTests
    {
        private readonly IConfigurationRoot _configMock;
        private readonly Mock<IConfigReader> _webConfigReaderMock;
        private readonly Mock<IConfigWriter> _webConfigWriterMock;
        private readonly ConfigTransformHandler _transformHandler;

        public WebConfigTransformHandlerTests()
        {
            _configMock = GetMockConfiguration();
            _webConfigReaderMock = new Mock<IConfigReader>();
            _webConfigWriterMock = new Mock<IConfigWriter>();

            _transformHandler = new ConfigTransformHandler(_configMock, _webConfigReaderMock.Object, _webConfigWriterMock.Object);
        }

        private IConfigurationRoot GetMockConfiguration()
        {
            var externalAppSettings = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("appSettings:BP_AppSettings_Key1", "External_AppSettings_Value1"),
                new KeyValuePair<string, string>("connectionStrings:BP_ConnectionStrings_Key1", "External_ConnectionStrings_Value1"),
                new KeyValuePair<string, string>("BP_Token1", "External_BP_Token1_Value"),
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(externalAppSettings).Build();
            return config;
        }

        [Fact]
        public void When_ConfigIsNull_Constructor_Should_ThrowException()
        {
            Action handlerCreation = () => { new ConfigTransformHandler(null, _webConfigReaderMock.Object, _webConfigWriterMock.Object); };

            handlerCreation.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_WebConfigReaderIsNull_Constructor_Should_ThrowException()
        {
            Action handlerCreation = () => { new ConfigTransformHandler(_configMock, null, _webConfigWriterMock.Object); };

            handlerCreation.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_WebConfigWriterIsNull_Constructor_Should_ThrowException()
        {
            Action handlerCreation = () => { new ConfigTransformHandler(_configMock, _webConfigReaderMock.Object, null); };

            handlerCreation.Should().Throw<ArgumentNullException>();
        }
        
        [Fact]
        public void When_ExternalConfigHasAppSetting_CopyExternalAppSettings_Should_CallSetAppSetting()
        {
            var configAppSettings = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("BP_AppSettings_Key1", "AppSettings_Value1"),
                new KeyValuePair<string, string>("BP_AppSettings_Key2", "AppSettings_Value2"),
            };
            _webConfigReaderMock.Setup(r => r.GetAppSettings()).Returns(configAppSettings);

            _transformHandler.CopyExternalAppSettings();

            _webConfigWriterMock.Verify(w => w.SetAppSetting(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void When_ExternalConfigHasConnectionStrings_CopyExternalConnectionStrings_Should_CallSetConnectionString()
        {
            var configConnectionStrings = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("BP_ConnectionStrings_Key1", "ConnectionStrings_Value1"),
                new KeyValuePair<string, string>("BP_ConnectionStrings_Key2", "ConnectionStrings_Value2"),
            };
            _webConfigReaderMock.Setup(r => r.GetConnectionStrings()).Returns(configConnectionStrings);

            _transformHandler.CopyExternalConnectionStrings();

            _webConfigWriterMock.Verify(w => w.SetConnectionString(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void When_ExternalConfigHasTokens_CopyExternalTokens_Should_CallInitializeReplaceFinalize()
        {
            _transformHandler.CopyExternalTokens();

            _webConfigWriterMock.Verify(w => w.InitializeWebConfigForTokenReplacements(), Times.Once);
            _webConfigWriterMock.Verify(w => w.ReplaceToken(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            _webConfigWriterMock.Verify(w => w.FinalizeWebConfigTokenReplacements(), Times.Once);
        }
    }
}
