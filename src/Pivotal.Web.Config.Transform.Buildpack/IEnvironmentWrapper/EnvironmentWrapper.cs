using System;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public void Exit(int code)
        {
            Environment.Exit(code);
        }

        public string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}
