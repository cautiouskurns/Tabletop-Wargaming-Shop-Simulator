using UnityEngine;
using UnityEditor;

namespace TabletopShop
{
    #if UNITY_EDITOR
    /// <summary>
    /// Custom editor for ShelfSlot to help with migration and testing
    /// </summary>
    [CustomEditor(typeof(ShelfSlot))]
    public class ShelfSlotEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ShelfSlot shelfSlot = (ShelfSlot)target;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Composition Components", EditorStyles.boldLabel);
            
            // Show component status
            var logic = shelfSlot.GetComponent<ShelfSlotLogic>();
            var visuals = shelfSlot.GetComponent<ShelfSlotVisuals>();
            var interaction = shelfSlot.GetComponent<ShelfSlotInteraction>();
            
            EditorGUILayout.LabelField($"Logic Component: {(logic != null ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"Visuals Component: {(visuals != null ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"Interaction Component: {(interaction != null ? "✓" : "✗")}");
            
            EditorGUILayout.Space();
            
            // Runtime state information
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Is Empty: {shelfSlot.IsEmpty}");
                EditorGUILayout.LabelField($"Current Product: {(shelfSlot.CurrentProduct != null ? shelfSlot.CurrentProduct.name : "None")}");
                EditorGUILayout.LabelField($"Slot Position: {shelfSlot.SlotPosition}");
                EditorGUILayout.LabelField($"Can Interact: {shelfSlot.CanInteract}");
                EditorGUILayout.LabelField($"Interaction Text: {shelfSlot.InteractionText}");
            }
            
            EditorGUILayout.Space();
            
            // Migration tools
            EditorGUILayout.LabelField("Migration Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Force Re-Initialize Components"))
            {
                // This will trigger component re-initialization
                if (logic == null) shelfSlot.gameObject.AddComponent<ShelfSlotLogic>();
                if (visuals == null) shelfSlot.gameObject.AddComponent<ShelfSlotVisuals>();
                if (interaction == null) shelfSlot.gameObject.AddComponent<ShelfSlotInteraction>();
                
                EditorUtility.SetDirty(shelfSlot);
            }
            
            if (GUILayout.Button("Test Public API Compatibility"))
            {
                TestPublicAPI(shelfSlot);
            }
        }
        
        /// <summary>
        /// Test that all public API methods still work after refactoring
        /// </summary>
        private void TestPublicAPI(ShelfSlot shelfSlot)
        {
            Debug.Log("=== ShelfSlot Public API Test ===");
            
            // Test properties
            Debug.Log($"IsEmpty: {shelfSlot.IsEmpty}");
            Debug.Log($"CurrentProduct: {shelfSlot.CurrentProduct}");
            Debug.Log($"SlotPosition: {shelfSlot.SlotPosition}");
            Debug.Log($"InteractionText: {shelfSlot.InteractionText}");
            Debug.Log($"CanInteract: {shelfSlot.CanInteract}");
            
            // Test SetSlotPosition
            Vector3 originalPosition = shelfSlot.SlotPosition;
            shelfSlot.SetSlotPosition(Vector3.up);
            Debug.Log($"SetSlotPosition test - New position: {shelfSlot.SlotPosition}");
            
            // Test ClearSlot
            shelfSlot.ClearSlot();
            Debug.Log($"ClearSlot test - Is empty after clear: {shelfSlot.IsEmpty}");
            
            Debug.Log("=== Public API Test Complete ===");
        }
    }
    #endif
}
