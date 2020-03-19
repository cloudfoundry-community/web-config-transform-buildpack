namespace Web.Config.Transform.Buildpack
{
    public interface IConfigTransformHandler
    {
        void ApplyXmlTransformation(string transformConfigPath);

        void CopyExternalAppSettings();

        void CopyExternalConnectionStrings();

        void CopyExternalTokens();
    }
}
