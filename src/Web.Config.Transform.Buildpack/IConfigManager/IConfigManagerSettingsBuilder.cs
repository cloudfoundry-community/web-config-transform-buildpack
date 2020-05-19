namespace Web.Config.Transform.Buildpack
{
    public interface IConfigManagerSettingsBuilder
    {
        IFileWrapper FileWrapper { get; set; }
        IXmlDocumentWrapper XmlDocumentWrapper { get; set; }
        IEnvironmentWrapper EnvironmentWrapper { get; set; }
        IConfigurationFactory ConfigurationFactory { get; set; }
        ILogger Logger { get; set; }
        string BuildPath { get; set; }
        ConfigManagerSettings Build();
        (string transformationConfigPath, string sourceConfigPath) GetConfigPaths();
    }
}