namespace Web.Config.Transform.Buildpack
{
    public interface IWebConfigTransformHandler
    {
        void ApplyXmlTransformation(string buildPath, IEnvironmentWrapper environmentWrapper, IWebConfigWriter webConfigWriter);

        void CopyExternalAppSettings(IWebConfigWriter webConfigWriter);

        void CopyExternalConnectionStrings(IWebConfigWriter webConfigWriter);

        void CopyExternalTokens(IWebConfigWriter webConfigWriter);
    }
}
