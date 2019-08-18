
var target = Argument("target", "Default");
var artifactsDir = "./artifacts"; 
var testResultDir = artifactsDir + "/test-results";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./src/**/obj");
	CleanDirectories("./src/**/bin");
	CleanDirectories("./tests/**/bin");
	CleanDirectories("./tests/**/obj");
	CleanDirectories(artifactsDir);
    CleanDirectories(testResultDir);
});

Task("Prepare")
 .IsDependentOn("Clean")
.Does(()=> 
{
    CreateDirectory(artifactsDir);
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
    var projects = GetFiles("./**/*.csproj");
    foreach(var project in projects)
    {
        DotNetCoreBuild(
            project.GetDirectory().FullPath, 
            new DotNetCoreBuildSettings {
                Configuration = "Release"
            });
    }
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
                            MakeAbsolute(Directory(artifactsDir)).FullPath, 
                            project.GetFilenameWithoutExtension().FullPath + ".trx"))
        });
    }
    
});


Task("Publish")
    .IsDependentOn("Test")
    .Does(() => {
        DotNetCorePack("./src/dng.Slugify", new DotNetCorePackSettings
        {
            Configuration = "Release",
            OutputDirectory = artifactsDir,
            NoBuild = true
        });
    });

Task("Push").IsDependentOn("Publish").Does(()=> {

    var nugetServer = EnvironmentVariable("nuget-server") ?? "";
    var nugetApiKey = EnvironmentVariable("nuget-apikey") ?? "";
    if (string.IsNullOrEmpty(nugetServer))
    {
        Console.Write("Nuget-Server not definied." + System.Environment.NewLine);
        return;
    }

    var packages = GetFiles("./artifacts/*.nupkg");
    foreach(var package in packages)
    {
        Console.Write(package);
        NuGetPush(package, new NuGetPushSettings {
            Source = nugetServer,
            ApiKey = nugetApiKey
        });
    }
});

Task("Default").IsDependentOn("Test");

Task("PushPackage").IsDependentOn("Push");

RunTarget(target);