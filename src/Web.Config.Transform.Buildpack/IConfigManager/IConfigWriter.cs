namespace Web.Config.Transform.Buildpack
{
    public interface IConfigWriter
    {
        void ExecuteXmlTransformation(string transformFilePath);

        void SetAppSetting(string key, string value);

        void SetConnectionString(string name, string connectionString);

        void InitializeWebConfigForTokenReplacements();

        void ReplaceToken(string token, string value);

        void FinalizeWebConfigTokenReplacements();
    }
}
