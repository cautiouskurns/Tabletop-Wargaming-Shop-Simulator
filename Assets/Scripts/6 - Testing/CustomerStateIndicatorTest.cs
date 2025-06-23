using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple test script for the customer state indicator system.
    /// Attach this to a customer to cycle through states and test the visual indicators.
    /// </summary>
    public class CustomerStateIndicatorTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableAutoTest = true;
        [SerializeField] private float stateChangeInterval = 3f;
        [SerializeField] private KeyCode manualTestKey = KeyCode.Space;
        
        private Customer customer;
        private CustomerState[] testStates = { 
            CustomerState.Entering, 
            CustomerState.Shopping, 
            CustomerState.Purchasing, 
            CustomerState.Leaving 
        };
        private int currentTestStateIndex = 0;
        private float lastStateChangeTime = 0f;
        
        private void Start()
        {
            customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError($"CustomerStateIndicatorTest: No Customer component found on {name}");
                enabled = false;
                return;
            }
            
            Debug.Log($"CustomerStateIndicatorTest: Initialized on {name}. " +
                     $"Auto test: {enableAutoTest}, Manual key: {manualTestKey}");
        }
        
        private void Update()
        {
            // Auto test - cycle through states automatically
            if (enableAutoTest && Time.time - lastStateChangeTime >= stateChangeInterval)
            {
                CycleToNextState();
            }
            
            // Manual test - press key to cycle states
            if (Input.GetKeyDown(manualTestKey))
            {
                CycleToNextState();
            }
        }
        
        /// <summary>
        /// Cycle to the next test state
        /// </summary>
        private void CycleToNextState()
        {
            if (customer?.Behavior == null) return;
            
            CustomerState newState = testStates[currentTestStateIndex];
            currentTestStateIndex = (currentTestStateIndex + 1) % testStates.Length;
            
            Debug.Log($"CustomerStateIndicatorTest: Changing {name} to state {newState}");
            customer.Behavior.ChangeState(newState);
            
            lastStateChangeTime = Time.time;
        }
        
        /// <summary>
        /// Set a specific state (useful for inspector testing)
        /// </summary>
        /// <param name="state">State to set</param>
        [ContextMenu("Set Entering State")]
        public void SetEnteringState() => SetState(CustomerState.Entering);
        
        [ContextMenu("Set Shopping State")]
        public void SetShoppingState() => SetState(CustomerState.Shopping);
        
        [ContextMenu("Set Purchasing State")]
        public void SetPurchasingState() => SetState(CustomerState.Purchasing);
        
        [ContextMenu("Set Leaving State")]
        public void SetLeavingState() => SetState(CustomerState.Leaving);
        
        private void SetState(CustomerState state)
        {
            if (customer?.Behavior != null)
            {
                customer.Behavior.ChangeState(state);
                Debug.Log($"CustomerStateIndicatorTest: Set {name} to state {state}");
            }
        }
    }
}
