namespace SoccerMatchSimulator.Simulation;

/// <summary>
/// Generates Poisson-distributed scores using Knuth's algorithm.
/// Optimal for small λ values (λ &lt; 30), typical in soccer (0.5-3.0).
/// For large λ values, consider using rejection method (Ahrens-Dieter) instead.
/// </summary>
public class PoissonScoreGenerator : IScoreGenerator
{
    private readonly Random _random;

    public PoissonScoreGenerator(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <summary>
    /// Generates a Poisson-distributed score using Knuth's algorithm.
    /// </summary>
    /// <param name="lambda">The expected value (λ). Should be small (&lt;30) for optimal performance.</param>
    /// <returns>A non-negative integer following Poisson distribution.</returns>
    public int Generate(double lambda)
    {
        // Handle edge case: λ = 0 always produces 0
        if (lambda == 0)
            return 0;

        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;

        do
        {
            k++;
            p *= _random.NextDouble();
        } while (p > L);

        return k - 1;
    }
}
