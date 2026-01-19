using System.Text;
using SoccerMatchSimulator.Models;
using SoccerMatchSimulator.Statistics;

namespace SoccerMatchSimulator.Output;

/// <summary>
/// Formats simulation results as ASCII/Unicode tables for console output.
/// </summary>
public static class OutputFormatter
{
    /// <summary>
    /// Formats simulation results and statistics into console-friendly tables.
    /// </summary>
    public static string Format(
        IReadOnlyList<MatchResult> results,
        SimulationStatistics statistics,
        double goalsSeedTeamA,
        double goalsSeedTeamB)
    {
        var output = new StringBuilder();

        AppendHeader(output, goalsSeedTeamA, goalsSeedTeamB, statistics.TotalSimulations);
        AppendResultsTable(output, results);
        AppendSummary(output, statistics);

        return output.ToString();
    }

    private static void AppendHeader(StringBuilder output, double seedA, double seedB, int count)
    {
        output.AppendLine();
        output.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        output.AppendLine("║           SOCCER MATCH SIMULATOR - MONTE CARLO               ║");
        output.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        output.AppendLine($"║  Team A Seed: {seedA,-8:F2}  Team B Seed: {seedB,-8:F2}            ║");
        output.AppendLine($"║  Simulations: {count,-8}                                    ║");
        output.AppendLine("╚══════════════════════════════════════════════════════════════╝");
        output.AppendLine();
    }

    private static void AppendResultsTable(StringBuilder output, IReadOnlyList<MatchResult> results)
    {
        output.AppendLine("┌────────┬────────────┬────────────┬────────┬───────────┐");
        output.AppendLine("│  Sim # │  Team A    │  Team B    │ Spread │   Total   │");
        output.AppendLine("├────────┼────────────┼────────────┼────────┼───────────┤");

        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            string spreadStr = r.Spread >= 0 ? $"+{r.Spread}" : $"{r.Spread}";
            output.AppendLine($"│ {i + 1,6} │ {r.GoalsTeamA,10} │ {r.GoalsTeamB,10} │ {spreadStr,6} │ {r.TotalGoals,9} │");
        }

        output.AppendLine("└────────┴────────────┴────────────┴────────┴───────────┘");
        output.AppendLine();
    }

    private static void AppendSummary(StringBuilder output, SimulationStatistics stats)
    {
        output.AppendLine("┌──────────────────────────────────────────────────────────────┐");
        output.AppendLine("│                         SUMMARY                              │");
        output.AppendLine("├──────────────────────────────────────────────────────────────┤");
        output.AppendLine($"│  Team A Wins: {stats.TeamAWins,6}  ({stats.TeamAWinPercentage,5:F1}%)                          │");
        output.AppendLine($"│  Draws:       {stats.Draws,6}  ({stats.DrawPercentage,5:F1}%)                          │");
        output.AppendLine($"│  Team B Wins: {stats.TeamBWins,6}  ({stats.TeamBWinPercentage,5:F1}%)                          │");
        output.AppendLine("├──────────────────────────────────────────────────────────────┤");
        output.AppendLine($"│  Avg Goals Team A: {stats.AvgGoalsTeamA,5:F2}                                   │");
        output.AppendLine($"│  Avg Goals Team B: {stats.AvgGoalsTeamB,5:F2}                                   │");
        output.AppendLine($"│  Avg Spread:       {(stats.AvgSpread >= 0 ? "+" : "")}{stats.AvgSpread:F2}                                   │");
        output.AppendLine($"│  Avg Total Goals:  {stats.AvgTotalGoals,5:F2}                                   │");
        output.AppendLine("└──────────────────────────────────────────────────────────────┘");
        output.AppendLine();
    }
}
