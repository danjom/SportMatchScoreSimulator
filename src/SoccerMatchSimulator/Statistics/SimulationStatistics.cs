namespace SoccerMatchSimulator.Statistics;

/// <summary>
/// Aggregated statistics from simulation results.
/// </summary>
public record SimulationStatistics(
    int TotalSimulations,
    int TeamAWins,
    int Draws,
    int TeamBWins,
    double AvgGoalsTeamA,
    double AvgGoalsTeamB,
    double AvgSpread,
    double AvgTotalGoals)
{
    public double TeamAWinPercentage => TotalSimulations > 0 ? 100.0 * TeamAWins / TotalSimulations : 0;
    public double DrawPercentage => TotalSimulations > 0 ? 100.0 * Draws / TotalSimulations : 0;
    public double TeamBWinPercentage => TotalSimulations > 0 ? 100.0 * TeamBWins / TotalSimulations : 0;
}
