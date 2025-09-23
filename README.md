# C# Trading Backtester

A simple backtesting engine for testing trading strategies on historical stock data.

> **Note**: This is currently an MVP (Minimum Viable Product). More features and improvements are actively being developed.

## Features

- **Multiple Trading Strategies**: SMA Crossover, RSI, Buy & Hold, ML Predictions
- **Performance Metrics**: Returns, win rate, drawdown, profit factor
- **Flexible Data Sources**: CSV import with automatic validation
- **Strategy Comparison**: Test multiple strategies on the same dataset

## Quick Start

```bash
# Clone and run
git clone <repo-url>
cd Csharp-trading-backtester
dotnet run
```

## Usage

### Basic Backtesting
```csharp
// Load data
var dataSource = new CsvPriceDataSource();
var prices = dataSource.LoadPrices("Data/sample-data.csv");

// Choose strategy
var strategy = new SMACrossoverStrategy(shortPeriod: 5, longPeriod: 20);

// Run backtest
var engine = new BacktestEngine(strategy, initialCash: 10000m, commission: 5m);
var result = engine.RunBacktest(prices);

// View results
var metrics = PerformanceCalculator.CalculateMetrics(result);
```

### Supported Strategies

- **SMACrossoverStrategy**: Buy when fast SMA crosses above slow SMA
- **RSIStrategy**: Buy when RSI < 30, sell when RSI > 70
- **BuyAndHoldStrategy**: Buy once and hold
- **MLPredictionStrategy**: Trade based on ML model predictions

### Data Format

CSV files should have this format:
```csv
Date,Open,High,Low,Close
2024-01-02,100.00,102.50,99.50,101.00
2024-01-03,101.25,103.75,100.75,102.50
```

## Project Structure

```
├── Models/           # Data models (Price, Trade, Portfolio)
├── Strategies/       # Trading strategy implementations
├── Engine/          # Backtesting engine and performance calculation
├── Data/            # CSV data sources and sample data
├── Indicators/      # Technical indicators (SMA, RSI, etc.)
└── Program.cs       # Main application entry point
```

## Performance Metrics

- **Total Return**: Percentage gain/loss
- **Win Rate**: Percentage of profitable trades
- **Max Drawdown**: Largest peak-to-trough decline
- **Profit Factor**: Gross profit ÷ gross loss
- **Sharpe Ratio**: Risk-adjusted returns

## Requirements

- .NET 9.0
- No external dependencies

## License

MIT