using System;

namespace Pivotal.Web.Config.Transform.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {
            return new WebConfigTransformBuildpack().Run(args);
        }
    }
}