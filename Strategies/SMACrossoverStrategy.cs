using System;
using System.Collections.Generic;
using System.Linq;
using TradingBacktester.Models;
using TradingBacktester.Indicators;

namespace TradingBacktester.Strategies
{
    /// <summary>
    /// Simple Moving Average Crossover Strategy
    /// Used for MVP to make sure this works
    ///
    /// TRADING LOGIC:
    /// - BUY when short SMA crosses ABOVE long SMA (bullish signal)
    /// - SELL when short SMA crosses BELOW long SMA (bearish signal)
    ///
    /// Example: 5-day SMA vs 20-day SMA
    /// - When 5-day > 20-day = Recent prices rising faster = BUY
    /// - When 5-day < 20-day = Recent prices falling = SELL
    /// </summary>
    public class SMACrossoverStrategy : IStrategy
    {
        // STRATEGY PARAMETERS
        private readonly int _shortPeriod;   // Fast moving average (e.g., 5 days)
        private readonly int _longPeriod;    // Slow moving average (e.g., 20 days)

        public string StrategyName => $"SMA Crossover ({_shortPeriod}/{_longPeriod})";

        // CONSTRUCTOR
        public SMACrossoverStrategy(int shortPeriod, int longPeriod)
        {
            if (shortPeriod <= 0 || longPeriod <= 0)
                throw new ArgumentException("Periods must be positive");

            if (shortPeriod >= longPeriod)
                throw new ArgumentException("Short period must be less than long period");

            _shortPeriod = shortPeriod;
            _longPeriod = longPeriod;
        }

        /// <summary>
        /// Generate buy/sell signals based on SMA crossovers
        /// </summary>
        public List<TradeSignal> GenerateSignals(List<Price> prices)
        {
            var signals = new List<TradeSignal>();

            // CALCULATE BOTH MOVING AVERAGES
            var shortSMA = MovingAverage.CalculateSMAWithDates(prices, _shortPeriod);
            var longSMA = MovingAverage.CalculateSMAWithDates(prices, _longPeriod);

            // FIND CROSSOVER POINTS
            // Start from the second long SMA value to compare with previous
            for (int i = 1; i < longSMA.Count; i++)
            {
                // CURRENT VALUES
                var currentDate = longSMA[i].Date;
                var currentShort = GetSMAForDate(shortSMA, currentDate);
                var currentLong = longSMA[i].SMA;

                // PREVIOUS VALUES
                var previousDate = longSMA[i - 1].Date;
                var previousShort = GetSMAForDate(shortSMA, previousDate);
                var previousLong = longSMA[i - 1].SMA;

                // DETECT CROSSOVERS
                var currentPrice = prices.First(p => p.Date == currentDate).Close;

                // BULLISH CROSSOVER: Short SMA crosses above Long SMA
                if (previousShort <= previousLong && currentShort > currentLong)
                {
                    var signal = new TradeSignal(
                        currentDate,
                        TradeAction.Buy,
                        currentPrice,
                        $"Bullish crossover: {_shortPeriod}-day SMA ({currentShort:F2}) > {_longPeriod}-day SMA ({currentLong:F2})"
                    );
                    signals.Add(signal);
                }

                // BEARISH CROSSOVER: Short SMA crosses below Long SMA
                else if (previousShort >= previousLong && currentShort < currentLong)
                {
                    var signal = new TradeSignal(
                        currentDate,
                        TradeAction.Sell,
                        currentPrice,
                        $"Bearish crossover: {_shortPeriod}-day SMA ({currentShort:F2}) < {_longPeriod}-day SMA ({currentLong:F2})"
                    );
                    signals.Add(signal);
                }
            }

            return signals;
        }

        /// <summary>
        /// Helper method to get SMA value for a specific date
        /// DO NOT USE EXTENSION because helper very specific to this class
        /// extension would be weird function calling
        /// C# prefers only one argument for extensions (REMEMBER IT'S SYNTACTIC SUGAR)
        /// </summary>
        private decimal GetSMAForDate(List<(DateTime Date, decimal SMA)> smaData, DateTime date)
        {
            var smaPoint = smaData.FirstOrDefault(s => s.Date == date);

            if (smaPoint.Date == default(DateTime))
                throw new InvalidOperationException($"No SMA data found for date {date:yyyy-MM-dd}");

            return smaPoint.SMA;
        }
    }
}