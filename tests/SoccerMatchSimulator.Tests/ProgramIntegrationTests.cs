using System.Diagnostics;

namespace SoccerMatchSimulator.Tests;

/// <summary>
/// Integration tests for the CLI application.
/// These tests run the actual compiled executable and verify its behavior.
/// </summary>
public class ProgramIntegrationTests : IDisposable
{
    private readonly string _testOutputDir;

    public ProgramIntegrationTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"soccer-sim-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, recursive: true);
        }
    }

    private static (int exitCode, string output, string error) RunProgram(params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project src/SoccerMatchSimulator -- {string.Join(" ", args)}",
            WorkingDirectory = GetSolutionDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit(30000); // 30 second timeout

        return (process.ExitCode, output, error);
    }

    private static string GetSolutionDirectory()
    {
        // Navigate up from test bin directory to solution root
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "SoccerMatchSimulator.sln")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }
        return dir ?? throw new InvalidOperationException("Could not find solution directory");
    }

    [Fact]
    public void Program_WithHelpFlag_ShowsUsageAndReturnsSuccess()
    {
        var (exitCode, output, _) = RunProgram("--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Usage:", output);
        Assert.Contains("Interactive mode", output);
        Assert.Contains("goalsSeedTeamA", output);
    }

    [Fact]
    public void Program_WithShortHelpFlag_ShowsUsage()
    {
        var (exitCode, output, _) = RunProgram("-h");

        Assert.Equal(0, exitCode);
        Assert.Contains("Usage:", output);
    }

    [Fact]
    public void Program_WithValidArguments_ReturnsSuccess()
    {
        var (exitCode, output, error) = RunProgram("1.5", "1.2", "10");

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Contains("SOCCER MATCH SIMULATOR", output);
        Assert.Contains("SUMMARY", output);
    }

    [Fact]
    public void Program_WithInvalidGoalSeed_ReturnsErrorMessage()
    {
        var (exitCode, _, error) = RunProgram("invalid", "1.2", "10");

        Assert.Equal(1, exitCode);
        Assert.Contains("Error:", error);
    }

    [Fact]
    public void Program_WithNegativeGoalSeed_ReturnsErrorMessage()
    {
        var (exitCode, _, error) = RunProgram("-1.0", "1.2", "10");

        Assert.Equal(1, exitCode);
        Assert.Contains("Error:", error);
    }

    [Fact]
    public void Program_WithZeroSimulations_ReturnsErrorMessage()
    {
        var (exitCode, _, error) = RunProgram("1.5", "1.2", "0");

        Assert.Equal(1, exitCode);
        Assert.Contains("Error:", error);
    }

    [Fact]
    public void Program_OutputContainsCorrectSimulationCount()
    {
        var (exitCode, output, _) = RunProgram("1.5", "1.2", "5");

        Assert.Equal(0, exitCode);
        // Should have 5 result rows (look for row numbers 1-5)
        Assert.Contains("│      1 │", output);
        Assert.Contains("│      5 │", output);
        Assert.DoesNotContain("│      6 │", output);
    }

    [Fact]
    public void Program_OutputContainsSummaryStatistics()
    {
        var (exitCode, output, _) = RunProgram("1.5", "1.2", "100");

        Assert.Equal(0, exitCode);
        Assert.Contains("Team A Wins:", output);
        Assert.Contains("Draws:", output);
        Assert.Contains("Team B Wins:", output);
        Assert.Contains("Avg Goals Team A:", output);
        Assert.Contains("Avg Goals Team B:", output);
    }

    [Fact]
    public void Program_WithOutputFlag_WritesToResultsFolder()
    {
        var prefix = $"test_{Guid.NewGuid():N}";
        var solutionDir = GetSolutionDirectory();
        var resultsDir = Path.Combine(solutionDir, "results");
        string[] createdFiles = [];

        try
        {
            var (exitCode, consoleOutput, _) = RunProgram("1.5", "1.2", "10", "--output", prefix);

            Assert.Equal(0, exitCode);
            Assert.Contains("Results written to: results/", consoleOutput);
            Assert.Contains(prefix, consoleOutput);

            // Find the created file in results folder
            if (Directory.Exists(resultsDir))
            {
                createdFiles = Directory.GetFiles(resultsDir, $"{prefix}_*.txt");
                Assert.True(createdFiles.Length > 0, "Output file should exist in results folder");

                var fileContent = File.ReadAllText(createdFiles[0]);
                Assert.Contains("SOCCER MATCH SIMULATOR", fileContent);
                Assert.Contains("SUMMARY", fileContent);
            }
        }
        finally
        {
            // Cleanup: always delete test files even if assertions fail
            foreach (var file in createdFiles)
            {
                try { File.Delete(file); } catch { /* ignore cleanup errors */ }
            }
        }
    }
}
