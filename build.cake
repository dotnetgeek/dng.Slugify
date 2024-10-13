#addin nuget:?package=Cake.MinVer&version= 4.0.0
#addin nuget:?package=Cake.Coverlet&version=4.0.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var publishDir = Directory (Argument("publishDir", EnvironmentVariable("BUILD_PUBLISH") ?? "./publish"));; 
var publishNuPkgDir = Directory(publishDir) + Directory("nupkg");
var publishTestReportsDir = Directory(publishDir) + Directory("test-results");
var solutionPath = "./dng.Slugify.sln";

DotNetBuildSettings dotNetbuildSettings; 
DotNetMSBuildSettings msBuildSettings;

Setup(context =>
{
    var version = MinVer(settings => 
    settings.WithMinimumMajorMinor("1.0")
            .WithTagPrefix("v")
            .WithVerbosity(MinVerVerbosity.Trace)
            .WithDefaultPreReleasePhase("preview"));

    Information("dng.Slugify");
    Information($"Configuration: {configuration}");
    Information($"Version: {version.Version}");
    Information($"FileVersion: {version.FileVersion}");
    Information($"AssemblyVersion: {version.AssemblyVersion}");
    Information($"Major: {version.Major}");
    Information($"Minor: {version.Minor}");
    Information($"Patch: {version.Patch}");
    

    msBuildSettings = new DotNetMSBuildSettings()
        .SetFileVersion(version.FileVersion)
        .SetInformationalVersion(version.AssemblyVersion.ToString())
        .SetVersion(version.Version.ToString())
        .WithProperty("PackageVersion", version.PackageVersion.ToString());

    dotNetbuildSettings = new DotNetBuildSettings
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

    var settings = new DotNetCleanSettings
    {
        Configuration = configuration
    };

    DotNetClean(solutionPath, settings);
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{

    DotNetRestore(solutionPath, new DotNetRestoreSettings
    {        
        Sources = new [] {
            "https://api.nuget.org/v3/index.json"
        }
    });
});

Task("Build")
    .Does(() =>
{
    DotNetBuild(solutionPath, dotNetbuildSettings);
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
        var testSettings = new DotNetTestSettings  {
                ArgumentCustomization = args =>
                        args.Append("--logger ")
                        .Append("trx;LogFileName=" +
                            System.IO.Path.Combine(
                                MakeAbsolute(Directory(publishTestReportsDir)).FullPath,
                                project.GetFilenameWithoutExtension().FullPath + ".trx"))
        };

        DotNetTest (project.ToString(), testSettings, coverletSettings);
    }
});


Task("Create-NuGet-Package")
    .Does(() => 
{
    Information("Publish Directory: {0}", MakeAbsolute(publishDir));

    DotNetPack("./src/dng.Slugify/dng.Slugify.csproj", new DotNetPackSettings
    {
        Configuration = configuration,
        OutputDirectory = publishNuPkgDir,
        MSBuildSettings = msBuildSettings
    });

    Information("NuGet Package created.");
});

Task("Push-To-NuGet")
	.Does(()=> {

    var nugetServer = EnvironmentVariable("nuget_server") ?? "";
    var nugetApiKey = EnvironmentVariable("nuget_apikey") ?? "";

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