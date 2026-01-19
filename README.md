# Soccer Match Simulator

A Monte Carlo-based soccer match simulator that models goal-scoring outcomes for a 90-minute match using Poisson distribution.

## Overview

This simulator takes team strength parameters (goal seeds) as input and runs multiple simulations to produce statistically meaningful results. Each simulation represents a full 90-minute soccer match, outputting goals scored by each team, the match spread, and total goals.

## Quick Start

### Prerequisites
- Docker (recommended) or .NET 8 SDK

### Running with Docker

```bash
# Start the development container
docker compose up -d

# Run simulation
./dev.sh run --project src/SoccerMatchSimulator -- <seedA> <seedB> [count] [--output file.txt]

# Example: 1000 simulations with Team A seed 1.8, Team B seed 1.2
./dev.sh run --project src/SoccerMatchSimulator -- 1.8 1.2 1000 --output results.txt
```

### Running with .NET SDK

```bash
cd src/SoccerMatchSimulator
dotnet run -- 1.8 1.2 1000 --output results.txt
```

## Technical Decisions

### Why .NET 8 / C#?

1. **Performance**: C# offers excellent performance for numerical computations with minimal overhead
2. **Type Safety**: Strong typing helps catch errors at compile time, crucial for mathematical simulations
3. **Cross-platform**: .NET 8 runs on Windows, macOS, and Linux via Docker
4. **Simplicity**: Top-level statements and records enable clean, minimal code

### Monte Carlo Simulation

The Monte Carlo approach was specified in the technical requirements. This method is well-suited for soccer simulation because:

1. **Probabilistic Nature**: Soccer match outcomes are inherently probabilistic - the same two teams can produce different results
2. **Statistical Convergence**: Running many simulations (1000+) produces stable probability distributions
3. **Flexibility**: Easy to adjust parameters and observe effects on outcomes

### Why Poisson Distribution for Goals?

The **Poisson distribution** is the standard statistical model for soccer goal scoring because:

1. **Rare Events**: Goals are relatively rare events during a match (typically 0-5 per team)
2. **Independence**: Goals can be modeled as independent events occurring at a constant average rate
3. **Historical Validation**: Empirical studies show real soccer match data closely follows Poisson distribution
4. **Single Parameter**: Only requires λ (expected goals) - maps directly to our "goal seed" input

The probability of scoring exactly k goals with expected rate λ is:

```
P(X = k) = (λ^k * e^(-λ)) / k!
```

## Algorithm Implementation

### Poisson Random Number Generation

To generate goals following a Poisson distribution, there are two main algorithms:

| Algorithm | Time Complexity | Best For | Description |
|-----------|-----------------|----------|-------------|
| **Knuth's Algorithm** | O(λ) | Small λ (< 30) | Simple multiplication of uniform randoms |
| **Rejection Method** (Ahrens-Dieter) | O(1) | Large λ (> 30) | Uses normal approximation with rejection sampling |

### Our Choice: Knuth's Algorithm

We chose **Knuth's algorithm** because soccer goal expectations (λ) are typically **0.5 to 3.0**, making it the optimal choice:

```csharp
private static int GeneratePoisson(double lambda, Random random)
{
    if (lambda == 0) return 0;

    double L = Math.Exp(-lambda);
    int k = 0;
    double p = 1.0;

    do
    {
        k++;
        p *= random.NextDouble();
    } while (p > L);

    return k - 1;
}
```

**Why Knuth's Algorithm for this use case:**

1. **Optimal for small λ**: With λ ≈ 1.5 (typical soccer), we average only ~2-3 iterations per Poisson sample
2. **Simplicity**: Only requires uniform random numbers and basic arithmetic
3. **Accuracy**: Produces exact Poisson distribution (no approximation error)
4. **No Dependencies**: Doesn't require external libraries or lookup tables

### Parallel Execution

For large simulation counts (≥10,000), the simulator automatically uses parallel execution:

