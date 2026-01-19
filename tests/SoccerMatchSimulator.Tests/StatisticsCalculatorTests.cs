using SoccerMatchSimulator.Models;
using SoccerMatchSimulator.Statistics;

namespace SoccerMatchSimulator.Tests;

public class StatisticsCalculatorTests
{
    [Fact]
    public void Calculate_WithValidResults_ReturnsCorrectCounts()
    {
        var results = new List<MatchResult>
        {
            new(2, 1),  // Team A win
            new(1, 1),  // Draw
            new(0, 2),  // Team B win
            new(3, 0),  // Team A win
            new(1, 2),  // Team B win
        };

        var stats = StatisticsCalculator.Calculate(results);

        Assert.Equal(5, stats.TotalSimulations);
        Assert.Equal(2, stats.TeamAWins);
        Assert.Equal(1, stats.Draws);
        Assert.Equal(2, stats.TeamBWins);
    }

    [Fact]
    public void Calculate_ReturnsCorrectPercentages()
    {
        var results = new List<MatchResult>
        {
            new(2, 1),  // Team A win
            new(1, 1),  // Draw
            new(0, 2),  // Team B win
            new(3, 0),  // Team A win
        };

        var stats = StatisticsCalculator.Calculate(results);

        Assert.Equal(50.0, stats.TeamAWinPercentage);
        Assert.Equal(25.0, stats.DrawPercentage);
        Assert.Equal(25.0, stats.TeamBWinPercentage);
    }

    [Fact]
    public void Calculate_ReturnsCorrectAverages()
    {
        var results = new List<MatchResult>
        {
            new(2, 1),  // Spread +1, Total 3
            new(3, 1),  // Spread +2, Total 4
            new(1, 2),  // Spread -1, Total 3
        };

        var stats = StatisticsCalculator.Calculate(results);

        Assert.Equal(2.0, stats.AvgGoalsTeamA);
        Assert.Equal(4.0 / 3.0, stats.AvgGoalsTeamB, 5);
        Assert.Equal(2.0 / 3.0, stats.AvgSpread, 5);
        Assert.Equal(10.0 / 3.0, stats.AvgTotalGoals, 5);
    }

    [Fact]
    public void Calculate_WithNullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => StatisticsCalculator.Calculate(null!));
    }

    [Fact]
    public void Calculate_WithEmptyResults_ThrowsArgumentException()
    {
        var emptyResults = new List<MatchResult>();

        Assert.Throws<ArgumentException>(() => StatisticsCalculator.Calculate(emptyResults));
    }

    [Fact]
    public void Calculate_WithSingleResult_ReturnsCorrectStats()
    {
        var results = new List<MatchResult> { new(2, 1) };

        var stats = StatisticsCalculator.Calculate(results);

        Assert.Equal(1, stats.TotalSimulations);
        Assert.Equal(1, stats.TeamAWins);
        Assert.Equal(0, stats.Draws);
        Assert.Equal(0, stats.TeamBWins);
        Assert.Equal(100.0, stats.TeamAWinPercentage);
    }

    [Fact]
    public void Calculate_WithAllDraws_ReturnsCorrectStats()
    {
        var results = new List<MatchResult>
        {
            new(0, 0),
            new(1, 1),
            new(2, 2),
        };

        var stats = StatisticsCalculator.Calculate(results);

        Assert.Equal(0, stats.TeamAWins);
        Assert.Equal(3, stats.Draws);
        Assert.Equal(0, stats.TeamBWins);
        Assert.Equal(100.0, stats.DrawPercentage);
    }
}
