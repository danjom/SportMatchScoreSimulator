using SoccerMatchSimulator.Simulation;

namespace SoccerMatchSimulator.Tests;

public class PoissonScoreGeneratorTests
{
    [Fact]
    public void Generate_WithZeroLambda_ReturnsZero()
    {
        var generator = new PoissonScoreGenerator(new Random(42));

        var result = generator.Generate(0.0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Generate_ReturnsNonNegativeValues()
    {
        var generator = new PoissonScoreGenerator(new Random(42));

        for (int i = 0; i < 1000; i++)
        {
            var result = generator.Generate(1.5);
            Assert.True(result >= 0, $"Generated negative value: {result}");
        }
    }

    [Fact]
    public void Generate_WithSameSeed_ProducesReproducibleResults()
    {
        var generator1 = new PoissonScoreGenerator(new Random(123));
        var generator2 = new PoissonScoreGenerator(new Random(123));

        var results1 = Enumerable.Range(0, 100).Select(_ => generator1.Generate(1.5)).ToList();
        var results2 = Enumerable.Range(0, 100).Select(_ => generator2.Generate(1.5)).ToList();

        Assert.Equal(results1, results2);
    }

    [Fact]
    public void Generate_AverageApproximatesLambda()
    {
        var generator = new PoissonScoreGenerator(new Random(42));
        double lambda = 2.5;

        var results = Enumerable.Range(0, 10_000).Select(_ => generator.Generate(lambda)).ToList();
        double average = results.Average();

        // Average should be within 5% of λ
        Assert.True(Math.Abs(average - lambda) < 0.15,
            $"Expected average ~{lambda}, got {average:F3}");
    }

    [Fact]
    public void Generate_WithSmallLambda_ProducesMostlyZeros()
    {
        var generator = new PoissonScoreGenerator(new Random(42));

        var results = Enumerable.Range(0, 1000).Select(_ => generator.Generate(0.01)).ToList();
        var zeros = results.Count(r => r == 0);

        // With λ = 0.01, P(0) ≈ 0.99
        Assert.True(zeros > 900, $"Expected mostly zeros, got {zeros}/1000");
    }

    [Fact]
    public void Constructor_WithNullRandom_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PoissonScoreGenerator(null!));
    }
}
