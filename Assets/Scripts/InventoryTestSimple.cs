using UnityEngine;
using UnityEngine.UI;
using TabletopShop;

/// <summary>
/// Simple script to test and validate the InventoryManager
/// Attach to any GameObject in the scene to test the inventory system
/// </summary>
public class InventoryTestSimple : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool logDetailedInfo = true;
    [SerializeField] private bool showOnScreenInventory = true;
    [SerializeField] private KeyCode toggleInventoryKey = KeyCode.Tab;
    
    [Header("UI Components")]
    private Canvas inventoryCanvas;
    private Text inventoryText;
    private bool isInventoryVisible = true;
    
    void Start()
    {
        if (showOnScreenInventory)
        {
            CreateInventoryUI();
        }
        
        if (runTestOnStart)
        {
            // Wait a frame to ensure InventoryManager is initialized
            Invoke(nameof(RunInventoryTest), 0.1f);
        }
    }
    
    void Update()
    {
        // Toggle inventory display
        if (Input.GetKeyDown(toggleInventoryKey))
        {
            ToggleInventoryDisplay();
        }
        
        // Update inventory display if visible
        if (showOnScreenInventory && isInventoryVisible && inventoryText != null)
        {
            UpdateInventoryDisplay();
        }
    }
    
    [ContextMenu("Run Inventory Test")]
    public void RunInventoryTest()
    {
        Debug.Log("=== INVENTORY SYSTEM TEST ===");
        
        // Test 1: Get the InventoryManager instance
        var inventory = InventoryManager.Instance;
        Debug.Log($"✓ InventoryManager instance created: {inventory != null}");
        
        // Test 2: Check if inventory is initialized
        Debug.Log($"✓ Total products in system: {inventory.TotalProductCount}");
        Debug.Log($"✓ Unique product types: {inventory.UniqueProductCount}");
        Debug.Log($"✓ Available products list: {inventory.AvailableProducts.Count}");
        
        // Test 3: Display current inventory status
        if (logDetailedInfo)
        {
            inventory.LogInventoryStatus();
        }
        
        // Test 4: Test basic operations (only if we have products)
        if (inventory.AvailableProducts.Count > 0)
        {
            var testProduct = inventory.AvailableProducts[0];
            if (testProduct != null)
            {
                Debug.Log($"✓ Testing with product: {testProduct.ProductName}");
                
                // Test HasProduct
                int currentCount = inventory.GetProductCount(testProduct);
                Debug.Log($"✓ Current count of {testProduct.ProductName}: {currentCount}");
                
                // Test AddProduct
                inventory.AddProduct(testProduct, 2);
                Debug.Log($"✓ Added 2, new count: {inventory.GetProductCount(testProduct)}");
                
                // Test RemoveProduct
                bool removed = inventory.RemoveProduct(testProduct, 1);
                Debug.Log($"✓ Removed 1: {removed}, new count: {inventory.GetProductCount(testProduct)}");
                
                // Test SelectProduct
                bool selected = inventory.SelectProduct(testProduct);
                Debug.Log($"✓ Selected product: {selected}, current selection: {inventory.SelectedProduct?.ProductName ?? "None"}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️  No products found. Create ProductData assets in Resources/Products/ folder.");
        }
        
        // Test 5: Validate inventory state
        bool isValid = inventory.ValidateInventory();
        Debug.Log($"✓ Inventory validation: {(isValid ? "PASSED" : "FAILED")}");
        
        Debug.Log("=== TEST COMPLETE ===");
    }
    
    [ContextMenu("Show Inventory Status")]
    public void ShowInventoryStatus()
    {
        InventoryManager.Instance.LogInventoryStatus();
    }
    
    [ContextMenu("Reset Inventory")]
    public void ResetInventory()
    {
        InventoryManager.Instance.ResetInventory();
        Debug.Log("Inventory reset to default state.");
    }
    
    #region On-Screen Inventory Display
    
    /// <summary>
    /// Create the on-screen inventory UI
    /// </summary>
    private void CreateInventoryUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("InventoryCanvas");
        inventoryCanvas = canvasGO.AddComponent<Canvas>();
        inventoryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        inventoryCanvas.sortingOrder = 100;
        
        // Add CanvasScaler for responsive design
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create background panel
        GameObject panelGO = new GameObject("InventoryPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.7f);
        panelRect.anchorMax = new Vector2(0.4f, 1f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create text for inventory display
        GameObject textGO = new GameObject("InventoryText");
        textGO.transform.SetParent(panelGO.transform, false);
        
        inventoryText = textGO.AddComponent<Text>();
        inventoryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inventoryText.fontSize = 16;
        inventoryText.color = Color.white;
        inventoryText.alignment = TextAnchor.UpperLeft;
        inventoryText.text = "Loading inventory...";
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        Debug.Log("✓ On-screen inventory UI created. Press TAB to toggle.");
    }
    
    /// <summary>
    /// Update the on-screen inventory display
    /// </summary>
    private void UpdateInventoryDisplay()
    {
        if (inventoryText == null) return;
        
        var inventory = InventoryManager.Instance;
        
        string displayText = "=== INVENTORY ===\n";
        displayText += $"Total Items: {inventory.TotalProductCount}\n";
        displayText += $"Unique Products: {inventory.UniqueProductCount}\n\n";
        
        if (inventory.SelectedProduct != null)
        {
            displayText += $"SELECTED: {inventory.SelectedProduct.ProductName}\n\n";
        }
        else
        {
            displayText += "SELECTED: None\n\n";
        }
        
        if (inventory.TotalProductCount > 0)
        {
            displayText += "PRODUCTS:\n";
            foreach (var product in inventory.AvailableProducts)
            {
                if (product != null)
                {
                    int count = inventory.GetProductCount(product);
                    if (count > 0)
                    {
                        string indicator = product == inventory.SelectedProduct ? " [SELECTED]" : "";
                        displayText += $"• {product.ProductName}: {count}{indicator}\n";
                    }
                }
            }
        }
        else
        {
            displayText += "No products in inventory\n";
            displayText += "Create ProductData assets in\nResources/Products/ folder";
        }
        
        displayText += $"\nPress {toggleInventoryKey} to toggle this display";
        
        inventoryText.text = displayText;
    }
    
    /// <summary>
    /// Toggle the inventory display on/off
    /// </summary>
    private void ToggleInventoryDisplay()
    {
        isInventoryVisible = !isInventoryVisible;
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(isInventoryVisible);
        }
        
        Debug.Log($"Inventory display: {(isInventoryVisible ? "SHOWN" : "HIDDEN")}");
    }
    
    /// <summary>
    /// Show the inventory display
    /// </summary>
    [ContextMenu("Show Inventory Display")]
    public void ShowInventoryDisplay()
    {
        if (!showOnScreenInventory)
        {
            showOnScreenInventory = true;
            CreateInventoryUI();
        }
        
        isInventoryVisible = true;
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the inventory display
    /// </summary>
    [ContextMenu("Hide Inventory Display")]
    public void HideInventoryDisplay()
    {
        isInventoryVisible = false;
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }
    }
    
    #endregion
}
