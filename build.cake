#addin "Cake.Figlet&version=2.0.1"
#addin nuget:?package=Cake.MinVer&version=1.0.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var publishDir = Directory (Argument("publishDir", EnvironmentVariable("BUILD_PUBLISH") ?? "./publish"));; 
var testResultDir = Directory(publishDir) + Directory("test-results");


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
        .SetVersion(version.Version.ToString());

    dotNetCoreBuildSettings = new DotNetCoreBuildSettings
    {
        NoRestore = true,
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

Task("Prepare")
 .IsDependentOn("Clean")
.Does(()=> 
{
    //CreateDirectory(artifactsDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Prepare")
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
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
DotNetCoreBuild("./dng.Slugify.sln", dotNetCoreBuildSettings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projects = GetFiles("./tests/**/*.Tests.csproj");
    foreach(var project in projects)
    {
        Console.Write(project);
        DotNetCoreTest (project.ToString(), new DotNetCoreTestSettings  {
            ArgumentCustomization = args =>
                    args.Append("--logger ")
                    .Append("trx;LogFileName=" +
                        System.IO.Path.Combine(
                            MakeAbsolute(publishDir).FullPath, 
                            project.GetFilenameWithoutExtension().FullPath + ".trx"))
        });
    }
    
});


Task("Publish")
    .IsDependentOn("Test")
    .Does(() => 
{
    Information("Publish Directory: {0}", MakeAbsolute(publishDir));
    var publishDirBuild = Directory(publishDir) + Directory("build");
    DotNetCorePack("./src/dng.Slugify", new DotNetCorePackSettings
    {
        NoBuild = false,
        NoDependencies = true,
        Configuration = configuration,
        OutputDirectory = MakeAbsolute(publishDirBuild)

    });
});

Task("Push")
	.IsDependentOn("Publish")
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

Task("Default").IsDependentOn("Test");

Task("PushPackage").IsDependentOn("Push");

RunTarget(target);