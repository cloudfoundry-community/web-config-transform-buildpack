#addin nuget:?package=SharpZipLib&version=1.1.0
#addin nuget:?package=Cake.Compression&version=0.2.2
var name = Argument("name","web_config_transform_buildpack");
var target = Argument("target", "Default");
var stack = Argument("stack","windows");
var vMajor = Argument("vmajor","1");
var vMinor = Argument("vminor","0");
var vBuild = (int)(DateTime.UtcNow - new DateTime(2000, 1, 1)).TotalSeconds;
var version = $"{vMajor}.{vMinor}.{vBuild}";

var framework = "net47";
var runtime = "win-x64";
if(stack.ToLower().Contains("linux"))
{
  framework = "netcoreapp2.0";
  runtime = "linux-x64";
}

var publishFolder = "publish";
var publishFolderBin = System.IO.Path.Combine(publishFolder, "bin");
Task("Default")
  .IsDependentOn("Package")
  .Does(() =>
{
  Information("Complete!");
  
});
Task("Package")
  .IsDependentOn("Clean")
  .IsDependentOn("CopyHooks")
  .IsDependentOn("Publish")
  .Does(() => 
{
  Information("Finalizing buildpack package");
  ZipCompress(publishFolder,System.IO.Path.Combine(publishFolder, $"{name}-{runtime}-{version}.zip"));
  System.IO.Directory.Delete(publishFolderBin,true);
});

Task("Publish")
  .IsDependentOn("Clean")
  .Does(() => 
{
    var settings = new DotNetCorePublishSettings
     {
         Framework = framework,
         Configuration = "Release",
         OutputDirectory = publishFolderBin,
         Runtime = runtime
     };
  DotNetCorePublish("src/web-config-transform-buildpack.sln", settings);
});

Task("Clean")
  .Does(() => 
{
  if(System.IO.Directory.Exists(publishFolder))
      System.IO.Directory.Delete(publishFolder,true);
  System.IO.Directory.CreateDirectory(publishFolderBin);
});

Task("CopyHooks")
  .IsDependentOn("Clean")
  .Does(() => 
{
  foreach(var file in System.IO.Directory.EnumerateFiles("scripts"))
  {
    Information(file);
    var target = System.IO.Path.Combine(publishFolderBin, System.IO.Path.GetFileName(file));
    System.IO.File.Copy(file, target);
  }
});

RunTarget(target);