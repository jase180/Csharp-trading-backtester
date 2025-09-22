using System;
using System.Collections.Generic;
using System.Linq;
using TradingBacktester.Models;

namespace TradingBacktester.Engine
{
    /// <summary>
    /// Calculate performance metrics for backtesting results
    /// What good is a backtester if we don't know how well it's performing, right?
    /// </summary>
    public static class PerformanceCalculator
    {
        /// <summary>
        /// Calculate comprehensive performance metrics
        /// </summary>
        public static PerformanceMetrics CalculateMetrics(BacktestResult result)
        {
            return new PerformanceMetrics
            {
                // BASIC METRICS
                TotalReturn = result.TotalReturn,
                InitialValue = result.InitialValue,
                FinalValue = result.FinalValue,
                TotalProfit = result.FinalValue - result.InitialValue,

                // TRADE METRICS
                TotalTrades = result.TradeCount,
                TotalCommissions = result.TotalCommissions,
                WinningTrades = CalculateWinningTrades(result.Trades),
                LosingTrades = CalculateLosingTrades(result.Trades),

                // TIME METRICS
                TradingDays = (result.EndDate - result.StartDate).Days,
                StartDate = result.StartDate,
                EndDate = result.EndDate,

                // RISK METRICS
                MaxDrawdown = CalculateMaxDrawdown(result.PortfolioHistory),

                // CALCULATED METRICS
                WinRate = CalculateWinRate(result.Trades),
                AverageWin = CalculateAverageWin(result.Trades),
                AverageLoss = CalculateAverageLoss(result.Trades),
                ProfitFactor = CalculateProfitFactor(result.Trades)
            };
        }

        /// <summary>
        /// Calculate maximum drawdown and returns it as a percentage
        /// Use static because it's a function with in and out
        /// </summary>
        private static decimal CalculateMaxDrawdown(List<PortfolioSnapshot> history)
        {
            if (history.Count == 0) return 0;

            decimal maxValue = 0;
            decimal maxDrawdown = 0;

            foreach (var snapshot in history)
            {
                // Update running maximum
                if (snapshot.TotalValue > maxValue)
                    maxValue = snapshot.TotalValue;

                // Calculate current drawdown from peak
                decimal currentDrawdown = (maxValue - snapshot.TotalValue) / maxValue * 100;

                // Update maximum drawdown
                if (currentDrawdown > maxDrawdown)
                    maxDrawdown = currentDrawdown;
            }

            return maxDrawdown;
        }

        /// <summary>
        /// Calculate win rate (percentage of profitable trades)
        /// </summary>
        private static decimal CalculateWinRate(List<Trade> trades)
        {
            if (trades.Count == 0) return 0;

            var tradePairs = GetTradePairs(trades);
            if (tradePairs.Count == 0) return 0;

            var winningPairs = tradePairs.Count(pair => pair.Profit > 0);
            return (decimal)winningPairs / tradePairs.Count * 100;
        }

        /// <summary>
        /// Get buy/sell trade pairs to calculate individual trade profits
        /// </summary>
        private static List<TradePair> GetTradePairs(List<Trade> trades)
        {
            var pairs = new List<TradePair>();
            Trade buyTrade = null;

            foreach (var trade in trades.OrderBy(t => t.Date))
            {
                if (trade.Action == TradeAction.Buy)
                {
                    buyTrade = trade;
                }
                else if (trade.Action == TradeAction.Sell && buyTrade != null)
                {
                    var profit = (trade.Price - buyTrade.Price) * buyTrade.Shares - buyTrade.Commission - trade.Commission;
                    pairs.Add(new TradePair { Buy = buyTrade, Sell = trade, Profit = profit });
                    buyTrade = null;
                }
            }

            return pairs;
        }

        /// <summary>
        /// Count how many trade pairs were profitable
        /// This shows successful round trips (BUY → SELL cycles that made money)
        /// </summary>
        private static int CalculateWinningTrades(List<Trade> trades)
        {
            // LINQ Count() with predicate: Count items for the lambda function
            return GetTradePairs(trades).Count(pair => pair.Profit > 0);
        }

