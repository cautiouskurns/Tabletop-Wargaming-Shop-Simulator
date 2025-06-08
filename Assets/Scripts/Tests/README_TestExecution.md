# GameManager Economic System Tests - Execution Guide

## Test Suite Overview

The GameManager economic system tests are implemented using Unity Test Framework with comprehensive coverage of:

### 1. Money Operations Tests
- `AddMoney_ValidAmount_IncreasesCurrentMoney()` - Basic money addition
- `AddMoney_NegativeAmount_DoesNotChangeMoney()` - Invalid input handling
- `AddMoney_ZeroAmount_DoesNotChangeMoney()` - Zero amount edge case
- `SubtractMoney_SufficientFunds_DecreasesMoneyAndReturnsTrue()` - Successful subtraction
- `SubtractMoney_InsufficientFunds_DoesNotChangeMoneyAndReturnsFalse()` - Insufficient funds
- `SubtractMoney_NegativeAmount_DoesNotChangeMoney()` - Negative amount handling
- `HasSufficientFunds_VariousAmounts_ReturnsCorrectResults()` - Fund checking validation

### 2. Customer Purchase Processing Tests
- `ProcessCustomerPurchase_NormalSatisfaction_UpdatesAllMetrics()` - Standard purchase flow
- `ProcessCustomerPurchase_LowSatisfaction_DecreasesReputation()` - Low satisfaction impact
- `ProcessCustomerPurchase_NeutralSatisfaction_MinimalReputationChange()` - Neutral satisfaction
- `ProcessCustomerPurchase_ZeroAmount_DoesNotUpdateMetrics()` - Zero purchase edge case

### 3. Day Transition and Daily Expense Tests
- `DayTransition_ProcessesDailyExpenses()` - Daily expense processing (UnityTest)
- `DayNightCycle_TogglesCorrectly()` - Day/night state management (UnityTest)

### 4. Customer Limits and Reputation Tests
- `CustomerLimits_TrackingWorksCorrectly()` - Customer count tracking
- `ReputationSystem_BoundaryConditions()` - Reputation 0-100 boundaries
- `ReputationSystem_SatisfactionImpact()` - Satisfaction impact calculations

### 5. Performance Tests
- `PerformanceTest_RapidTransactions()` - 1000 rapid transactions (UnityTest)
- `PerformanceTest_RapidCustomerPurchases()` - 500 rapid customer purchases (UnityTest)

### 6. Edge Case Tests
- `EdgeCase_ExtremeValues()` - Maximum float value handling
- `EdgeCase_FloatingPointPrecision()` - Decimal precision testing
- `EdgeCase_RapidStateChanges()` - State consistency under load

### 7. Integration and Error Handling Tests
- `Integration_EconomicStatusConsistency()` - Data integrity verification
- `Integration_EconomyReset()` - Reset functionality completeness
- `ErrorHandling_InvalidSourceStrings()` - Null/empty string handling
- `ErrorHandling_ExtremeSatisfactionValues()` - Satisfaction boundary testing

## How to Run Tests

### Option 1: Unity Test Runner Window (Recommended)
1. Open Unity Editor with the project
2. Go to **Window > General > Test Runner**
3. Click the **PlayMode** tab in the Test Runner window
4. Click **Run All** to execute all tests
5. Individual tests can be run by clicking the play button next to each test

### Option 2: Unity Test Runner Command Line (when Unity is closed)
```bash
cd "/Users/diarmuidcurran/Unity Projects/Tabletop-Wargaming-Shop-Simulator"
/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath . \
  -runTests -testResults TestResults.xml \
  -testPlatform PlayMode
```

### Option 3: Unity Test Runner Edit Mode
1. In Test Runner window, click **EditMode** tab
2. Click **Run All** for immediate execution without Play mode

## Test Architecture

### Test Isolation
- Each test uses `[SetUp]` to create a fresh GameManager instance
- `[TearDown]` ensures proper cleanup between tests
- No shared state between tests prevents interference

### Test Patterns
- **[Test]** - Simple unit tests that complete immediately
- **[UnityTest]** - Frame-based tests using coroutines for timing-dependent operations
- **Assert.AreEqual** with tolerance for float comparisons
- **Assert.IsTrue/IsFalse** for boolean validations
- **Performance assertions** with time limits

### Expected Results
- All 25+ tests should pass
- Performance tests should complete under specified time limits
- No compilation errors or warnings
- Complete coverage of GameManager economic API

## Files
- Test Implementation: `Assets/Scripts/Tests/Runtime/GameManagerTests.cs`
- Assembly Definition: `Assets/Scripts/Tests/Runtime/TabletopShop.Tests.asmdef`
- Target System: `Assets/Scripts/Core/GameManager.cs`

## Test Coverage Summary
✅ Money Operations (AddMoney, SubtractMoney, HasSufficientFunds)
✅ Customer Purchase Processing with satisfaction levels
✅ Day transition logic and daily expense processing  
✅ Customer limits and reputation system calculations
✅ Performance tests for rapid transaction processing
✅ Edge case testing with negative values and extreme numbers
✅ Integration tests for state consistency
✅ Error handling for invalid inputs
