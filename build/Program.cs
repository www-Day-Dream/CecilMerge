using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Solution.Project;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Package.Add;
using Cake.Common.Tools.MSBuild;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.IO.Arguments;
using Cake.Frosting;
using Spectre.Console;
using Enumerable = System.Linq.Enumerable;

// ReSharper disable All

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .InstallTool(new Uri("dotnet:?package=tcli&version=0.2.3"))
            .UseContext<BuildContext>()
            .UseLifetime<BuildLifetime>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public DirectoryPath IntermediateOutput = new DirectoryPath("../bin");
    public bool InstallToBepInEx = false;
    public DirectoryPath BepInExPath = "";
    public IReadOnlyCollection<SolutionProject> Projects;
    
    public BuildContext(ICakeContext context)
        : base(context)
    {
    }
}

public class BuildLifetime : FrostingLifetime<BuildContext>
{
    public override void Setup(BuildContext context, ISetupContext info)
    {
        if (!context.DirectoryExists(context.IntermediateOutput))
            context.CreateDirectory(context.IntermediateOutput);
        
        AnsiConsole.MarkupLine("Cleaning intermediate output directory: " + context.IntermediateOutput);
        context.CleanDirectory(context.IntermediateOutput);

        context.BepInExPath =
            context.Argument("BepInEx", (string)null) ??
            context.Argument("BepInX", (string)null) ??
            context.Argument("BIX", (string)null) ??
            context.Argument("BIE", (string)null) ??
            context.Argument("InstallAt", "");
        context.InstallToBepInEx = context.BepInExPath.FullPath.Length > 0 && 
                                  context.DirectoryExists(context.BepInExPath) &&
                                  context.Argument("target", "") == "Install";
        
        if (!context.InstallToBepInEx && context.Argument("target", "") == "Install")
            AnsiConsole.MarkupLine("[yellow]No BepInExPath provided with --BepInEx, output will not be installed anywhere.[/]");

        context.Projects = context.ParseSolution(
            context.Argument("Solution", "../CecilMerge.sln"))?.Projects;
        if (context.Projects == null)
            throw new Exception("Can't continue, no solution!");
    }

    public override void Teardown(BuildContext context, ITeardownContext info)
    {
        var arg = context.Argument("Launch", string.Empty);
        
        if (arg != string.Empty && context.InstallToBepInEx && context.FileExists(arg))
            context.StartAndReturnProcess(arg, new ProcessSettings()
            {
                WorkingDirectory = FilePath.FromString(arg).GetDirectory(),
                Arguments = new ProcessArgumentBuilder()
                    .Append("--doorstop-enable true")
                    .Append("--doorstop-target " + context.BepInExPath.Combine("./core/BepInEx.Preloader.dll"))
            });
    }
}

[TaskName("Build")]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        AnsiConsole.MarkupLine("Building with DotNetMSBuild " + context.Argument("BepInExPath", ""));
        context.MSBuild(new FilePath("../"), new MSBuildSettings(){Configuration = "Release", Verbosity = Verbosity.Minimal});

        foreach (var project in context.Projects)
        {
            var dirToDll = project.Path.GetDirectory()
                .Combine("./bin/Release/");
            if (!context.DirectoryExists(dirToDll))
            {
                AnsiConsole.MarkupLine("[underlined red]Project '" + project.Name + "' lacks a /bin/Release/ folder![/]");
                continue;
            }

            foreach (var directory in context.GetSubDirectories(dirToDll))
            {
                var filePath = FilePath.FromString("./" + project.Name + ".dll");
                
                var toDir = context.IntermediateOutput
                    .Combine(DirectoryPath.FromString("./" + directory.GetDirectoryName()));
                if (!context.DirectoryExists(toDir))
                    context.CreateDirectory(toDir);
                
                var fileToCopy = directory.GetFilePath(filePath);
                if (context.FileExists(fileToCopy))
                    context.CopyFile(fileToCopy, toDir.GetFilePath(filePath));
            }
            
            context.DeleteDirectory(project.Path.GetDirectory().Combine("./bin"), new DeleteDirectorySettings(){Force = true, Recursive = true});
        }
        
        AnsiConsole.MarkupLine("Building with TCLI");
        var tcliPath = context.Tools.Resolve("tcli.exe");
        context.StartProcess(tcliPath,
            new ProcessSettings() { Arguments = 
                new ProcessArgumentBuilder()
                    .Append("build").Append("--config-path").Append("./thunderstore.toml") });
    }
}

[TaskName("Install")]
[IsDependentOn(typeof(BuildTask))]
public sealed class InstallTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.InstallToBepInEx;
    }

    public override void Run(BuildContext context)
    {
        AnsiConsole.MarkupLine("[blue]Installing to BepInExPath[/] [gray]'" + context.BepInExPath + "'[/]");
        
        AnsiConsole.MarkupLine("");
        var cecilMergeRuntime = FilePath.FromString("./CecilMerge.Runtime.dll");
        var cecilMergePreloader = FilePath.FromString("./CecilMerge.Preloader.dll");
        var testPluginA = FilePath.FromString("./TestPluginA.dll");
        var fromNet35 = context.IntermediateOutput.Combine("./net35");
        var fromNet21 = context.IntermediateOutput.Combine("./netstandard2.1");
        var toPluginDir = context.BepInExPath.Combine("./plugins/");
        var toPatcherDir = context.BepInExPath.Combine("./patchers/");
        
        if (!context.DirectoryExists(toPluginDir))
            context.CreateDirectory(toPluginDir);
        if (!context.DirectoryExists(toPatcherDir))
            context.CreateDirectory(toPatcherDir);

        var cecilMergeRuntimeFinal = toPatcherDir.GetFilePath(cecilMergeRuntime);
        var cecilMergePreloaderFinal = toPatcherDir.GetFilePath(cecilMergePreloader);
        var testPluginAFinal = toPluginDir.GetFilePath(testPluginA);
        
        AnsiConsole.MarkupLine("  " + cecilMergeRuntime + " -> " + 
                               cecilMergeRuntimeFinal);
        context.CopyFile(fromNet35.GetFilePath(cecilMergeRuntime), 
            cecilMergeRuntimeFinal);
        
        AnsiConsole.MarkupLine("  " + cecilMergePreloader + " -> " + 
                               cecilMergePreloaderFinal);
        context.CopyFile(fromNet35.GetFilePath(cecilMergePreloader), 
            cecilMergePreloaderFinal);
        
        AnsiConsole.MarkupLine("  " + testPluginA + " -> " + 
                               testPluginAFinal);
        context.CopyFile(fromNet21.GetFilePath(testPluginA), 
            testPluginAFinal);
    }
}