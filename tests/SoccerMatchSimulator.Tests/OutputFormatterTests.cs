using SoccerMatchSimulator.Models;
using SoccerMatchSimulator.Output;
using SoccerMatchSimulator.Statistics;

namespace SoccerMatchSimulator.Tests;

public class OutputFormatterTests
{
    [Fact]
    public void Format_ContainsHeader()
    {
        var results = new List<MatchResult> { new(2, 1) };
        var stats = new SimulationStatistics(1, 1, 0, 0, 2.0, 1.0, 1.0, 3.0);

        var output = OutputFormatter.Format(results, stats, 1.5, 1.2);

        Assert.Contains("SOCCER MATCH SIMULATOR", output);
        Assert.Contains("Team A Seed: 1.50", output);
        Assert.Contains("Team B Seed: 1.20", output);
    }

    [Fact]
    public void Format_ContainsResultsTable()
    {
        var results = new List<MatchResult>
        {
            new(2, 1),
            new(1, 0),
        };
        var stats = new SimulationStatistics(2, 2, 0, 0, 1.5, 0.5, 1.0, 2.0);

        var output = OutputFormatter.Format(results, stats, 1.5, 1.2);

        Assert.Contains("Sim #", output);
        Assert.Contains("Team A", output);
        Assert.Contains("Team B", output);
        Assert.Contains("Spread", output);
        Assert.Contains("Total", output);
    }

    [Fact]
    public void Format_ContainsSummarySection()
    {
        var results = new List<MatchResult> { new(2, 1) };
        var stats = new SimulationStatistics(1, 1, 0, 0, 2.0, 1.0, 1.0, 3.0);

        var output = OutputFormatter.Format(results, stats, 1.5, 1.2);

        Assert.Contains("SUMMARY", output);
        Assert.Contains("Team A Wins:", output);
        Assert.Contains("Draws:", output);
        Assert.Contains("Team B Wins:", output);
        Assert.Contains("Avg Goals Team A:", output);
    }

    [Fact]
    public void Format_ShowsCorrectSpreadSign()
    {
        var results = new List<MatchResult>
        {
            new(2, 1),  // +1
            new(1, 3),  // -2
        };
        var stats = new SimulationStatistics(2, 1, 0, 1, 1.5, 2.0, -0.5, 3.5);

        var output = OutputFormatter.Format(results, stats, 1.5, 1.2);

        Assert.Contains("+1", output);
        Assert.Contains("-2", output);
    }

    [Fact]
    public void Format_ShowsSimulationNumbers()
    {
        var results = new List<MatchResult>
        {
            new(2, 1),
            new(1, 0),
            new(3, 2),
        };
        var stats = new SimulationStatistics(3, 3, 0, 0, 2.0, 1.0, 1.0, 3.0);

        var output = OutputFormatter.Format(results, stats, 1.5, 1.2);

        // Should show simulation numbers 1, 2, 3
        Assert.Contains("│      1 │", output);
        Assert.Contains("│      2 │", output);
        Assert.Contains("│      3 │", output);
    }
}
