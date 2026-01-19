using SoccerMatchSimulator.Models;

namespace SoccerMatchSimulator.Statistics;

/// <summary>
/// Calculates aggregated statistics from simulation results.
/// </summary>
public static class StatisticsCalculator
{
    /// <summary>
    /// Calculates statistics from a collection of match results.
    /// </summary>
    public static SimulationStatistics Calculate(IReadOnlyList<MatchResult> results)
    {
        if (results == null)
            throw new ArgumentNullException(nameof(results));

        if (results.Count == 0)
            throw new ArgumentException("Results cannot be empty.", nameof(results));

        return new SimulationStatistics(
            TotalSimulations: results.Count,
            TeamAWins: results.Count(r => r.Spread > 0),
            Draws: results.Count(r => r.Spread == 0),
            TeamBWins: results.Count(r => r.Spread < 0),
            AvgGoalsTeamA: results.Average(r => r.GoalsTeamA),
            AvgGoalsTeamB: results.Average(r => r.GoalsTeamB),
            AvgSpread: results.Average(r => r.Spread),
            AvgTotalGoals: results.Average(r => r.TotalGoals));
    }
}
