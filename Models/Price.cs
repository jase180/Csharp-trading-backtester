using System;

namespace TradingBacktester.Models
{
    /// <summary>
    /// Represents a single price data point for a stock on a specific date.
    /// OHLC stands for Open, High, Low, Close - the four key prices for any trading period.
    /// </summary>
    public class Price
    {
        // CORE DATA PROPERTIES
        // These are public because models are data containers - other code needs access
        // TO BE REMOVED: 'get; init;' means read-only after construction (immutable data)
        public DateTime Date { get; init; }
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }

        // CONSTRUCTOR WITH VALIDATION
        public Price(DateTime date, decimal open, decimal high, decimal low, decimal close)
        {
            // INPUT VALIDATION
            if (high < low)
                throw new ArgumentException($"High price ({high}) cannot be less than low price ({low})");

            if (high < open || high < close)
                throw new ArgumentException($"High price ({high}) must be >= open ({open}) and close ({close})");

            if (low > open || low > close)
                throw new ArgumentException($"Low price ({low}) must be <= open ({open}) and close ({close})");

            // All validation passed - assign the values
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }

        // CALCULATED PROPERTIES

        /// <summary>
        /// Daily price range - useful for measuring volatility
        /// </summary>
        public decimal Range => High - Low;

        /// <summary>
        /// Daily price change (close minus open)
        /// </summary>
        public decimal Change => Close - Open;

        /// <summary>
        /// Daily percentage change
        /// TO BE REMOVED: Uses ternary operator: condition ? ifTrue : ifFalse, C# syntactic sugar
        /// Protects against division by zero if Open price is somehow 0
        /// </summary>
        public decimal PercentChange => Open == 0 ? 0 : (Change / Open) * 100;

        // DEBUG FUNCTIONS

        public override string ToString()
        {
            // String interpolation with formatting:
            // :yyyy-MM-dd formats the date as 2024-01-15 TO BE REMOVED
            // :C formats as currency like $123.45 TO BE REMOVED 
            return $"{Date:yyyy-MM-dd}: O:{Open:C} H:{High:C} L:{Low:C} C:{Close:C}";
        }
    }
}