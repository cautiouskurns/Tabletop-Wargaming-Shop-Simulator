using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles player interaction with shelf slots including IInteractable implementation and mouse events
    /// </summary>
    public class ShelfSlotInteraction : MonoBehaviour, IInteractable
    {
        // Component references
        private ShelfSlotLogic slotLogic;
        private ShelfSlotVisuals slotVisuals;
        private Collider slotCollider;
        
        // IInteractable Properties
        public string InteractionText => slotLogic.IsEmpty ? GetPlacementText() : $"Remove {slotLogic.CurrentProduct.ProductData?.ProductName ?? "Product"}";
        public bool CanInteract => true;
        
        private void Awake()
        {
            // Get component references
            slotLogic = GetComponent<ShelfSlotLogic>();
            slotVisuals = GetComponent<ShelfSlotVisuals>();
            
            // Set layer for interaction system
            InteractionLayers.SetShelfLayer(gameObject);
            
            // Get collider for mouse interactions
            slotCollider = GetComponent<Collider>();
            if (slotCollider == null)
            {
                // Add a collider if none exists
                slotCollider = gameObject.AddComponent<BoxCollider>();
                BoxCollider boxCollider = slotCollider as BoxCollider;
                boxCollider.size = new Vector3(1f, 0.5f, 1f);
                boxCollider.isTrigger = true;
            }
        }
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle player interaction with this slot
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (slotLogic.IsEmpty)
            {
                // Try to place selected product from inventory
                PlaceProductFromInventory();
            }
            else
            {
                Debug.Log($"Player interacted with slot {name} - removing {slotLogic.CurrentProduct.ProductData?.ProductName}");
                // Remove product and add back to inventory
                Product removedProduct = slotLogic.RemoveProduct();
                if (removedProduct != null)
                {
                    // Add product back to inventory
                    if (removedProduct.ProductData != null)
                    {
                        InventoryManager.Instance.AddProduct(removedProduct.ProductData, 1);
                        Debug.Log($"Added {removedProduct.ProductData.ProductName} back to inventory");
                    }
                    
                    // Destroy the visual product object
                    Destroy(removedProduct.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Called when player starts looking at this slot
        /// </summary>
        public void OnInteractionEnter()
        {
            if (slotVisuals != null)
            {
                slotVisuals.ApplyHighlight();
            }
        }
        
        /// <summary>
        /// Called when player stops looking at this slot
        /// </summary>
        public void OnInteractionExit()
        {
            if (slotVisuals != null)
            {
                slotVisuals.RemoveHighlight();
            }
        }
        
        #endregion
        
        #region Mouse Interactions
        
        /// <summary>
        /// Handle mouse click on slot - for player stocking interactions
        /// </summary>
        private void OnMouseDown()
        {
            if (slotLogic.IsEmpty)
            {
                Debug.Log($"Clicked on empty slot {name} - attempting to place product from inventory");
                PlaceProductFromInventory();
            }
            else
            {
                Debug.Log($"Clicked on slot {name} containing {slotLogic.CurrentProduct.ProductData?.ProductName}");
                // Allow removal by clicking (alternative to E key interaction)
                Product removedProduct = slotLogic.RemoveProduct();
                if (removedProduct != null && removedProduct.ProductData != null)
                {
                    // Add product back to inventory
                    InventoryManager.Instance.AddProduct(removedProduct.ProductData, 1);
                    Debug.Log($"Clicked to remove: Added {removedProduct.ProductData.ProductName} back to inventory");
                    
                    // Destroy the visual product object
                    Destroy(removedProduct.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Handle mouse enter for hover highlighting
        /// </summary>
        private void OnMouseEnter()
        {
            if (slotVisuals != null)
            {
                slotVisuals.ApplyHighlight();
            }
            
            if (slotLogic.IsEmpty)
            {
                Debug.Log($"Hovering over empty slot {name}");
            }
            else
            {
                Debug.Log($"Hovering over slot {name} with {slotLogic.CurrentProduct.ProductData?.ProductName}");
            }
        }
        
        /// <summary>
        /// Handle mouse exit to remove highlighting
        /// </summary>
        private void OnMouseExit()
        {
            if (slotVisuals != null)
            {
                slotVisuals.RemoveHighlight();
            }
        }
        
        #endregion
        
        #region Inventory Integration
        
        /// <summary>
        /// Attempt to place the currently selected product from inventory
        /// </summary>
        private void PlaceProductFromInventory()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null)
            {
                Debug.LogWarning("InventoryManager not found!");
                return;
            }
            
            ProductData selectedProduct = inventory.SelectedProduct;
            if (selectedProduct == null)
            {
                Debug.Log("No product selected in inventory");
                if (slotVisuals != null)
                {
                    StartCoroutine(slotVisuals.InteractionFeedback());
                }
                return;
            }
            
            if (!inventory.HasProduct(selectedProduct, 1))
            {
                Debug.Log($"Not enough {selectedProduct.ProductName} in inventory");
                if (slotVisuals != null)
                {
                    StartCoroutine(slotVisuals.InteractionFeedback());
                }
                return;
            }
            
            // Remove product from inventory
            bool removed = inventory.RemoveProduct(selectedProduct, 1);
            if (!removed)
            {
                Debug.LogWarning($"Failed to remove {selectedProduct.ProductName} from inventory");
                return;
            }
            
            // Create and place product on shelf
            slotLogic.CreateAndPlaceProduct(selectedProduct);
            
            Debug.Log($"Placed {selectedProduct.ProductName} from inventory onto shelf. Remaining: {inventory.GetProductCount(selectedProduct)}");
        }
        
        /// <summary>
        /// Get interaction text for placing products from inventory
        /// </summary>
        /// <returns>Descriptive text for the interaction</returns>
        private string GetPlacementText()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return "Empty Slot";
            
            ProductData selectedProduct = inventory.SelectedProduct;
            if (selectedProduct == null) return "Empty Slot - No Product Selected";
            
            int quantity = inventory.GetProductCount(selectedProduct);
            if (quantity <= 0) return $"Empty Slot - No {selectedProduct.ProductName} Available";
            
            return $"Place {selectedProduct.ProductName} ({quantity} available)";
        }
        
        #endregion
    }
}
