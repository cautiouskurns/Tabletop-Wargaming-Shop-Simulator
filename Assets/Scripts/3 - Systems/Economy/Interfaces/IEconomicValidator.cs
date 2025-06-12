using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Interface for validating and processing economic transactions in the shop simulator.
    /// Provides abstraction layer for economic operations, allowing different implementations
    /// for testing, different game modes, or alternative economic systems.
    /// </summary>
    public interface IEconomicValidator
    {
        /// <summary>
        /// Check if the player has sufficient funds to afford a given cost
        /// </summary>
        /// <param name="cost">The cost to validate</param>
        /// <returns>True if the player can afford the cost, false otherwise</returns>
        bool CanAffordCost(float cost);
        
        /// <summary>
        /// Process an economic transaction by deducting the cost from available funds
        /// </summary>
        /// <param name="cost">The cost to deduct</param>
        /// <param name="description">Description of the transaction for logging/tracking</param>
        /// <returns>True if the transaction was successful, false if insufficient funds or other error</returns>
        bool ProcessTransaction(float cost, string description);
        
        /// <summary>
        /// Calculate the total cost for restocking items
        /// </summary>
        /// <param name="quantity">Number of items to restock</param>
        /// <param name="basePrice">Base price per item</param>
        /// <param name="multiplier">Cost multiplier (e.g., 0.7 for 70% of retail price)</param>
        /// <returns>Total calculated restock cost</returns>
        float CalculateRestockCost(int quantity, float basePrice, float multiplier);
        
        /// <summary>
        /// Get current available funds for display or validation purposes
        /// </summary>
        /// <returns>Current available money/funds</returns>
        float GetAvailableFunds();
        
        /// <summary>
        /// Check if the economic validator is available and functional
        /// </summary>
        /// <returns>True if validator is ready to process transactions</returns>
        bool IsAvailable();
    }
}
