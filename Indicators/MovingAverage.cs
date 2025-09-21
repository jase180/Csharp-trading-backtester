using System;
using System.Collections.Generic;
using System.Linq;
using TradingBacktester.Models;

namespace TradingBacktester.Indicators
{
    /// <summary>
    /// Calculate Simple Moving Average (SMA) for price data
    /// SMA smooths price action by constantly updating the average price
    /// Although data sets may contain SMA, this double checks dataset's SMA and creates a more standalone MVP
    /// </summary>
    public static class MovingAverage
    {
        /// <summary>
        /// Calculate Simple Moving Average for closing prices
        /// Returns a list of SMA values, one for each valid period
        /// </summary>
        /// <param name="prices">Historical price data (must be sorted by date for SMA Calc to work)</param>
        /// <param name="period">Number of days to average (e.g., 5, 20, 50)</param>
        /// <returns>List of SMA values starting from the first valid calculation</returns>
        public static List<decimal> CalculateSMA(List<Price> prices, int period)
        {
            // INPUT VALIDATION
            if (prices == null || prices.Count == 0)
                throw new ArgumentException("Price data cannot be null or empty");

            if (period <= 0)
                throw new ArgumentException("Period must be positive");

            if (period > prices.Count)
                throw new ArgumentException($"Period ({period}) cannot be larger than data points ({prices.Count})");

            var smaValues = new List<decimal>();

            // CALCULATE SMA: For each valid position, take average of previous 'period' prices
            for (int i = period - 1; i < prices.Count; i++)
            {
                // NOTETOSELF LINQ MAGIC: Take 'period' prices ending at position i, get their close prices, calculate average
                // NOTETOSELF Pretty cool way of doing it with LINQ I found
                decimal sma = prices
                    .Skip(i - (period - 1))  // Start from the correct position
                    .Take(period)            // Take exactly 'period' number of prices
                    .Select(p => p.Close)    // Extract just the closing prices
                    .Average();              // Calculate the mean

                smaValues.Add(sma);
            }

            return smaValues;
        }

        /// <summary>
        /// Calculate SMA and return it paired with corresponding dates
        /// Useful for plotting and analysis
        /// </summary>
        public static List<(DateTime Date, decimal SMA)> CalculateSMAWithDates(List<Price> prices, int period)
        {
            var smaValues = CalculateSMA(prices, period);
            var result = new List<(DateTime, decimal)>();

            // PAIR SMA VALUES WITH DATES: Start from position where first SMA is calculated
            for (int i = 0; i < smaValues.Count; i++)
            {
                var dateIndex = i + (period - 1);  // Offset by period to get correct date
                result.Add((prices[dateIndex].Date, smaValues[i]));
            }

            return result;
        }

        /// <summary>
        /// Get the most recent SMA value (useful for trading decisions)
        /// </summary>
        public static decimal GetCurrentSMA(List<Price> prices, int period)
        {
            var smaValues = CalculateSMA(prices, period);
            return smaValues.Last();  // Most recent SMA value
        }
    }

    /// <summary>
    /// NOTETOSELF Try to use extension method from C# here to make it look neater as a learner. 
    /// They are essentially helper methods that don't make the 'main' block of code too noisy.
    /// Extension methods to make SMA calculations cleaner by 'adding' a new method to class without modifying the class itself
    /// Example: prices.ToSMA(20) instead of MovingAverage.CalculateSMA(prices, 20)
    /// </summary>
    public static class PriceExtensions
    {
        public static List<decimal> ToSMA(this List<Price> prices, int period)
        {
            return MovingAverage.CalculateSMA(prices, period);
        }

        public static List<(DateTime Date, decimal SMA)> ToSMAWithDates(this List<Price> prices, int period)
        {
            return MovingAverage.CalculateSMAWithDates(prices, period);
        }

        public static decimal CurrentSMA(this List<Price> prices, int period)
        {
            return MovingAverage.GetCurrentSMA(prices, period);
        }
    }
}