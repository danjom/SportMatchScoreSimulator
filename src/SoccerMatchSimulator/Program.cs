using SoccerMatchSimulator.Models;
using SoccerMatchSimulator.Output;
using SoccerMatchSimulator.Simulation;
using SoccerMatchSimulator.Statistics;

// ============================================
// Soccer Match Simulator - Monte Carlo
// ============================================

const string ResultsFolder = "results";
const string DefaultOutputPrefix = "result";

double goalsSeedTeamA;
double goalsSeedTeamB;
int simulationCount = 1000;
string? outputPrefix = null;
bool saveToFile = false;

// Check for --help flag
if (args.Any(a => a == "--help" || a == "-h"))
{
    PrintUsage();
    return 0;
}

// Determine mode: interactive (no args) or command-line
if (args.Length == 0)
{
    // Interactive mode
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║           SOCCER MATCH SIMULATOR - MONTE CARLO               ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    goalsSeedTeamA = PromptForDouble("Enter Team A goal seed (0-20, e.g., 1.5): ");
    goalsSeedTeamB = PromptForDouble("Enter Team B goal seed (0-20, e.g., 1.2): ");
    simulationCount = PromptForInt("Enter number of simulations (default 1000): ", 1000);

    Console.Write("Save results to file? (y/N): ");
    var saveResponse = Console.ReadLine()?.Trim().ToLower();
    if (saveResponse == "y" || saveResponse == "yes")
    {
        saveToFile = true;
        Console.Write("Enter result file prefix (default 'result'): ");
        var prefixInput = Console.ReadLine()?.Trim();
        outputPrefix = string.IsNullOrEmpty(prefixInput) ? DefaultOutputPrefix : prefixInput;
    }
    Console.WriteLine();
}
else if (args.Length >= 2)
{
    // Command-line mode
    if (!double.TryParse(args[0], out goalsSeedTeamA))
    {
        Console.Error.WriteLine($"Error: Invalid goalsSeedTeamA value: {args[0]}");
        return 1;
    }

    if (!double.TryParse(args[1], out goalsSeedTeamB))
    {
        Console.Error.WriteLine($"Error: Invalid goalsSeedTeamB value: {args[1]}");
        return 1;
    }

    // Parse optional arguments
    for (int i = 2; i < args.Length; i++)
    {
        if (args[i] == "--output")
        {
            saveToFile = true;
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                outputPrefix = args[i + 1];
                i++;
            }
            else
            {
                outputPrefix = DefaultOutputPrefix;
            }
        }
        else if (!args[i].StartsWith("--") && int.TryParse(args[i], out int count))
        {
            simulationCount = count;
        }
    }
}
else
{
    PrintUsage();
    return 1;
}

// Run simulations
var simulator = new MonteCarloSimulator();
IReadOnlyList<MatchResult> results;
try
{
    results = simulator.RunSimulations(goalsSeedTeamA, goalsSeedTeamB, simulationCount);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unexpected error: {ex.Message}");
    return 1;
}

// Calculate statistics and format output
var statistics = StatisticsCalculator.Calculate(results);
var output = OutputFormatter.Format(results, statistics, goalsSeedTeamA, goalsSeedTeamB);

// Output to console
Console.Write(output);

// Write to file if requested
if (saveToFile)
{
    try
    {
        // Ensure results folder exists
        Directory.CreateDirectory(ResultsFolder);

        // Generate filename with prefix and UTC timestamp
        var prefix = outputPrefix ?? DefaultOutputPrefix;
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var filename = $"{prefix}_{timestamp}.txt";
        var filepath = Path.Combine(ResultsFolder, filename);

        File.WriteAllText(filepath, output);
        Console.WriteLine($"Results written to: {filepath}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error writing to file: {ex.Message}");
        return 1;
    }
}

return 0;

// ============================================
// Helper Methods
// ============================================

static void PrintUsage()
{
    Console.WriteLine("Soccer Match Simulator - Monte Carlo");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run                     Interactive mode (prompts for input)");
    Console.WriteLine("  dotnet run <seedA> <seedB> ... Command-line mode");
    Console.WriteLine();
    Console.WriteLine("Command-line Arguments:");
    Console.WriteLine("  goalsSeedTeamA    Team A's goal-scoring strength (0-20, e.g., 1.5)");
    Console.WriteLine("  goalsSeedTeamB    Team B's goal-scoring strength (0-20, e.g., 1.2)");
    Console.WriteLine("  simulationCount   Number of simulations (1-1,000,000, default: 1000)");
    Console.WriteLine("  --output [prefix] Save results to results/<prefix>_<timestamp>.txt");
    Console.WriteLine("  --help, -h        Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run                              # Interactive mode");
    Console.WriteLine("  dotnet run 1.5 1.2 1000");
    Console.WriteLine("  dotnet run 1.5 1.2 1000 --output");
    Console.WriteLine("  dotnet run 1.5 1.2 1000 --output my_sim");
}

static double PromptForDouble(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (double.TryParse(input, out double value))
        {
            return value;
        }
        Console.WriteLine("Invalid input. Please enter a valid number.");
    }
}

static int PromptForInt(string prompt, int defaultValue)
{
    Console.Write(prompt);
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        return defaultValue;
    }
    if (int.TryParse(input, out int value))
    {
        return value;
    }
    Console.WriteLine($"Invalid input. Using default: {defaultValue}");
    return defaultValue;
}