```csharp
Parallel.For(0, count, () => new Random(), (i, state, localRandom) =>
{
    // Each thread uses its own Random instance for thread safety
    results[i] = SimulateMatch(goalsSeedTeamA, goalsSeedTeamB, localRandom);
    return localRandom;
}, _ => { });
```

## Architecture

The application follows **separation of concerns** with distinct modules for each responsibility:

### Component Overview

| Component | Interface | Implementation | Responsibility |
|-----------|-----------|----------------|----------------|
| **Simulation** | `ISimulator` | `MonteCarloSimulator` | Orchestrates match simulation |
| **Random Generation** | `IPoissonGenerator` | `PoissonGenerator` | Generates Poisson-distributed numbers |
| **Statistics** | `IStatisticsCalculator` | `StatisticsCalculator` | Calculates aggregated statistics |
| **Output** | `IResultFormatter` | `ConsoleResultFormatter` | Formats results for display |

### Dependency Flow

```
Program.cs (CLI Entry Point)
    │
    ├── ISimulator (MonteCarloSimulator)
    │       └── IPoissonGenerator (PoissonGenerator)
    │
    ├── IStatisticsCalculator (StatisticsCalculator)
    │
    └── IResultFormatter (ConsoleResultFormatter)
```

### Benefits of This Architecture

1. **Testability**: Each component can be tested in isolation with mocks
2. **Extensibility**: Easy to add new formatters (JSON, CSV) or distributions
3. **Single Responsibility**: Each class has one clear purpose
4. **Dependency Injection Ready**: Interfaces enable DI container integration

### Input Validation

All inputs are validated with clear error messages:

| Parameter | Valid Range | Error |
|-----------|-------------|-------|
| `goalsSeedTeamA` | 0.0 - 20.0 | `ArgumentOutOfRangeException` |
| `goalsSeedTeamB` | 0.0 - 20.0 | `ArgumentOutOfRangeException` |
| `count` | 1 - 1,000,000 | `ArgumentOutOfRangeException` |

Special cases handled:
- `NaN` and `Infinity` → rejected
- `λ = 0` → returns 0 goals (deterministic)

## Project Structure

```
SoccerMatchSimulator/
├── src/
│   └── SoccerMatchSimulator/
│       ├── Program.cs                    # CLI entry point
│       ├── Models/
│       │   └── MatchResult.cs            # Match result data
│       ├── Simulation/
│       │   ├── ISimulator.cs             # Simulator interface
│       │   ├── MonteCarloSimulator.cs    # Main simulator
│       │   ├── IPoissonGenerator.cs      # Poisson generator interface
│       │   └── PoissonGenerator.cs       # Knuth's algorithm implementation
│       ├── Statistics/
│       │   ├── IStatisticsCalculator.cs  # Calculator interface
│       │   ├── StatisticsCalculator.cs   # Statistics implementation
│       │   └── SimulationStatistics.cs   # Statistics data model
│       └── Output/
│           ├── IResultFormatter.cs       # Formatter interface
│           └── ConsoleResultFormatter.cs # ASCII table formatter
├── tests/
│   └── SoccerMatchSimulator.Tests/
│       ├── MonteCarloSimulatorTests.cs   # Simulator tests
│       ├── PoissonGeneratorTests.cs      # Generator tests
│       ├── StatisticsCalculatorTests.cs  # Statistics tests
│       ├── ConsoleResultFormatterTests.cs # Formatter tests
│       └── ProgramIntegrationTests.cs    # CLI integration tests
├── docker-compose.yml
├── dev.sh                                # Docker helper script
├── .gitignore
└── README.md
```

## Input Parameters

| Parameter | Description | Valid Range | Default |
|-----------|-------------|-------------|---------|
| `goalsSeedTeamA` | Team A's expected goals (λ) | 0-20 | Required |
| `goalsSeedTeamB` | Team B's expected goals (λ) | 0-20 | Required |
| `simulationCount` | Number of matches to simulate | 1-1,000,000 | 1000 |
| `--output <file>` | Write results to a file | - | - |

