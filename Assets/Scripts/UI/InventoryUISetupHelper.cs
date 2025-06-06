using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Helper script to set up the InventoryUI with proper icons and fix common issues
    /// </summary>
    public class InventoryUISetupHelper : MonoBehaviour
    {
        [Header("Product Icons")]
        [SerializeField] private Sprite miniatureBoxIcon;
        [SerializeField] private Sprite paintPotIcon;
        [SerializeField] private Sprite rulebookIcon;
        
        [Header("References")]
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private Button[] productButtons;
        
        [ContextMenu("Auto Setup Icons")]
        public void AutoSetupIcons()
        {
            // Load icons from textures
            LoadIcons();
            
            // Setup button icons and names
            SetupButtons();
            
            Debug.Log("InventoryUI setup completed with icons and proper button configuration.");
        }
        
        private void LoadIcons()
        {
            // Load sprites from the Textures folder
            miniatureBoxIcon = Resources.Load<Sprite>("MiniatureBoxTexture");
            paintPotIcon = Resources.Load<Sprite>("PaintPotTexture");
            rulebookIcon = Resources.Load<Sprite>("RulebookTexture");
            
            // If not found in Resources, try direct asset loading
            if (miniatureBoxIcon == null)
                miniatureBoxIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/MiniatureBoxTexture.png");
            if (paintPotIcon == null)
                paintPotIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/PaintPotTexture.png");
            if (rulebookIcon == null)
                rulebookIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/RulebookTexture.png");
        }
        
        private void SetupButtons()
        {
            if (productButtons == null || productButtons.Length != 3)
            {
                Debug.LogError("Please assign exactly 3 product buttons in the array.");
                return;
            }
            
            // Setup each button
            SetupButton(0, miniatureBoxIcon, "Miniature Box", ProductType.MiniatureBox);
            SetupButton(1, paintPotIcon, "Paint Pot", ProductType.PaintPot);
            SetupButton(2, rulebookIcon, "Rulebook", ProductType.Rulebook);
        }
        
        private void SetupButton(int index, Sprite icon, string productName, ProductType productType)
        {
            if (index >= productButtons.Length || productButtons[index] == null)
                return;
                
            Button button = productButtons[index];
            
            // Find child components
            Image iconImage = button.transform.Find("ProductIcon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = button.transform.Find("ProductName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI countText = button.transform.Find("ProductCount")?.GetComponent<TextMeshProUGUI>();
            
            // Set icon
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
            }
            
            // Set name
            if (nameText != null)
            {
                nameText.text = productName;
            }
            
            // Set initial count
            if (countText != null)
            {
                countText.text = "0";
            }
            
            // Ensure button is properly configured
            if (button.GetComponent<Image>() == null)
            {
                button.gameObject.AddComponent<Image>();
            }
            
            Debug.Log($"Setup button {index}: {productName} with icon");
        }
        
        [ContextMenu("Find and Assign References")]
        public void FindAndAssignReferences()
        {
            // Find InventoryUI
            if (inventoryUI == null)
                inventoryUI = FindAnyObjectByType<InventoryUI>();
            
            // Find product buttons
            if (productButtons == null || productButtons.Length == 0)
            {
                Transform inventoryPanel = FindChildRecursively(transform.root, "InventoryPanel");
                if (inventoryPanel != null)
                {
                    productButtons = new Button[3];
                    for (int i = 0; i < 3; i++)
                    {
                        string buttonName = i == 0 ? "ProductButton" : $"ProductButton ({i})";
                        Transform buttonTransform = inventoryPanel.Find(buttonName);
                        if (buttonTransform != null)
                        {
                            productButtons[i] = buttonTransform.GetComponent<Button>();
                        }
                    }
                }
            }
            
            Debug.Log($"Found InventoryUI: {inventoryUI != null}");
            Debug.Log($"Found {productButtons?.Length ?? 0} product buttons");
        }
        
        private Transform FindChildRecursively(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                    return child;
                    
                Transform found = FindChildRecursively(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
