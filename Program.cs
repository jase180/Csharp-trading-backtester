using System;                           
using System.IO;                        
using TradingBacktester.Data;           
using TradingBacktester.Strategies;     
using TradingBacktester.Engine;         

namespace TradingBacktester
{
    /// <summary>
    /// Main console application for the C# Trading Backtester MVP
    /// NOTETOSELF: This is the entry point - like main() in Go
    /// Orchestrates the entire backtesting workflow
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // CONSOLE OUTPUT: for clearer output
            Console.WriteLine("🚀 C# Trading Backtester MVP");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            try
            {
                // STEP 1: LOAD HISTORICAL PRICE DATA
                Console.WriteLine("📊 Loading historical price data...");

                var dataSource = new CsvPriceDataSource();

                var prices = dataSource.LoadPrices("Data/sample-data.csv");

                // Show Date Range for info
                Console.WriteLine($"   📅 Date range: {prices[0].Date:yyyy-MM-dd} to {prices[^1].Date:yyyy-MM-dd}");

                // Show Price Range for info
                Console.WriteLine($"   📈 Price range: {prices.Min(p => p.Low):C} to {prices.Max(p => p.High):C}");
                Console.WriteLine();

                // STEP 2: CONFIGURE TRADING STRATEGY
                // FUTURE: For multiple strategies, create a List<IStrategy> and foreach loop through them
                var strategy = new SMACrossoverStrategy(shortPeriod: 3, longPeriod: 8); // FUTURE:extract period for configs

                Console.WriteLine($"🧠 Configuring {strategy.StrategyName}...");

                Console.WriteLine($"   Strategy: {strategy.StrategyName}");
                Console.WriteLine($"   Logic: {strategy.StrategyDescription}");
                Console.WriteLine();

                // STEP 3: SET UP BACKTESTING ENGINE
                Console.WriteLine("⚙️  Setting up backtesting engine...");

                // DECIMAL LITERALS: 'm' suffix indicates decimal type (precise for money)
                // FUTURE: Extract this out to configs
                decimal initialCash = 10000m;
                decimal commission = 5m;

                // DEPENDENCY INJECTION: Pass strategy and config to engine constructor
                var engine = new BacktestEngine(strategy, initialCash, commission);
                Console.WriteLine($"   Initial cash: {initialCash:C}");
                Console.WriteLine($"   Commission per trade: {commission:C}");
                Console.WriteLine();

                // STEP 4: RUN BACKTEST SIMULATION
                Console.WriteLine("🔄 Running backtest simulation...");
                Console.WriteLine();

                // METHOD CALL: Run the actual backtesting simulation
                var result = engine.RunBacktest(prices);
                Console.WriteLine();

                // STEP 5: CALCULATE PERFORMANCE METRICS
                Console.WriteLine("📊 Calculating performance metrics...");

                var metrics = PerformanceCalculator.CalculateMetrics(result);
                Console.WriteLine();

                // STEP 6: DISPLAY RESULTS
                // METHOD CALL: Call our custom display method
                DisplayResults(metrics, result);

                // STEP 7: SHOW TRADE HISTORY
                Console.WriteLine();
                DisplayTradeHistory(result);

                // STEP 8: STRATEGY COMPARISON
                Console.WriteLine();
                Console.WriteLine();

                // STATIC METHOD CALL: Compare multiple strategies
                StrategyComparison.CompareStrategies();

            }
            // SPECIFIC EXCEPTION HANDLING: Catch file-related errors
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine("💡 Make sure the Data/sample-data.csv file exists in your project directory.");
            }
            // GENERAL EXCEPTION HANDLING: Catch any other unexpected errors
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                Console.WriteLine($"   Details: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("✅ Backtesting complete!");
        }

        /// <summary>
        /// Display comprehensive performance results
        /// </summary>
        static void DisplayResults(PerformanceMetrics metrics, BacktestResult result)
        {
            Console.WriteLine("🎯 BACKTEST RESULTS");
            Console.WriteLine("=====================================");
            Console.WriteLine($"Strategy: {result.StrategyName}");
            Console.WriteLine($"Period: {metrics.StartDate:yyyy-MM-dd} to {metrics.EndDate:yyyy-MM-dd} ({metrics.TradingDays} days)");
            Console.WriteLine();

            Console.WriteLine("💰 FINANCIAL PERFORMANCE");
            Console.WriteLine($"   Initial Value:     {metrics.InitialValue:C}");
            Console.WriteLine($"   Final Value:       {metrics.FinalValue:C}");
            Console.WriteLine($"   Total Profit:      {metrics.TotalProfit:C}");
            Console.WriteLine($"   Total Return:      {metrics.TotalReturn:F2}%");
            Console.WriteLine();

            Console.WriteLine("📈 TRADING STATISTICS");
            Console.WriteLine($"   Total Trades:      {metrics.TotalTrades}");
            Console.WriteLine($"   Winning Trades:    {metrics.WinningTrades}");
            Console.WriteLine($"   Losing Trades:     {metrics.LosingTrades}");
            Console.WriteLine($"   Win Rate:          {metrics.WinRate:F1}%");
            Console.WriteLine($"   Average Win:       {metrics.AverageWin:C}");
            Console.WriteLine($"   Average Loss:      {metrics.AverageLoss:C}");
            Console.WriteLine($"   Profit Factor:     {metrics.ProfitFactor:F2}");
            Console.WriteLine();

            Console.WriteLine("⚠️  RISK METRICS");
            Console.WriteLine($"   Max Drawdown:      {metrics.MaxDrawdown:F2}%");
            Console.WriteLine($"   Total Commissions: {metrics.TotalCommissions:C}");
            Console.WriteLine();

            // PERFORMANCE ASSESSMENT
            Console.WriteLine("🎖️  PERFORMANCE ASSESSMENT");
            if (metrics.TotalReturn > 10)
                Console.WriteLine("   ✅ Excellent performance!");
            else if (metrics.TotalReturn > 5)
                Console.WriteLine("   ✅ Good performance!");
            else if (metrics.TotalReturn > 0)
                Console.WriteLine("   ⚠️  Modest gains");
            else
                Console.WriteLine("   ❌ Strategy lost money");

            if (metrics.WinRate > 60)
                Console.WriteLine("   ✅ High win rate");
            else if (metrics.WinRate > 50)
                Console.WriteLine("   ⚠️  Moderate win rate");
            else
                Console.WriteLine("   ❌ Low win rate");
        }

        /// <summary>
        /// Display detailed trade history
        /// </summary>
        static void DisplayTradeHistory(BacktestResult result)
        {
            Console.WriteLine("📋 TRADE HISTORY");
            Console.WriteLine("=====================================");

            if (result.Trades.Count == 0)
            {
                Console.WriteLine("   No trades were executed.");
                return;
            }

            foreach (var trade in result.Trades)
            {
                var icon = trade.Action == Models.TradeAction.Buy ? "🟢" : "🔴";
                Console.WriteLine($"   {icon} {trade}");
            }
        }
    }
}