**Typical seed values:**
- 0.5 - 1.0: Weak/defensive team
- 1.0 - 1.5: Average team
- 1.5 - 2.0: Strong team
- 2.0 - 2.5: Very strong team / weak opponent

## Output

Each simulation produces:

| Field | Description |
|-------|-------------|
| `GoalsTeamA` | Goals scored by Team A |
| `GoalsTeamB` | Goals scored by Team B |
| `Spread` | Goal difference (Team A - Team B) |
| `TotalGoals` | Combined goals in the match |

Summary statistics include:
- Win/Draw/Loss percentages
- Average goals per team
- Average spread and total

## Example Output

```
╔══════════════════════════════════════════════════════════════╗
║           SOCCER MATCH SIMULATOR - MONTE CARLO               ║
╠══════════════════════════════════════════════════════════════╣
║  Team A Seed: 1.80      Team B Seed: 1.20                    ║
║  Simulations: 10000                                          ║
╚══════════════════════════════════════════════════════════════╝

┌────────┬────────────┬────────────┬────────┬───────────┐
│  Sim # │  Team A    │  Team B    │ Spread │   Total   │
├────────┼────────────┼────────────┼────────┼───────────┤
│      1 │          2 │          1 │     +1 │         3 │
│      2 │          1 │          0 │     +1 │         1 │
│    ... │        ... │        ... │    ... │       ... │
└────────┴────────────┴────────────┴────────┴───────────┘

┌──────────────────────────────────────────────────────────────┐
│                         SUMMARY                              │
├──────────────────────────────────────────────────────────────┤
│  Team A Wins:   5081  ( 50.8%)                               │
│  Draws:         2322  ( 23.2%)                               │
│  Team B Wins:   2597  ( 26.0%)                               │
├──────────────────────────────────────────────────────────────┤
│  Avg Goals Team A:  1.79                                     │
│  Avg Goals Team B:  1.21                                     │
│  Avg Spread:       +0.58                                     │
│  Avg Total Goals:   3.00                                     │
└──────────────────────────────────────────────────────────────┘
```

## Running Tests

```bash
# Run all tests
./dev.sh test

# Run with verbose output
./dev.sh test --verbosity normal
```

### Test Coverage

| Component | Tests | Description |
|-----------|-------|-------------|
| `MonteCarloSimulator` | 22 | Simulation behavior, edge cases, validation |
| `PoissonGenerator` | 6 | Random generation, reproducibility, statistics |
| `StatisticsCalculator` | 8 | Win/loss counts, percentages, averages |
| `ConsoleResultFormatter` | 5 | Output format, tables, summary |
| Integration (CLI) | 8 | Argument parsing, file output, errors |
| **Total** | **48** | |

## Error Handling

The application handles errors gracefully:

```bash
# Invalid input
$ dotnet run -- invalid 1.2 10
Error: Invalid goalsSeedTeamA value: invalid

# Out of range
$ dotnet run -- -1.0 1.2 10
Error: Goal seed must be between 0 and 20. (Parameter 'goalsSeedTeamA')

# Invalid simulation count
$ dotnet run -- 1.5 1.2 0
Error: Simulation count must be between 1 and 1,000,000. (Parameter 'count')
```

## Statistical Validation

With sufficient simulations (1000+), the results converge to expected values:

| Metric | Expected | Observed (10k sims) |
|--------|----------|---------------------|
| Avg Goals Team A | 1.80 | ~1.79 |
| Avg Goals Team B | 1.20 | ~1.21 |
| Avg Total Goals | 3.00 | ~3.00 |

This convergence validates that the Poisson distribution is correctly implemented.

## Limitations & Future Improvements

**Current Limitations:**
- Goals are independent (no momentum/fatigue modeling)
- No home/away advantage
- No time-based effects (e.g., late-game pressure)
- Single match type (90 minutes only)

**Potential Enhancements:**
- Correlation between team performances
- Time-segmented simulation (first half vs second half)
- Additional match events (cards, injuries)
- Historical data integration for seed calibration
