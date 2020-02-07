using FluentAssertions;
using Moq;
using Web.Config.Transform.Buildpack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace UnitTests
{
    public class WebConfigTransformBuildpackTests
    {
        private Mock<IEnvironmentWrapper> _environmentWrapperMock;
        private Mock<ITracer> _environmentTracerMock;
        private Mock<IConfigurationFactory> _configurationFactoryMock;
        private Mock<IFileWrapper> _fileWrapperMock;
        private Mock<IXmlDocumentWrapper> _xmlDocumentWrapperMock;
        private Mock<ILogger> _consoleLoggerMock;
        private WebConfigTransformBuildpack _buildpack;

        public WebConfigTransformBuildpackTests()
        {
            _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
            _environmentTracerMock = new Mock<ITracer>();
            _configurationFactoryMock = new Mock<IConfigurationFactory>();
            _fileWrapperMock = new Mock<IFileWrapper>();
            _xmlDocumentWrapperMock = new Mock<IXmlDocumentWrapper>();
            _consoleLoggerMock = new Mock<ILogger>();

            _buildpack = new WebConfigTransformBuildpack(
               _environmentWrapperMock.Object,
               _environmentTracerMock.Object,
               _configurationFactoryMock.Object,
               _fileWrapperMock.Object,
               _xmlDocumentWrapperMock.Object,
               _consoleLoggerMock.Object
               );
        }

        [Fact(Skip = "Skip until there is an abstraction for transform-worker creation")]
        public void BuildPackRun_Should_InvokeAllActionsOnTrasformHandler()
        {
            //_buildpack.Run(new[] { "supply", "", "", "", "0" });

            // TODO:    we can't do this unless we delgate transform-worker creation to another class
            //          then we can return mock transformHandler and verify these methods are invoked
            
            // CopyExternalAppSettings on transformHandler mock should be called once
            // CopyExternalConnectionStrings on transformHandler mock should be called once
            // CopyExternalTokens on transformHandler mock should be called once
        }

        [Fact]
        public void When_TransformationFileNotExists_VariableExists_ApplyWebConfigTransform_ShouldNotFind_WebEnvironmetConfig()
        {
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .Returns("Development");

            var environment = _environmentWrapperMock.Object.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";

            var xdt = Path.Combine("", $"web.{environment}.config");
            var fileExists = File.Exists(xdt);

            fileExists.Should().BeFalse();
        }

        [Fact]
        public void When_TransformationFileExists_ApplyWebConfigTransform_ShouldFind_WebEnvironmetConfig()
        {
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .Returns("Release");

            var environment = _environmentWrapperMock.Object.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Release";

            var xdt = Path.Combine("", $"web.{environment}.config");
            var fileExists = File.Exists(xdt);

            fileExists.Should().BeTrue();
        }
    }
}