        /// <summary>
        /// Count how many trade pairs lost money or broke even
        /// NOTETOSELF: <= 0 includes break-even trades as "losses" (due to commissions even if 0)
        /// </summary>
        private static int CalculateLosingTrades(List<Trade> trades)
        {
            // LINQ Count() with predicate: Count items for the lambda function
            return GetTradePairs(trades).Count(pair => pair.Profit <= 0);
        }

        /// <summary>
        /// Calculate average profit of winning trades
        /// Only looks at profitable trades to see typical win size
        /// </summary>
        private static decimal CalculateAverageWin(List<Trade> trades)
        {
            // NOETTOSELF: LINQ CHAINING: Where() filters, ToList() converts to a list
            var winningTrades = GetTradePairs(trades).Where(pair => pair.Profit > 0).ToList();

            // Prevents division by zero if no winning trades exist (sad)
            return winningTrades.Any() ? winningTrades.Average(pair => pair.Profit) : 0;
        }

        /// <summary>
        /// Calculate average loss of losing trades
        /// </summary>
        private static decimal CalculateAverageLoss(List<Trade> trades)
        {
            // SAME PATTERN as CalculateAverageWin but filters for losses
            var losingTrades = GetTradePairs(trades).Where(pair => pair.Profit <= 0).ToList();

            // DEFENSIVE PROGRAMMING: Check if collection has items before calling Average()
            return losingTrades.Any() ? losingTrades.Average(pair => pair.Profit) : 0;
        }

        /// <summary>
        /// Calculate profit factor: Total Wins ÷ Total Losses
        /// NOTETOSELF: > 1.0 = profitable strategy, < 1.0 = losing strategy
        /// Industry standard metric for comparing strategies
        /// </summary>
        private static decimal CalculateProfitFactor(List<Trade> trades)
        {
            var pairs = GetTradePairs(trades);

            // SEPARATE WIN/LOSS CALCULATIONS using LINQ Sum() extension
            var totalWins = pairs.Where(pair => pair.Profit > 0).Sum(pair => pair.Profit);

            // Math.Abs() converts negative losses to positive numbers for division
            // NOTETOSELF: Losses are negative, but we want positive number for ratio
            var totalLosses = Math.Abs(pairs.Where(pair => pair.Profit <= 0).Sum(pair => pair.Profit));

            // EDGE CASE HANDLING: What if no losses? (Perfect strategy)
            // Return 999 if profitable with no losses, 0 if no trades at all
            return totalLosses == 0 ? (totalWins > 0 ? 999 : 0) : totalWins / totalLosses;
        }

        /// <summary>
        /// Helper class for tracking buy/sell pairs
        /// </summary>
        private class TradePair
        {
            public Trade Buy { get; set; }
            public Trade Sell { get; set; }
            public decimal Profit { get; set; }
        }
    }

    /// <summary>
    /// Comprehensive performance metrics
    /// Because it is essentially a helper class and what we want is just the metrics
    /// Define it here at the bottom for logical reading flow
    /// </summary>
    public class PerformanceMetrics
    {
        // BASIC METRICS
        public decimal TotalReturn { get; set; }
        public decimal InitialValue { get; set; }
        public decimal FinalValue { get; set; }
        public decimal TotalProfit { get; set; }

        // TRADE METRICS
        public int TotalTrades { get; set; }
        public decimal TotalCommissions { get; set; }
        public int WinningTrades { get; set; }
        public int LosingTrades { get; set; }

        // TIME METRICS
        public int TradingDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // RISK METRICS
        public decimal MaxDrawdown { get; set; }

        // CALCULATED METRICS
        public decimal WinRate { get; set; }
        public decimal AverageWin { get; set; }
        public decimal AverageLoss { get; set; }
        public decimal ProfitFactor { get; set; }
    }
}