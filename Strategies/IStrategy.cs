using System.Collections.Generic;
using TradingBacktester.Models;

namespace TradingBacktester.Strategies
{
    /// <summary>
    /// Interface for all trading strategies
    /// NOTETOSELF C# interfacing is NOT IMPLICIT LIKE GOLANG
    /// To be added after MVP: Configs
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Generate trading signals based on price data
        /// </summary>
        /// <param name="prices">Historical price data</param>
        /// <returns>List of buy/sell signals</returns>
        List<TradeSignal> GenerateSignals(List<Price> prices);

        /// <summary>
        /// Name of this strategy (for reporting)
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Description of this strategy's logic (for user display)
        /// </summary>
        string StrategyDescription { get; }
    }
}