using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Debug utility to help diagnose interaction system issues
    /// </summary>
    public class InteractionSystemDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugOutput = true;
        [SerializeField] private bool showDebugGUI = true;
        [SerializeField] private KeyCode debugKey = KeyCode.F1;
        
        [Header("Components to Check")]
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private CrosshairUI crosshairUI;
        [SerializeField] private Camera playerCamera;
        
        // Debug info
        private List<string> debugMessages = new List<string>();
        private Vector2 scrollPosition;
        private bool showDebugWindow = false;
        
        private void Update()
        {
            if (Input.GetKeyDown(debugKey))
            {
                showDebugWindow = !showDebugWindow;
                if (showDebugWindow)
                {
                    RunDiagnostics();
                }
            }
            
            if (enableDebugOutput)
            {
                DebugInteractionSystem();
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugGUI || !showDebugWindow) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 500, Screen.height - 20));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Interaction System Debugger", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
            GUILayout.Space(10);
            
            if (GUILayout.Button("Run Diagnostics"))
            {
                RunDiagnostics();
            }
            
            GUILayout.Space(10);
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Screen.height - 100));
            
            foreach (string message in debugMessages)
            {
                GUILayout.Label(message);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void RunDiagnostics()
        {
            debugMessages.Clear();
            debugMessages.Add("=== INTERACTION SYSTEM DIAGNOSTICS ===");
            debugMessages.Add($"Time: {System.DateTime.Now:HH:mm:ss}");
            debugMessages.Add("");
            
            // Check layers
            CheckLayers();
            debugMessages.Add("");
            
            // Check components
            CheckComponents();
            debugMessages.Add("");
            
            // Check interactable objects
            CheckInteractableObjects();
            debugMessages.Add("");
            
            // Check UI
            CheckUI();
            
            debugMessages.Add("");
            debugMessages.Add("=== END DIAGNOSTICS ===");
        }
        
        private void CheckLayers()
        {
            debugMessages.Add("--- LAYER CHECK ---");
            
            bool layersValid = InteractionLayers.ValidateLayers();
            debugMessages.Add($"Layers Valid: {(layersValid ? "✓" : "✗")}");
            
            debugMessages.Add($"Interactable Layer Index: {InteractionLayers.InteractableLayerIndex}");
            debugMessages.Add($"Product Layer Index: {InteractionLayers.ProductLayerIndex}");
            debugMessages.Add($"Shelf Layer Index: {InteractionLayers.ShelfLayerIndex}");
            
            // Check if objects are on correct layers
            Product[] products = FindObjectsOfType<Product>();
            ShelfSlot[] slots = FindObjectsOfType<ShelfSlot>();
            
            debugMessages.Add($"Products found: {products.Length}");
            foreach (var product in products)
            {
                debugMessages.Add($"  {product.name} - Layer: {LayerMask.LayerToName(product.gameObject.layer)} ({product.gameObject.layer})");
            }
            
            debugMessages.Add($"Shelf slots found: {slots.Length}");
            foreach (var slot in slots)
            {
                debugMessages.Add($"  {slot.name} - Layer: {LayerMask.LayerToName(slot.gameObject.layer)} ({slot.gameObject.layer})");
            }
        }
        
        private void CheckComponents()
        {
            debugMessages.Add("--- COMPONENT CHECK ---");
            
            // Auto-find components if not assigned
            if (playerInteraction == null)
                playerInteraction = FindObjectOfType<PlayerInteraction>();
            if (crosshairUI == null)
                crosshairUI = FindObjectOfType<CrosshairUI>();
            if (playerCamera == null)
                playerCamera = Camera.main ?? FindObjectOfType<Camera>();
            
            debugMessages.Add($"PlayerInteraction: {(playerInteraction != null ? "✓" : "✗")}");
            if (playerInteraction != null)
            {
                debugMessages.Add($"  GameObject: {playerInteraction.gameObject.name}");
                debugMessages.Add($"  Active: {playerInteraction.gameObject.activeInHierarchy}");
                debugMessages.Add($"  Enabled: {playerInteraction.enabled}");
            }
            
            debugMessages.Add($"CrosshairUI: {(crosshairUI != null ? "✓" : "✗")}");
            if (crosshairUI != null)
            {
                debugMessages.Add($"  GameObject: {crosshairUI.gameObject.name}");
                debugMessages.Add($"  Active: {crosshairUI.gameObject.activeInHierarchy}");
                debugMessages.Add($"  Enabled: {crosshairUI.enabled}");
            }
            
            debugMessages.Add($"Player Camera: {(playerCamera != null ? "✓" : "✗")}");
            if (playerCamera != null)
            {
                debugMessages.Add($"  GameObject: {playerCamera.gameObject.name}");
                debugMessages.Add($"  Active: {playerCamera.gameObject.activeInHierarchy}");
                debugMessages.Add($"  Enabled: {playerCamera.enabled}");
            }
        }
        
        private void CheckInteractableObjects()
        {
            debugMessages.Add("--- INTERACTABLE OBJECTS CHECK ---");
            
            IInteractable[] interactables = FindObjectsOfType<MonoBehaviour>() as IInteractable[];
            
            // Find all objects with IInteractable components
            List<IInteractable> foundInteractables = new List<IInteractable>();
            MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var obj in allObjects)
            {
                if (obj is IInteractable)
                {
                    foundInteractables.Add(obj as IInteractable);
                }
            }
            
            debugMessages.Add($"IInteractable objects found: {foundInteractables.Count}");
            
            foreach (var interactable in foundInteractables)
            {
                MonoBehaviour mono = interactable as MonoBehaviour;
                if (mono != null)
                {
                    debugMessages.Add($"  {mono.name}:");
                    debugMessages.Add($"    Type: {mono.GetType().Name}");
                    debugMessages.Add($"    Layer: {LayerMask.LayerToName(mono.gameObject.layer)}");
                    debugMessages.Add($"    Active: {mono.gameObject.activeInHierarchy}");
                    debugMessages.Add($"    CanInteract: {interactable.CanInteract}");
                    debugMessages.Add($"    InteractionText: {interactable.InteractionText}");
                    
                    // Check collider
                    Collider col = mono.GetComponent<Collider>();
                    debugMessages.Add($"    Collider: {(col != null ? "✓" : "✗")}");
                    if (col != null)
                    {
                        debugMessages.Add($"      Enabled: {col.enabled}");
                        debugMessages.Add($"      IsTrigger: {col.isTrigger}");
                    }
                }
            }
        }
        
        private void CheckUI()
        {
            debugMessages.Add("--- UI CHECK ---");
            
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            debugMessages.Add($"Canvases found: {canvases.Length}");
            
            foreach (var canvas in canvases)
            {
                debugMessages.Add($"  {canvas.name}:");
                debugMessages.Add($"    RenderMode: {canvas.renderMode}");
                debugMessages.Add($"    SortingOrder: {canvas.sortingOrder}");
                debugMessages.Add($"    Active: {canvas.gameObject.activeInHierarchy}");
            }
            
            if (crosshairUI != null)
            {
                debugMessages.Add("CrosshairUI Details:");
                
                // Check crosshair image
                Image crosshairImage = crosshairUI.GetComponentInChildren<Image>();
                debugMessages.Add($"  Crosshair Image: {(crosshairImage != null ? "✓" : "✗")}");
                if (crosshairImage != null)
                {
                    debugMessages.Add($"    Active: {crosshairImage.gameObject.activeInHierarchy}");
                    debugMessages.Add($"    Enabled: {crosshairImage.enabled}");
                    debugMessages.Add($"    Color: {crosshairImage.color}");
                }
                
                // Check interaction text
                Text interactionText = crosshairUI.GetComponentInChildren<Text>();
                debugMessages.Add($"  Interaction Text: {(interactionText != null ? "✓" : "✗")}");
                if (interactionText != null)
                {
                    debugMessages.Add($"    Active: {interactionText.gameObject.activeInHierarchy}");
                    debugMessages.Add($"    Text: '{interactionText.text}'");
                    debugMessages.Add($"    Color: {interactionText.color}");
                }
            }
        }
        
        private void DebugInteractionSystem()
        {
            if (playerInteraction == null || playerCamera == null) return;
            
            // Perform raycast debug
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            
            LayerMask interactableLayer = InteractionLayers.AllInteractablesMask;
            bool hitSomething = Physics.Raycast(ray, out hit, 3f, interactableLayer);
            
            if (hitSomething)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null)
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                
                if (interactable != null && enableDebugOutput)
                {
                    Debug.Log($"[InteractionDebug] Looking at: {hit.collider.name} - {interactable.InteractionText}");
                }
            }
        }
    }
}
