using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Web.Config.Transform.Buildpack;
using Xunit;

namespace UnitTests
{
    public class ConfigurationTracerTests
    {
        private Mock<IEnvironmentWrapper> _environmentWrapperMock;
        private Mock<IConfigurationFactory> _configurationFactoryMock;
        private Mock<ILogger> _consoleLoggerMock;

        public ConfigurationTracerTests()
        {
            _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
            _configurationFactoryMock = new Mock<IConfigurationFactory>();
            _consoleLoggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void Trace_Configuration_When_TraceConfig_IsTrue_And_EnvironmentContains_Dev()
        {
            var environment = "Development";
            var traceConfigEnabled = "true";

            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.ASPNETCORE_ENVIRONMENT_NM))).Returns(environment);
            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.TRACE_CONFIG_ENABLED_NM))).Returns(traceConfigEnabled);

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>() { { "foo", "bar"} })
                .Build();

            _configurationFactoryMock.Setup(c => c.GetConfiguration(It.Is<string>(s => s == environment))).Returns(configuration);

            var tracer = new ConfigurationTracer(_environmentWrapperMock.Object, 
                _configurationFactoryMock.Object, 
                _consoleLoggerMock.Object);

            tracer.FlushEnvironmentVariables();

            _consoleLoggerMock.Verify(c => c.WriteLog(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ShouldNot_Trace_Configuration_When_TraceConfig_IsFalse_And_EnvironmentContains_Dev()
        {
            var environment = "Development";
            var traceConfigEnabled = "false";

            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.ASPNETCORE_ENVIRONMENT_NM))).Returns(environment);
            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.TRACE_CONFIG_ENABLED_NM))).Returns(traceConfigEnabled);

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>() { { "foo", "bar" } })
                .Build();

            _configurationFactoryMock.Setup(c => c.GetConfiguration(It.Is<string>(s => s == environment))).Returns(configuration);

            var tracer = new ConfigurationTracer(_environmentWrapperMock.Object,
                _configurationFactoryMock.Object,
                _consoleLoggerMock.Object);

            tracer.FlushEnvironmentVariables();

            _consoleLoggerMock.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ShouldNot_Trace_Configuration_When_TraceConfig_IsTrue_And_EnvironmentDoesNotContain_Dev()
        {
            var environment = "Production";
            var traceConfigEnabled = "true";

            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.ASPNETCORE_ENVIRONMENT_NM))).Returns(environment);
            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.TRACE_CONFIG_ENABLED_NM))).Returns(traceConfigEnabled);

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>() { { "foo", "bar" } })
                .Build();

            _configurationFactoryMock.Setup(c => c.GetConfiguration(It.Is<string>(s => s == environment))).Returns(configuration);

            var tracer = new ConfigurationTracer(_environmentWrapperMock.Object,
                _configurationFactoryMock.Object,
                _consoleLoggerMock.Object);

            tracer.FlushEnvironmentVariables();

            _consoleLoggerMock.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ShouldNot_Trace_Configuration_When_TraceConfig_IsFalse_And_EnvironmentDoesNotContain_Dev()
        {
            var environment = "Production";
            var traceConfigEnabled = "false";

            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.ASPNETCORE_ENVIRONMENT_NM))).Returns(environment);
            _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(It.Is<string>(s => s == Constants.TRACE_CONFIG_ENABLED_NM))).Returns(traceConfigEnabled);

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>() { { "foo", "bar" } })
                .Build();

            _configurationFactoryMock.Setup(c => c.GetConfiguration(It.Is<string>(s => s == environment))).Returns(configuration);

            var tracer = new ConfigurationTracer(_environmentWrapperMock.Object,
                _configurationFactoryMock.Object,
                _consoleLoggerMock.Object);

            tracer.FlushEnvironmentVariables();

            _consoleLoggerMock.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Never);
        }
    }
}
