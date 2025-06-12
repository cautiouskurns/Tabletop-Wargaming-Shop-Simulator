using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Interface for objects that can be interacted with by the player
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player interacts with this object
        /// </summary>
        /// <param name="player">The player GameObject that is interacting</param>
        void Interact(GameObject player);
        
        /// <summary>
        /// Display name for the interaction (shown in UI)
        /// </summary>
        string InteractionText { get; }
        
        /// <summary>
        /// Whether this object can currently be interacted with
        /// </summary>
        bool CanInteract { get; }
        
        /// <summary>
        /// Called when the player starts looking at this object
        /// </summary>
        void OnInteractionEnter();
        
        /// <summary>
        /// Called when the player stops looking at this object
        /// </summary>
        void OnInteractionExit();
    }
}
