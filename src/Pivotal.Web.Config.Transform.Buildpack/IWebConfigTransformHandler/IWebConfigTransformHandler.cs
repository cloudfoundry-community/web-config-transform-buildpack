namespace Pivotal.Web.Config.Transform.Buildpack
{
    public interface IWebConfigTransformHandler
    {
        void CopyExternalAppSettings(IWebConfigWriter webConfigWriter);

        void CopyExternalConnectionStrings(IWebConfigWriter webConfigWriter);

        void CopyExternalTokens(IWebConfigWriter webConfigWriter);
    }
}
