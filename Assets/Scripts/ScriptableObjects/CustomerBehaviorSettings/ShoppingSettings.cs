using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Settings for customer shopping behavior
    /// Centralized configuration for all shopping-related AI parameters
    /// </summary>
    [System.Serializable]
    public class ShoppingSettings
    {
        [Header("Product Selection")]
        [Range(0f, 1f)]
        [Tooltip("Probability that a customer will buy a product they're looking at")]
        public float buyProbability = 0.7f;
        
        [Header("Shopping Behavior")]
        [Tooltip("Default shopping duration in seconds")]
        public float shoppingDuration = 30f;
        
        [Tooltip("Maximum number of products a customer can select")]
        public int maxProducts = 5;
        
        [Header("Movement")]
        [Tooltip("Time spent browsing at each shelf")]
        public float shelfBrowseTime = 3f;
        
        [Tooltip("Probability of moving to a different shelf while shopping")]
        [Range(0f, 1f)]
        public float shelfSwitchProbability = 0.3f;
        
        [Header("Decision Making")]
        [Tooltip("Minimum time between product selection attempts")]
        public float productCheckInterval = 3f;
        
        [Tooltip("Time customer waits when a shelf is empty")]
        public float emptyShelfWaitTime = 2f;
    }
}