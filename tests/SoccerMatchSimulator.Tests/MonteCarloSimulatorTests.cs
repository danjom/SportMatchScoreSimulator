using SoccerMatchSimulator.Simulation;

namespace SoccerMatchSimulator.Tests;

public class MonteCarloSimulatorTests
{
    [Fact]
    public void RunSimulations_ReturnsNonNegativeGoals()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        var result = simulator.RunSimulations(1.5, 1.2, 1).First();

        Assert.True(result.GoalsTeamA >= 0);
        Assert.True(result.GoalsTeamB >= 0);
    }

    [Fact]
    public void RunSimulations_WithSameSeed_ProducesReproducibleResults()
    {
        var simulator1 = new MonteCarloSimulator(seed: 123);
        var simulator2 = new MonteCarloSimulator(seed: 123);

        var result1 = simulator1.RunSimulations(1.8, 1.3, 1).First();
        var result2 = simulator2.RunSimulations(1.8, 1.3, 1).First();

        Assert.Equal(result1.GoalsTeamA, result2.GoalsTeamA);
        Assert.Equal(result1.GoalsTeamB, result2.GoalsTeamB);
    }

    [Fact]
    public void RunSimulations_SpreadIsCorrect()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        var result = simulator.RunSimulations(1.5, 1.2, 1).First();

        Assert.Equal(result.GoalsTeamA - result.GoalsTeamB, result.Spread);
    }

    [Fact]
    public void RunSimulations_TotalGoalsIsCorrect()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        var result = simulator.RunSimulations(1.5, 1.2, 1).First();

        Assert.Equal(result.GoalsTeamA + result.GoalsTeamB, result.TotalGoals);
    }

    [Fact]
    public void RunSimulations_WithZeroSeed_ReturnsZeroGoals()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        var result = simulator.RunSimulations(0.0, 0.0, 1).First();

        Assert.Equal(0, result.GoalsTeamA);
        Assert.Equal(0, result.GoalsTeamB);
    }

    [Fact]
    public void RunSimulations_ReturnsCorrectCount()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        var results = simulator.RunSimulations(1.5, 1.2, 1000);

        Assert.Equal(1000, results.Count);
    }

    [Fact]
    public void RunSimulations_ProducesVariedResults()
    {
        var simulator = new MonteCarloSimulator();

        var results = simulator.RunSimulations(1.5, 1.5, 1000);

        // With 1000 simulations, we should see variety in scores
        var uniqueScores = results.Select(r => (r.GoalsTeamA, r.GoalsTeamB)).Distinct().Count();
        Assert.True(uniqueScores > 5, $"Expected variety, got only {uniqueScores} unique scores");
    }

    [Fact]
    public void RunSimulations_HigherSeedProducesMoreGoalsOnAverage()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        // Team A has much higher expected goals
        var results = simulator.RunSimulations(3.0, 0.5, 10_000);
        var avgGoalsA = results.Average(r => r.GoalsTeamA);
        var avgGoalsB = results.Average(r => r.GoalsTeamB);

        Assert.True(avgGoalsA > avgGoalsB,
            $"Team A avg ({avgGoalsA:F2}) should be higher than Team B avg ({avgGoalsB:F2})");
    }

    #region Edge Cases

    [Fact]
    public void RunSimulations_WithMaxGoalSeed_Succeeds()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        // Should not throw at boundary (λ = 20.0)
        var results = simulator.RunSimulations(20.0, 20.0, 10);

        Assert.Equal(10, results.Count);
        Assert.All(results, r => Assert.True(r.GoalsTeamA >= 0));
    }

    [Fact]
    public void RunSimulations_WithVerySmallLambda_ProducesMainlyZeros()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        // Very small λ should produce mostly 0 goals
        var results = simulator.RunSimulations(0.01, 0.01, 1000);
        var zeroGoalMatches = results.Count(r => r.GoalsTeamA == 0 && r.GoalsTeamB == 0);

        // With λ = 0.01, P(0) ≈ e^(-0.01) ≈ 0.99, so most should be 0-0
        Assert.True(zeroGoalMatches > 900, $"Expected mostly 0-0 matches, got {zeroGoalMatches}/1000");
    }

    [Fact]
    public void RunSimulations_WithSingleSimulation_ReturnsOneResult()
    {
        var simulator = new MonteCarloSimulator(seed: 42);

        var results = simulator.RunSimulations(1.5, 1.2, 1);

        Assert.Single(results);
    }

    [Fact]
    public void RunSimulations_AverageApproximatesLambda()
    {
        var simulator = new MonteCarloSimulator(seed: 42);
        double expectedLambda = 2.5;

        var results = simulator.RunSimulations(expectedLambda, expectedLambda, 10_000);
        double avgGoalsA = results.Average(r => r.GoalsTeamA);
        double avgGoalsB = results.Average(r => r.GoalsTeamB);

        // Average should be within 5% of λ with 10,000 samples
        Assert.True(Math.Abs(avgGoalsA - expectedLambda) < 0.15,
            $"Expected avg ~{expectedLambda}, got {avgGoalsA:F3}");
        Assert.True(Math.Abs(avgGoalsB - expectedLambda) < 0.15,
            $"Expected avg ~{expectedLambda}, got {avgGoalsB:F3}");
    }

    #endregion

    #region Error Handling Tests

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.1)]
    public void RunSimulations_WithNegativeGoalSeedTeamA_ThrowsArgumentOutOfRangeException(double invalidSeed)
    {
        var simulator = new MonteCarloSimulator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            simulator.RunSimulations(invalidSeed, 1.5, 1));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.1)]
    public void RunSimulations_WithNegativeGoalSeedTeamB_ThrowsArgumentOutOfRangeException(double invalidSeed)
    {
        var simulator = new MonteCarloSimulator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            simulator.RunSimulations(1.5, invalidSeed, 1));
    }

    [Fact]
    public void RunSimulations_WithNaN_ThrowsArgumentOutOfRangeException()
    {
        var simulator = new MonteCarloSimulator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            simulator.RunSimulations(double.NaN, 1.5, 1));
    }

    [Fact]
    public void RunSimulations_WithInfinity_ThrowsArgumentOutOfRangeException()
    {
        var simulator = new MonteCarloSimulator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            simulator.RunSimulations(double.PositiveInfinity, 1.5, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void RunSimulations_WithInvalidCount_ThrowsArgumentOutOfRangeException(int invalidCount)
    {
        var simulator = new MonteCarloSimulator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            simulator.RunSimulations(1.5, 1.2, invalidCount));
    }

    [Fact]
    public void RunSimulations_WithCountExceedingMax_ThrowsArgumentOutOfRangeException()
    {
        var simulator = new MonteCarloSimulator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            simulator.RunSimulations(1.5, 1.2, 1_000_001));
    }

    #endregion
}
