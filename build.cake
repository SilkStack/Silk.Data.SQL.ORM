var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");

var buildDir = Directory("./Silk.Data.SQL.ORM/bin") + Directory(configuration) +
	Directory("netstandard1.3");
var artifactDir = Directory("./artifacts") + Directory(configuration);

var mainProject = "./Silk.Data.SQL.ORM/Silk.Data.SQL.ORM.csproj";
var testProject = "./Silk.Data.SQL.ORM.Tests/Silk.Data.SQL.ORM.Tests.csproj";

Task("Clean")
	.Does(() =>
	{
		CleanDirectory(artifactDir);
		CleanDirectory(buildDir);
	});
	
Task("Build")
	.Does(() =>
	{
		var settings = new DotNetCoreBuildSettings
		{
			Configuration = configuration,
			OutputDirectory = buildDir
		};

		DotNetCoreBuild(mainProject, settings);
	});

Task("Run-Tests")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var settings = new DotNetCoreTestSettings
		{
			Configuration = configuration
		};

		DotNetCoreTest(testProject, settings);
	});

Task("Pack")
	.IsDependentOn("Run-Tests")
	.Does(() =>
	{
		var settings = new DotNetCorePackSettings
		{
			Configuration = configuration,
			OutputDirectory = artifactDir,
			NoBuild = true
		};

		DotNetCorePack(mainProject, settings);
	});

RunTarget(target);