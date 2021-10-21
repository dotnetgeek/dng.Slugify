#addin "Cake.Figlet&version=2.0.1"
#addin nuget:?package=Cake.MinVer&version=1.0.1
#addin nuget:?package=Cake.Coverlet&version=2.5.4

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var publishDir = Directory (Argument("publishDir", EnvironmentVariable("BUILD_PUBLISH") ?? "./publish"));; 
var publishNuPkgDir = Directory(publishDir) + Directory("nupkg");
var publishTestReportsDir = Directory(publishDir) + Directory("test-results");

DotNetCoreBuildSettings dotNetCoreBuildSettings; 
DotNetCoreMSBuildSettings msBuildSettings;

Setup(context =>
{
    var version = MinVer(settings => settings.WithMinimumMajorMinor("1.0"));

    Information(Figlet("dng.Slugify"));
    Information($"Configuration: {configuration}");
    Information($"Version: {version.Version}");

    msBuildSettings = new DotNetCoreMSBuildSettings()
        .SetFileVersion(version.FileVersion)
        .SetInformationalVersion(version.AssemblyVersion.ToString())
        .SetVersion(version.Version.ToString())
            .WithProperty("PackageVersion", version.Version.ToString());

    dotNetCoreBuildSettings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = msBuildSettings
    };
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./src/**/obj");
	CleanDirectories("./src/**/bin");
	CleanDirectories("./tests/**/bin");
	CleanDirectories("./tests/**/obj");
	CleanDirectory(publishDir);
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreRestore("./",new DotNetCoreRestoreSettings
    {
        Sources = new [] {
            "https://api.nuget.org/v3/index.json"
        }
    });
});

Task("Build")
    .Does(() =>
{
    DotNetCoreBuild("./dng.Slugify.sln", dotNetCoreBuildSettings);
});

Task("Test")
    .Does(() =>
{
    var coverletSettings = new CoverletSettings {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.opencover | CoverletOutputFormat.cobertura,
        CoverletOutputDirectory = publishTestReportsDir, //MakeAbsolute(publishTestReportsDir).FullPath,
        CoverletOutputName = $"codecover"
    };

    var projects = GetFiles("./tests/**/*.Tests.csproj");
    foreach(var project in projects)
    {
        var testSettings = new DotNetCoreTestSettings  {
                ArgumentCustomization = args =>
                        args.Append("--logger ")
                        .Append("trx;LogFileName=" +
                            System.IO.Path.Combine(
                                MakeAbsolute(Directory(publishTestReportsDir)).FullPath,
                                project.GetFilenameWithoutExtension().FullPath + ".trx"))
        };

        DotNetCoreTest (project.ToString(), testSettings, coverletSettings);
    }
});


Task("Create-NuGet-Package")
    .Does(() => 
{
    Information("Publish Directory: {0}", MakeAbsolute(publishDir));
  //  var publishDirBuild = Directory(publishDir) + Directory("build");
    DotNetCorePack("./src/dng.Slugify/dng.Slugify.csproj", new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = publishNuPkgDir,
        MSBuildSettings = msBuildSettings
    });

    Information("NuGet Package created.");
});

Task("Push-To-NuGet")
	.Does(()=> {

    var nugetServer = EnvironmentVariable("nuget-server") ?? "";
    var nugetApiKey = EnvironmentVariable("nuget-apikey") ?? "";

    if (string.IsNullOrEmpty(nugetServer))
    {
        Error("Nuget-Server not definied.");
        return;
    }

    var packages = GetFiles($"{publishDir}/**/*.nupkg");
    foreach(var package in packages)
    {
        Information($"NuGet Package {package} found to push");
        NuGetPush(package, new NuGetPushSettings {
            Source = nugetServer,
            ApiKey = nugetApiKey
        });
    }
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build");

Task("build-and-test")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Create-NuGet-Package");

Task("Publish")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Create-NuGet-Package")
    .IsDependentOn("Push-To-NuGet");

RunTarget(target);