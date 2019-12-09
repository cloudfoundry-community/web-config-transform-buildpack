namespace Web.Config.Transform.Buildpack
{
    public interface IEnvironmentWrapper
    {
        void Exit(int code);
        string GetEnvironmentVariable(string variable);
    }
}
