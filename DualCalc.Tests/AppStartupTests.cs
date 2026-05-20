using System.Diagnostics;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Locator;
using Xunit;

namespace DualCalc.Tests;

public class AppStartupTests
{
    private const string Configuration = "Debug";
    private const string Platform = "x64";
    private static readonly string SolutionRoot = GetSolutionRoot();
    private static readonly string ProjectPath = Path.Combine(SolutionRoot, "DualCalc", "DualCalc.csproj");
    private static readonly string ExePath = Path.Combine(SolutionRoot, "DualCalc", "bin", Platform, Configuration, "net10.0-windows10.0.19041.0", "DualCalc.exe");

    [Fact]
    public void App_can_build_and_start()
    {
        EnsureMSBuildRegistered();
        BuildApplication();

        Assert.True(File.Exists(ExePath), $"找不到可執行檔: {ExePath}");

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = ExePath,
            WorkingDirectory = Path.GetDirectoryName(ExePath)!,
            UseShellExecute = false
        });

        Assert.NotNull(process);
        
        bool exited = process!.WaitForExit(5000);
        
        if (exited)
        {
            Assert.Fail($"應用程式在啟動 5 秒內結束，ExitCode={process.ExitCode}");
        }

        process.Kill(true);
        process.WaitForExit();
    }

    private static void BuildApplication()
    {
        var projectCollection = new ProjectCollection(new Dictionary<string, string>
        {
            ["Configuration"] = Configuration,
            ["Platform"] = Platform
        });

        var logger = new ConsoleLogger(LoggerVerbosity.Minimal);
        var buildParameters = new BuildParameters(projectCollection)
        {
            Loggers = new ILogger[] { logger }
        };

        var buildRequest = new BuildRequestData(
            ProjectPath,
            new Dictionary<string, string>
            {
                ["Configuration"] = Configuration,
                ["Platform"] = Platform
            },
            toolsVersion: null,
            targetsToBuild: ["Build"],
            hostServices: null);

        var result = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
        Assert.Equal(BuildResultCode.Success, result.OverallResult);
    }

    private static void EnsureMSBuildRegistered()
    {
        if (MSBuildLocator.IsRegistered)
        {
            return;
        }

        var instance = MSBuildLocator.QueryVisualStudioInstances()
            .OrderByDescending(i => i.Version)
            .First();

        MSBuildLocator.RegisterInstance(instance);
    }

    private static string GetSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DualCalc.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("找不到 DualCalc.sln");
    }
}
