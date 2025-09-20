using System;

namespace TradingBacktester.Models
{
    /// <summary>
    /// Represents a single trade transaction (buy or sell)
    /// This is an immutable record to make sure trade history CANNOT be overwritten
    /// </summary>
    public class Trade
    {
        // CORE DATA PROPERTIES
        public DateTime Date { get; init; }
        public TradeAction Action { get; init; }  // Buy or Sell 
        public decimal Price { get; init; }       // Price per share
        public int Shares { get; init; }          // Number of shares
        public decimal Commission { get; init; }  // Trading fees

        // CONSTRUCTOR WITH VALIDATION
        public Trade(DateTime date, TradeAction action, decimal price, int shares, decimal commission = 0m)
        {
            // INPUT VALIDATION: Business rules for trades
            if (price <= 0)
                throw new ArgumentException("Price must be positive");

            if (shares <= 0)
                throw new ArgumentException("Shares must be positive");

            if (commission < 0)
                throw new ArgumentException("Commission cannot be negative");

            Date = date;
            Action = action;
            Price = price;
            Shares = shares;
            Commission = commission;
        }

        // CALCULATED PROPERTIES
        // Use if Buy else Sell (implied)
        public decimal TotalCost => Action == TradeAction.Buy
            ? (Price * Shares) + Commission      // Buying costs money
            : -((Price * Shares) - Commission);  // Selling gives money back

        /// <summary>
        /// Total value without considering fees
        /// </summary>
        public decimal GrossValue => Price * Shares;

        // DEBUG FUNCTIONS

        /// <summary>
        /// Display trade information clearly
        /// </summary>
        public override string ToString()
        {
            var actionText = Action == TradeAction.Buy ? "BUY" : "SELL";
            return $"{Date:yyyy-MM-dd} {actionText} {Shares} shares at {Price:C} = {TotalCost:C}";
        }
    }

    /// <summary>
    /// Enum: Use enum for multiple reasons rather than strings
    /// 1. Prevents typos (compiler checks) MAINLY because now it checks the 0 rather than the buy string
    /// 2. Can't accidentally use invalid values
    /// 3. More memory efficient than strings (BONUS)
    /// </summary>
    public enum TradeAction
    {
        Buy, // enum makes buy = 0
        Sell // enum makes buy = 0
    }
}