using UnityEngine;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Simple shop window sign that displays OPEN/CLOSED status
    /// Can be placed on windows, doors, or anywhere in the shop
    /// </summary>
    public class ShopWindowSign : MonoBehaviour
    {
        [Header("Sign Configuration")]
        [SerializeField] private TextMeshPro signText;
        [SerializeField] private MeshRenderer signBackground;
        
        [Header("Sign Appearance")]
        [SerializeField] private string openText = "OPEN";
        [SerializeField] private string closedText = "CLOSED";
        
        [Header("Colors")]
        [SerializeField] private Color openTextColor = Color.green;
        [SerializeField] private Color closedTextColor = Color.red;
        [SerializeField] private Material openBackgroundMaterial;
        [SerializeField] private Material closedBackgroundMaterial;
        
        [Header("Auto-Connect")]
        [SerializeField] private bool autoFindStoreHours = true;
        
        // Component references
        private StoreHours storeHours;
        private bool isCurrentlyOpen = true;
        
        private void Start()
        {
            // Set up text component if not assigned
            if (signText == null)
                signText = GetComponentInChildren<TextMeshPro>();
            
            // Set up background renderer if not assigned
            if (signBackground == null)
                signBackground = GetComponent<MeshRenderer>();
            
            // Find StoreHours if auto-connect is enabled
            if (autoFindStoreHours && storeHours == null)
            {
                storeHours = FindFirstObjectByType<StoreHours>();
                if (storeHours != null)
                {
                    Debug.Log($"ShopWindowSign: Connected to StoreHours automatically");
                }
            }
            
            // Set initial sign state
            UpdateSignDisplay();
        }
        
        private void Update()
        {
            // Check for store status changes
            if (storeHours != null)
            {
                bool newStatus = storeHours.IsStoreOpen;
                if (newStatus != isCurrentlyOpen)
                {
                    isCurrentlyOpen = newStatus;
                    UpdateSignDisplay();
                }
            }
        }
        
        /// <summary>
        /// Update the sign display based on current store status
        /// </summary>
        private void UpdateSignDisplay()
        {
            bool isOpen = storeHours?.IsStoreOpen ?? true;
            
            // Update text
            if (signText != null)
            {
                signText.text = isOpen ? openText : closedText;
                signText.color = isOpen ? openTextColor : closedTextColor;
            }
            
            // Update background material
            if (signBackground != null)
            {
                if (isOpen && openBackgroundMaterial != null)
                {
                    signBackground.material = openBackgroundMaterial;
                }
                else if (!isOpen && closedBackgroundMaterial != null)
                {
                    signBackground.material = closedBackgroundMaterial;
                }
            }
            
            Debug.Log($"ShopWindowSign: Updated to show '{(isOpen ? openText : closedText)}'");
        }
        
        /// <summary>
        /// Manually set the store hours reference
        /// </summary>
        public void SetStoreHours(StoreHours newStoreHours)
        {
            storeHours = newStoreHours;
            UpdateSignDisplay();
        }
        
        /// <summary>
        /// Manually set the sign to open
        /// </summary>
        [ContextMenu("Show Open")]
        public void ShowOpen()
        {
            isCurrentlyOpen = true;
            if (signText != null)
            {
                signText.text = openText;
                signText.color = openTextColor;
            }
            if (signBackground != null && openBackgroundMaterial != null)
            {
                signBackground.material = openBackgroundMaterial;
            }
        }
        
        /// <summary>
        /// Manually set the sign to closed
        /// </summary>
        [ContextMenu("Show Closed")]
        public void ShowClosed()
        {
            isCurrentlyOpen = false;
            if (signText != null)
            {
                signText.text = closedText;
                signText.color = closedTextColor;
            }
            if (signBackground != null && closedBackgroundMaterial != null)
            {
                signBackground.material = closedBackgroundMaterial;
            }
        }
        
        /// <summary>
        /// Toggle between open and closed for testing
        /// </summary>
        [ContextMenu("Toggle Sign")]
        public void ToggleSign()
        {
            if (isCurrentlyOpen)
                ShowClosed();
            else
                ShowOpen();
        }
    }
}