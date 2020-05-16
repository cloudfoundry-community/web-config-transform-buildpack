using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;

namespace Nuke.Extended
{
    //
    // Summary:
    //     Injects an instance of Nuke.Common.Tools.GitVersion.GitVersion based on the local
    //     repository.
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    public class GitVersionAttribute : InjectionAttributeBase
    {
        public string Framework { get; set; } = "netcoreapp3.1";
        public bool DisableOnUnix { get; set; }
        public bool UpdateAssemblyInfo { get; set; }
        public bool UpdateBuildNumber { get; set; } = true;
        public bool NoFetch { get; set; }
        public string ToolPath => GetToolPath();

        public override object GetValue(MemberInfo member, object instance)
        {
            if (EnvironmentInfo.IsUnix && DisableOnUnix)
            {
                Logger.Warn("GitVersion is disabled on UNIX environment.");
                return null;
            }
            GitVersion item =
                GitVersionTasks.GitVersion((GitVersionSettings s) =>
                    s.SetFramework(Framework)
                    .SetNoFetch(NoFetch)
                    .DisableLogOutput()
                    .SetUpdateAssemblyInfo(UpdateAssemblyInfo)
                    .SetToolPath(ToolPath)
                ).Result;

            if (UpdateBuildNumber)
            {
                AzurePipelines.Instance?.UpdateBuildNumber(item.FullSemVer);
                TeamCity.Instance?.SetBuildNumber(item.FullSemVer);
                AppVeyor.Instance?.UpdateBuildNumber($"{item.FullSemVer}.build.{AppVeyor.Instance.BuildNumber}");
            }
            return item;
        }

        private string GetToolPath()
        {
            // Linux based system are case sensative. 
            // This class was extended in order to lowercase the .dll/.exe 
            // filename being resolved.
            // TODO: fix in Nuke.Common repo
            return ToolPathResolver.GetPackageExecutable(
                "GitVersion.Tool|GitVersion.CommandLine",
                "gitversion.dll|gitversion.exe", null, Framework);
        }
    }
}