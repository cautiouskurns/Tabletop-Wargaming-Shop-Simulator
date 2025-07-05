using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Manages UI canvas layering and sorting orders to prevent interaction conflicts
    /// </summary>
    public class UILayerManager : MonoBehaviour
    {
        public static UILayerManager Instance { get; private set; }
        
        [Header("UI Layer Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // UI Layer Constants - defines the hierarchy of UI elements
        public static class UILayers
        {
            public const int Background = 0;       // Background UI elements
            public const int Game = 10;            // In-game UI (crosshair, etc.)
            public const int Inventory = 20;       // Inventory and shop UI
            public const int Dialogue = 30;        // Dialogue system
            public const int Lore = 40;            // Lore terminal and codex
            public const int Pause = 50;           // Pause menu and system UI
            public const int Overlay = 60;         // Top-level overlays and notifications
        }
        
        private Dictionary<Canvas, UICanvasInfo> registeredCanvases = new Dictionary<Canvas, UICanvasInfo>();
        
        /// <summary>
        /// Information about a registered canvas
        /// </summary>
        private class UICanvasInfo
        {
            public string name;
            public int layerOrder;
            public bool blocksRaycast;
            public CanvasGroup canvasGroup;
            
            public UICanvasInfo(string name, int layerOrder, bool blocksRaycast, CanvasGroup canvasGroup)
            {
                this.name = name;
                this.layerOrder = layerOrder;
                this.blocksRaycast = blocksRaycast;
                this.canvasGroup = canvasGroup;
            }
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (enableDebugLogs)
                    Debug.Log("[UILayerManager] Initialized as singleton");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("[UILayerManager] Duplicate instance destroyed");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Auto-register any canvases that are already in the scene
            AutoRegisterCanvases();
        }
        
        #endregion
        
        #region Canvas Registration
        
        /// <summary>
        /// Register a canvas with the UI layer manager
        /// </summary>
        /// <param name="canvas">Canvas to register</param>
        /// <param name="uiName">Name identifier for the UI</param>
        /// <param name="layerOrder">Layer order from UILayers constants</param>
        /// <param name="blocksRaycast">Whether this UI should block raycast when active</param>
        public void RegisterCanvas(Canvas canvas, string uiName, int layerOrder, bool blocksRaycast = true)
        {
            if (canvas == null)
            {
                Debug.LogError($"[UILayerManager] Cannot register null canvas for {uiName}");
                return;
            }
            
            // Ensure canvas has a CanvasGroup for raycast blocking control
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                if (enableDebugLogs)
                    Debug.Log($"[UILayerManager] Added CanvasGroup to {uiName}");
            }
            
            // Set canvas sorting order
            canvas.sortingOrder = layerOrder;
            
            // Set initial raycast blocking state
            canvasGroup.blocksRaycasts = blocksRaycast && canvas.gameObject.activeInHierarchy;
            
            // Register the canvas
            UICanvasInfo info = new UICanvasInfo(uiName, layerOrder, blocksRaycast, canvasGroup);
            registeredCanvases[canvas] = info;
            
            if (enableDebugLogs)
                Debug.Log($"[UILayerManager] Registered {uiName} canvas with layer order {layerOrder}, blocks raycast: {blocksRaycast}");
        }
        
        /// <summary>
        /// Unregister a canvas from the UI layer manager
        /// </summary>
        /// <param name="canvas">Canvas to unregister</param>
        public void UnregisterCanvas(Canvas canvas)
        {
            if (canvas != null && registeredCanvases.ContainsKey(canvas))
            {
                string name = registeredCanvases[canvas].name;
                registeredCanvases.Remove(canvas);
                
                if (enableDebugLogs)
                    Debug.Log($"[UILayerManager] Unregistered {name} canvas");
            }
        }
        
        /// <summary>
        /// Auto-register canvases that are already in the scene
        /// </summary>
        private void AutoRegisterCanvases()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            
            foreach (Canvas canvas in canvases)
            {
                // Skip already registered canvases
                if (registeredCanvases.ContainsKey(canvas))
                    continue;
                
                // Try to determine layer based on canvas name
                string canvasName = canvas.name.ToLower();
                int layerOrder = UILayers.Game; // default
                bool blocksRaycast = true;
                
                if (canvasName.Contains("lore") || canvasName.Contains("codex"))
                {
                    layerOrder = UILayers.Lore;
                }
                else if (canvasName.Contains("dialogue") || canvasName.Contains("conversation"))
                {
                    layerOrder = UILayers.Dialogue;
                }
                else if (canvasName.Contains("inventory") || canvasName.Contains("shop"))
                {
                    layerOrder = UILayers.Inventory;
                }
                else if (canvasName.Contains("pause") || canvasName.Contains("menu"))
                {
                    layerOrder = UILayers.Pause;
                }
                else if (canvasName.Contains("crosshair") || canvasName.Contains("hud"))
                {
                    layerOrder = UILayers.Game;
                    blocksRaycast = false; // HUD elements shouldn't block interactions
                }
                
                RegisterCanvas(canvas, canvas.name, layerOrder, blocksRaycast);
            }
        }
        
        #endregion
        
        #region Canvas Control
        
        /// <summary>
        /// Set whether a canvas blocks raycast
        /// </summary>
        /// <param name="canvas">Canvas to control</param>
        /// <param name="blocksRaycast">Whether it should block raycast</param>
        public void SetCanvasRaycastBlocking(Canvas canvas, bool blocksRaycast)
        {
            if (canvas != null && registeredCanvases.ContainsKey(canvas))
            {
                UICanvasInfo info = registeredCanvases[canvas];
                info.canvasGroup.blocksRaycasts = blocksRaycast;
                
                if (enableDebugLogs)
                    Debug.Log($"[UILayerManager] Set {info.name} raycast blocking: {blocksRaycast}");
            }
        }
        
        /// <summary>
        /// Update raycast blocking for all registered canvases based on their active state
        /// </summary>
        public void UpdateRaycastBlocking()
        {
            foreach (var kvp in registeredCanvases)
            {
                Canvas canvas = kvp.Key;
                UICanvasInfo info = kvp.Value;
                
                if (canvas != null && info.canvasGroup != null)
                {
                    // Only block raycast if the canvas is active and should block
                    bool shouldBlock = canvas.gameObject.activeInHierarchy && info.blocksRaycast;
                    info.canvasGroup.blocksRaycasts = shouldBlock;
                }
            }
        }
        
        /// <summary>
        /// Get the highest active UI layer
        /// </summary>
        /// <returns>Highest layer order of currently active UI</returns>
        public int GetHighestActiveLayer()
        {
            int highest = UILayers.Background;
            
            foreach (var kvp in registeredCanvases)
            {
                Canvas canvas = kvp.Key;
                UICanvasInfo info = kvp.Value;
                
                if (canvas != null && canvas.gameObject.activeInHierarchy && info.layerOrder > highest)
                {
                    highest = info.layerOrder;
                }
            }
            
            return highest;
        }
        
        /// <summary>
        /// Check if a specific UI layer is currently active
        /// </summary>
        /// <param name="layerOrder">Layer order to check</param>
        /// <returns>True if any UI at this layer is active</returns>
        public bool IsLayerActive(int layerOrder)
        {
            foreach (var kvp in registeredCanvases)
            {
                Canvas canvas = kvp.Key;
                UICanvasInfo info = kvp.Value;
                
                if (canvas != null && canvas.gameObject.activeInHierarchy && info.layerOrder == layerOrder)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Debug method to show all registered canvases
        /// </summary>
        [ContextMenu("Debug: Show Registered Canvases")]
        public void DebugShowRegisteredCanvases()
        {
            Debug.Log("=== REGISTERED CANVASES ===");
            
            foreach (var kvp in registeredCanvases)
            {
                Canvas canvas = kvp.Key;
                UICanvasInfo info = kvp.Value;
                
                if (canvas != null)
                {
                    string status = canvas.gameObject.activeInHierarchy ? "ACTIVE" : "INACTIVE";
                    string raycast = info.canvasGroup.blocksRaycasts ? "BLOCKS" : "ALLOWS";
                    Debug.Log($"  {info.name}: Layer {info.layerOrder}, {status}, {raycast} raycast");
                }
                else
                {
                    Debug.Log($"  {info.name}: DESTROYED");
                }
            }
            
            Debug.Log($"Highest active layer: {GetHighestActiveLayer()}");
            Debug.Log("===========================");
        }
        
        #endregion
    }
}