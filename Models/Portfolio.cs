using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingBacktester.Models
{
    /// <summary>
    /// Represents the current state of our trading portfolio
    /// Tracks cash balance and stock positions
    /// </summary>
    public class Portfolio
    {
        // CORE DATA PROPERTIES
        public decimal Cash { get; private set; }              // Available cash
        public List<Trade> TradeHistory { get; private set; }  // All trades executed
        public int SharesOwned { get; private set; }           // Current position

        // CONSTRUCTOR
        public Portfolio(decimal initialCash)
        {
            if (initialCash < 0)
                throw new ArgumentException("Initial cash cannot be negative");

            Cash = initialCash;
            TradeHistory = new List<Trade>();
            SharesOwned = 0;
        }

        // BUSINESS LOGIC METHODS

        /// <summary>
        /// Execute a trade and update portfolio state
        /// Returns true if trade was successful, false if insufficient funds/shares
        /// </summary>
        public bool ExecuteTrade(Trade trade)
        {
            // VALIDATION: Check if we can afford this trade
            if (trade.Action == TradeAction.Buy)
            {
                // Buying: Need enough cash
                if (Cash < trade.TotalCost)
                    return false; // Insufficient funds
            }
            else
            {
                // Selling: Need enough shares
                if (SharesOwned < trade.Shares)
                    return false; // Insufficient shares
            }

            // EXECUTE: Update portfolio state
            Cash -= trade.TotalCost;  // TotalCost is positive for buys, negative for sells

            if (trade.Action == TradeAction.Buy)
                SharesOwned += trade.Shares;
            else
                SharesOwned -= trade.Shares;

            // RECORD: Add to history
            TradeHistory.Add(trade);

            return true; // Trade successful
        }

        // CALCULATED PROPERTIES

        /// <summary>
        /// Calculate total portfolio value (cash + stock value)
        /// Requires current stock price to value the shares
        /// </summary>
        public decimal CalculateTotalValue(decimal currentStockPrice)
        {
            return Cash + (SharesOwned * currentStockPrice);
        }

        /// <summary>
        /// Original starting cash (useful for calculating returns)
        /// </summary>
        public decimal InitialValue => Cash + TradeHistory.Sum(t => t.TotalCost);

        /// <summary>
        /// Total amount spent on commissions
        /// </summary>
        public decimal TotalCommissions => TradeHistory.Sum(t => t.Commission);

        /// <summary>
        /// Number of trades executed
        /// </summary>
        public int TradeCount => TradeHistory.Count;

        // DEBUG FUNCTIONS

        public override string ToString()
        {
            return $"Portfolio: {Cash:C} cash, {SharesOwned} shares, {TradeCount} trades";
        }
    }
}
