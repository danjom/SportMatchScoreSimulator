namespace SoccerMatchSimulator.Simulation;

/// <summary>
/// Generates scores based on a team's strength parameter.
/// Different implementations model different sports' scoring patterns.
/// </summary>
public interface IScoreGenerator
{
    /// <summary>
    /// Generates a score based on team strength.
    /// </summary>
    /// <param name="strength">Team's scoring strength parameter (λ for Poisson, mean for Normal, etc.).</param>
    /// <returns>A non-negative integer score.</returns>
    int Generate(double strength);
}
