using System;
using System.Collections.Generic;
using System.Linq;
using TradingBacktester.Models;
using TradingBacktester.Strategies;

namespace TradingBacktester.Engine
{
    /// <summary>
    /// Main backtesting engine that orchestrates everything we've written
    /// Takes strategy + price data, simulates trading, returns results
    /// Utilizes Dependency Injection
    /// </summary>
    public class BacktestEngine
    {
        private readonly IStrategy _strategy;
        private readonly decimal _initialCash;
        private readonly decimal _commissionPerTrade;

        // Constructor 
        // NOTETOSELF remember constructors have no return type in C# and has same name as class (Implicit)
        public BacktestEngine(IStrategy strategy, decimal initialCash, decimal commissionPerTrade = 0m)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _initialCash = initialCash;
            _commissionPerTrade = commissionPerTrade;

            if (initialCash <= 0)
                throw new ArgumentException("Initial cash must be positive");
        }

        /// <summary>
        /// Run the complete backtest simulation
        /// </summary>
        public BacktestResult RunBacktest(List<Price> prices)
        {
            if (prices == null || prices.Count == 0)
                throw new ArgumentException("Price data cannot be null or empty");

            // STEP 1: GENERATE SIGNALS
            Console.WriteLine($"🧠 Generating signals using {_strategy.StrategyName}...");
            var signals = _strategy.GenerateSignals(prices);
            Console.WriteLine($"✅ Generated {signals.Count} trading signals");

            // STEP 2: INITIALIZE PORTFOLIO
            var portfolio = new Portfolio(_initialCash);
            var portfolioHistory = new List<PortfolioSnapshot>();

            // STEP 3: SIMULATE DAY-BY-DAY TRADING
            Console.WriteLine("📈 Simulating trading...");

            foreach (var price in prices)
            {
                // CHECK FOR SIGNALS ON THIS DATE
                var todaysSignals = signals.Where(s => s.Date == price.Date).ToList();


                // EXECUTE ANY TRADES
                foreach (var signal in todaysSignals)
                {
                    ExecuteSignal(portfolio, signal, price);
                }

                // RECORD PORTFOLIO STATE
                var snapshot = new PortfolioSnapshot(
                    price.Date,
                    portfolio.Cash,
                    portfolio.SharesOwned,
                    portfolio.CalculateTotalValue(price.Close),
                    price.Close
                );
                portfolioHistory.Add(snapshot);
            }

            // STEP 4: CALCULATE FINAL RESULTS
            var result = new BacktestResult(
                _strategy.StrategyName,
                _initialCash,
                portfolio.CalculateTotalValue(prices.Last().Close),
                portfolio.TradeHistory.ToList(),
                portfolioHistory,
                prices.First().Date,
                prices.Last().Date
            );

            Console.WriteLine("✅ Backtest simulation complete!");
            return result;
        }

        /// <summary>
        /// Execute a trading signal (buy or sell)
        /// </summary>
        private void ExecuteSignal(Portfolio portfolio, TradeSignal signal, Price currentPrice)
        {
            if (signal.Action == TradeAction.Buy)
            {
                // BUY: Calculate how many shares we can afford
                var availableCash = portfolio.Cash - _commissionPerTrade;
                var sharesToBuy = (int)(availableCash / signal.Price);

                if (sharesToBuy > 0)
                {
                    var trade = new Trade(
                        signal.Date,
                        TradeAction.Buy,
                        signal.Price,
                        sharesToBuy,
                        _commissionPerTrade
                    );

                    if (portfolio.ExecuteTrade(trade))
                    {
                        Console.WriteLine($"  🟢 {trade}");
                    }
                }
            }
            else // SELL
            {
                // SELL: Sell all shares we own
                if (portfolio.SharesOwned > 0)
                {
                    var trade = new Trade(
                        signal.Date,
                        TradeAction.Sell,
                        signal.Price,
                        portfolio.SharesOwned,
                        _commissionPerTrade
                    );

                    if (portfolio.ExecuteTrade(trade))
                    {
                        Console.WriteLine($"  🔴 {trade}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Snapshot of portfolio state at a specific point in time
    /// </summary>
    public class PortfolioSnapshot
    {
        public DateTime Date { get; init; }
        public decimal Cash { get; init; }
        public int SharesOwned { get; init; }
        public decimal TotalValue { get; init; }
        public decimal StockPrice { get; init; }

        public PortfolioSnapshot(DateTime date, decimal cash, int sharesOwned, decimal totalValue, decimal stockPrice)
        {
            Date = date;
            Cash = cash;
            SharesOwned = sharesOwned;
            TotalValue = totalValue;
            StockPrice = stockPrice;
        }
    }

    /// <summary>
    /// Complete results of a backtest simulation
    /// </summary>
    public class BacktestResult
    {
        public string StrategyName { get; init; }
        public decimal InitialValue { get; init; }
        public decimal FinalValue { get; init; }
        public List<Trade> Trades { get; init; }
        public List<PortfolioSnapshot> PortfolioHistory { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

        public BacktestResult(string strategyName, decimal initialValue, decimal finalValue,
            List<Trade> trades, List<PortfolioSnapshot> portfolioHistory,
            DateTime startDate, DateTime endDate)
        {
            StrategyName = strategyName;
            InitialValue = initialValue;
            FinalValue = finalValue;
            Trades = trades;
            PortfolioHistory = portfolioHistory;
            StartDate = startDate;
            EndDate = endDate;
        }

        // CALCULATED PROPERTIES
        public decimal TotalReturn => InitialValue == 0 ? 0 : ((FinalValue - InitialValue) / InitialValue) * 100;
        public int TradeCount => Trades.Count;
        public decimal TotalCommissions => Trades.Sum(t => t.Commission);
    }
}