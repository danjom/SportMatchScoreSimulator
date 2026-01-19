namespace SoccerMatchSimulator.Models;

/// <summary>
/// Represents the result of a single simulated 90-minute soccer match.
/// </summary>
public record MatchResult(int GoalsTeamA, int GoalsTeamB)
{
    /// <summary>
    /// The match spread (Team A goals - Team B goals).
    /// </summary>
    public int Spread => GoalsTeamA - GoalsTeamB;

    /// <summary>
    /// The match total goals.
    /// </summary>
    public int TotalGoals => GoalsTeamA + GoalsTeamB;
}
