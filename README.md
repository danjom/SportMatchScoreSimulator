# Sport Match Score Simulator

A Monte Carlo-based soccer match simulator using Poisson distribution to model goal-scoring outcomes.

## Quick Start

### Prerequisites
- Docker (recommended) or .NET 8 SDK

### Interactive Mode

Run without arguments to get prompted for inputs:

```bash
./dev.sh run --project src/SoccerMatchSimulator
# or
cd src/SoccerMatchSimulator && dotnet run
```

You'll be prompted for:
- Team A & B goal seeds (0-20)
- Number of simulations (default: 1000)
- Whether to save results to file

### Command-Line Mode

```bash
dotnet run -- <seedA> <seedB> [count] [--output [prefix]]
```

**Examples:**
```bash
dotnet run -- 1.8 1.2              # Basic simulation
dotnet run -- 1.8 1.2 5000         # 5000 simulations
dotnet run -- 1.8 1.2 --output     # Save to file
dotnet run -- --help               # Show help
```

### Goal Seed Reference

| Seed Range | Team Strength |
|------------|---------------|
| 0.5 - 1.0  | Weak/defensive |
| 1.0 - 1.5  | Average |
| 1.5 - 2.0  | Strong |
| 2.0 - 2.5  | Very strong |

## Example Output

```
╔══════════════════════════════════════════════════════════════╗
║           SOCCER MATCH SIMULATOR - MONTE CARLO               ║
╠══════════════════════════════════════════════════════════════╣
║  Team A Seed: 1.80      Team B Seed: 1.20                    ║
║  Simulations: 10000                                          ║
╚══════════════════════════════════════════════════════════════╝

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
./dev.sh test
```

## Why Poisson Distribution?

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

We use **Knuth's algorithm** for Poisson random number generation:

| Algorithm | Time Complexity | Best For |
|-----------|-----------------|----------|
| **Knuth's Algorithm** | O(λ) | Small λ (< 30) |
| Rejection Method | O(1) | Large λ (> 30) |

Since soccer goal expectations (λ) are typically **0.5 to 3.0**, Knuth's algorithm is optimal:

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

**Why this works:**
- With λ ≈ 1.5 (typical soccer), we average only ~2-3 iterations per sample
- Only requires uniform random numbers and basic arithmetic
- Produces exact Poisson distribution (no approximation error)

## Statistical Validation

With sufficient simulations (1000+), results converge to expected values:

| Metric | Expected | Observed (10k sims) |
|--------|----------|---------------------|
| Avg Goals Team A | 1.80 | ~1.79 |
| Avg Goals Team B | 1.20 | ~1.21 |
| Avg Total Goals | 3.00 | ~3.00 |

This convergence validates the Poisson distribution implementation.

## Architecture

The application follows **separation of concerns** with distinct components:

```
Program.cs (CLI Entry Point)
    │
    ├── MonteCarloSimulator
    │       └── PoissonScoreGenerator
    │
    ├── StatisticsCalculator
    │
    └── OutputFormatter
```

| Component | Responsibility |
|-----------|----------------|
| **MonteCarloSimulator** | Orchestrates match simulations |
| **PoissonScoreGenerator** | Generates Poisson-distributed goal scores |
| **StatisticsCalculator** | Calculates win/loss/draw statistics |
| **OutputFormatter** | Formats results for console display |

**Benefits:**
- **Testability**: Each component can be tested in isolation with mocks
- **Extensibility**: Easy to add new formatters (JSON, CSV) or distributions
- **Single Responsibility**: Each class has one clear purpose
- **Dependency Injection Ready**: Interfaces enable DI container integration

## Technical Notes

- Uses **Poisson distribution** (standard model for soccer goal scoring)
- Implements **Knuth's algorithm** for random number generation
- Parallel execution for large simulation counts (≥10,000)
- 48 unit and integration tests
