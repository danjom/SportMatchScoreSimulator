using SoccerMatchSimulator.Models;

namespace SoccerMatchSimulator.Simulation;

/// <summary>
/// Monte Carlo simulator for soccer matches using Poisson distribution.
/// </summary>
public class MonteCarloSimulator
{
    private readonly IScoreGenerator _scoreGenerator;
    private readonly Func<IScoreGenerator> _generatorFactory;

    // Validation constants
    private const double MinGoalSeed = 0.0;
    private const double MaxGoalSeed = 20.0;
    private const int MinSimulations = 1;
    private const int MaxSimulations = 1_000_000;
    private const int ParallelThreshold = 10_000;

    /// <summary>
    /// Creates a simulator with default Poisson score generator.
    /// </summary>
    public MonteCarloSimulator()
        : this(() => new PoissonScoreGenerator(new Random()))
    {
    }

    /// <summary>
    /// Creates a simulator with a specific seed for reproducibility.
    /// </summary>
    public MonteCarloSimulator(int seed)
        : this(() => new PoissonScoreGenerator(new Random(seed)))
    {
    }

    /// <summary>
    /// Creates a simulator with a custom score generator factory.
    /// </summary>
    /// <param name="generatorFactory">Factory function for creating score generators.</param>
    public MonteCarloSimulator(Func<IScoreGenerator> generatorFactory)
    {
        _generatorFactory = generatorFactory ?? throw new ArgumentNullException(nameof(generatorFactory));
        _scoreGenerator = _generatorFactory();
    }

    /// <summary>
    /// Runs simulations of soccer matches.
    /// Uses parallel execution for large counts (≥10,000).
    /// </summary>
    public IReadOnlyList<MatchResult> RunSimulations(double goalsSeedTeamA, double goalsSeedTeamB, int count)
    {
        ValidateGoalSeed(goalsSeedTeamA, nameof(goalsSeedTeamA));
        ValidateGoalSeed(goalsSeedTeamB, nameof(goalsSeedTeamB));
        ValidateSimulationCount(count);

        if (count >= ParallelThreshold)
        {
            return RunSimulationsParallel(goalsSeedTeamA, goalsSeedTeamB, count);
        }

        return Enumerable.Range(0, count)
            .Select(_ => SimulateMatch(goalsSeedTeamA, goalsSeedTeamB))
            .ToList();
    }

    private IReadOnlyList<MatchResult> RunSimulationsParallel(double goalsSeedTeamA, double goalsSeedTeamB, int count)
    {
        var results = new MatchResult[count];

        Parallel.For(0, count, _generatorFactory, (i, state, localGenerator) =>
        {
            int goalsA = localGenerator.Generate(goalsSeedTeamA);
            int goalsB = localGenerator.Generate(goalsSeedTeamB);
            results[i] = new MatchResult(goalsA, goalsB);
            return localGenerator;
        }, _ => { });

        return results;
    }

    private MatchResult SimulateMatch(double goalsSeedTeamA, double goalsSeedTeamB)
    {
        int goalsA = _scoreGenerator.Generate(goalsSeedTeamA);
        int goalsB = _scoreGenerator.Generate(goalsSeedTeamB);
        return new MatchResult(goalsA, goalsB);
    }

    private static void ValidateGoalSeed(double seed, string paramName)
    {
        if (double.IsNaN(seed) || double.IsInfinity(seed))
            throw new ArgumentOutOfRangeException(paramName, seed, "Goal seed must be a valid number.");

        if (seed < MinGoalSeed || seed > MaxGoalSeed)
            throw new ArgumentOutOfRangeException(paramName, seed, $"Goal seed must be between {MinGoalSeed} and {MaxGoalSeed}.");
    }

    private static void ValidateSimulationCount(int count)
    {
        if (count < MinSimulations || count > MaxSimulations)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Simulation count must be between {MinSimulations} and {MaxSimulations:N0}.");
    }
}
