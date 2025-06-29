using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Clean, minimal customer for behavior tree testing
    /// </summary>
    public class SimpleTestCustomer : MonoBehaviour
    {
        [Header("Shopping Configuration")]
        [SerializeField] private float startingMoney = 1000f;
        [SerializeField] private float shoppingTime = 10f;
        [SerializeField] private int maxProducts = 3;
        
        [Header("Current State")]
        public float currentMoney;
        public List<Product> selectedProducts = new List<Product>();
        public ShelfSlot currentTargetShelf;
        public Vector3 spawnPosition;
        public float shoppingStartTime;
        
        [Header("Debug")]
        public bool showDebugLogs = true;
        
        // Unity components (these are just references, not logic)
        public NavMeshAgent NavAgent { get; private set; }
        
        // Public properties for configuration access
        public float StartingMoney => startingMoney;
        public float ShoppingTime => shoppingTime;
        public int MaxProducts => maxProducts;
        
        private void Awake()
        {
            NavAgent = GetComponent<NavMeshAgent>();
            currentMoney = startingMoney;
            spawnPosition = transform.position;
            
            if (showDebugLogs)
                Debug.Log($"Customer initialized with ${currentMoney}");
        }
        
        // Simple utility methods that don't contain business logic
        public void StartShoppingTimer()
        {
            shoppingStartTime = Time.time;
        }
        
        public void AddProduct(Product product)
        {
            selectedProducts.Add(product);
            currentMoney -= product.CurrentPrice;
        }
        
        public void CleanupOnDestroy()
        {
            if (showDebugLogs)
                Debug.Log($"Customer cleanup - Selected {selectedProducts.Count} products, Spent ${startingMoney - currentMoney:F2}");
            
            foreach (Product product in selectedProducts)
            {
                if (product != null && !product.IsPurchased)
                    Destroy(product.gameObject);
            }
            
            selectedProducts.Clear();
        }
    }
}