# Module 2: Advanced C# Patterns
## Object-Oriented Design and Unity-Specific Patterns

### 📋 Module Overview
**Focus**: Advanced C# programming patterns applied in Unity game development  
**Project Context**: Customer system architecture, component composition, and event-driven systems  
**Prerequisites**: Module 1 complete, solid understanding of C# basics (classes, inheritance, interfaces)

---

## 🎯 Module Learning Objectives
By completing this module, you will be able to:
- [ ] Apply interface segregation principles in Unity component design
- [ ] Implement robust component composition patterns for complex game objects
- [ ] Design and implement event-driven architectures for loose coupling
- [ ] Use ScriptableObjects effectively for data management and configuration
- [ ] Understand when and how to apply various design patterns in Unity context

---

## 📚 Lesson Breakdown

### Lesson 2.1: Interface Segregation in Unity 🔌
**Difficulty**: 🟡 **Duration**: 60 minutes

#### Core Concepts
- **Interface Design Principles**: Small, focused interfaces vs monolithic ones
- **Unity-Specific Applications**: Component interfaces, system boundaries
- **Real Project Examples**: ICustomer, ICustomerBehavior, ICustomerMovement interfaces

#### Project Context
Our customer system evolved from a monolithic Customer class to a composition of specialized components, each implementing focused interfaces.

#### Key Learning Outcomes
- Design interfaces that follow single responsibility principle
- Understand how interface segregation improves testability and maintainability
- Implement Unity component interfaces that promote loose coupling

#### Files Referenced
- `Assets/Scripts/Interfaces/ICustomer.cs` - Core customer interface design
- `Assets/Scripts/Interfaces/ICustomerBehavior.cs` - Behavior-specific interface
- `Assets/Scripts/3 - Systems/AI/Customer/Core/Customer.cs` - Interface implementation

---

### Lesson 2.2: Component Composition Patterns 🧩
**Difficulty**: 🟡 **Duration**: 75 minutes

#### Core Concepts
- **Composition over Inheritance**: Why composition leads to more flexible designs
- **Component Communication**: How Unity components coordinate without tight coupling
- **Lifecycle Management**: Ensuring proper initialization order and cleanup

#### Project Context
Customer entities consist of CustomerBehavior, CustomerMovement, CustomerVisuals, and CustomerEconomics components that work together seamlessly.

#### Key Learning Outcomes
- Structure complex GameObjects using composition principles
- Implement reliable component initialization patterns
- Design component APIs that promote loose coupling

#### Files Referenced
- `Assets/Scripts/3 - Systems/AI/Customer/Core/Customer.cs` - Component coordinator
- `Assets/Scripts/3 - Systems/AI/Customer/Core/CustomerBehavior.cs` - Behavior management
- `Assets/Scripts/3 - Systems/AI/Customer/Movement/CustomerMovement.cs` - Movement handling

---

### Lesson 2.3: Event-Driven Architecture 📡
**Difficulty**: 🔴 **Duration**: 90 minutes

#### Core Concepts
- **Event Systems**: C# events, Unity Events, and custom event patterns
- **Observer Pattern**: Implementing loose coupling through event observation
- **Performance Considerations**: Event system optimization and memory management

#### Project Context
Product purchase events, customer state changes, and UI updates are coordinated through event-driven patterns without direct coupling.

#### Key Learning Outcomes
- Design robust event systems for game architecture
- Implement the Observer pattern using C# events and UnityEvents
- Handle event subscription/unsubscription to prevent memory leaks

#### Files Referenced
- `Assets/Scripts/2 - Entities/Products/Interaction/ProductInteraction.cs` - Event publishing
- `Assets/Scripts/3 - Systems/AI/Customer/Core/CustomerBehavior.cs` - Event handling
- `Assets/Scripts/4 - UI/ShopUI/ShopUI.cs` - UI event integration

---

### Lesson 2.4: ScriptableObject Data Management 📊
**Difficulty**: 🟡 **Duration**: 60 minutes

#### Core Concepts
- **Data-Driven Design**: Separating configuration from code logic
- **ScriptableObject Patterns**: Configuration objects, event channels, runtime sets
- **Asset Management**: Creating and managing ScriptableObject assets

#### Project Context
Product data, customer configuration, and game settings are managed through ScriptableObjects for flexibility and designer accessibility.

#### Key Learning Outcomes
- Create effective ScriptableObject architectures for game data
- Implement configuration systems that support runtime changes
- Use ScriptableObjects for communication between systems

#### Files Referenced
- `Assets/Scripts/0 - ScriptableObjects/ProductData.cs` - Product configuration data
- `Assets/Scripts/0 - ScriptableObjects/CustomerConfig.cs` - Customer behavior settings
- Various product data assets in `Assets/Resources/Data/Products/`

---

## 🔗 Module Integration

### Cross-Lesson Connections
- **Lesson 2.1 → 2.2**: Interfaces designed in 2.1 are implemented in composition patterns in 2.2
- **Lesson 2.2 → 2.3**: Component composition enables event-driven communication patterns
- **Lesson 2.3 → 2.4**: Events can be triggered by ScriptableObject state changes
- **All Lessons → Module 3**: These patterns provide foundation for larger system architecture

### Real Project Evolution
Our customer system demonstrates the progression:
1. **Monolithic Design**: Single Customer class handling everything
2. **Interface Introduction**: Separated concerns with focused interfaces
3. **Component Composition**: Broke monolith into specialized components
4. **Event Integration**: Added loose coupling through events
5. **Data-Driven Configuration**: Externalized behavior through ScriptableObjects

---

## 💻 Practical Applications

### Code Pattern Examples

#### Interface Segregation
```csharp
// Instead of one large interface
public interface IBadCustomer
{
    void Move(Vector3 destination);
    void SelectProduct(Product product);
    void UpdateVisuals();
    void ProcessPayment();
    void HandleAudio();
    // ... 20+ methods
}

// Use focused interfaces
public interface ICustomerMovement
{
    bool MoveToPosition(Vector3 position);
    bool HasReachedDestination();
}

public interface ICustomerBehavior
{
    void StartShopping();
    void SelectProduct(Product product);
}
```

#### Component Composition
```csharp
public class Customer : MonoBehaviour
{
    // Composed of specialized components
    public CustomerMovement Movement { get; private set; }
    public CustomerBehavior Behavior { get; private set; }
    public CustomerVisuals Visuals { get; private set; }
    
    private void Awake()
    {
        // Initialize composition
        Movement = GetComponent<CustomerMovement>();
        Behavior = GetComponent<CustomerBehavior>();
        Visuals = GetComponent<CustomerVisuals>();
    }
}
```

#### Event-Driven Communication
```csharp
public class ProductInteraction : MonoBehaviour
{
    public System.Action<Product> OnProductPurchased;
    
    public void PurchaseProduct()
    {
        // Notify all interested systems
        OnProductPurchased?.Invoke(product);
    }
}
```

---

## 🛠️ Module Exercises

### Progressive Exercise Series
Each lesson builds on the previous, culminating in a complete system implementation:

1. **Lesson 2.1 Exercise**: Design interfaces for a shop inventory system
2. **Lesson 2.2 Exercise**: Implement inventory using component composition
3. **Lesson 2.3 Exercise**: Add event-driven notifications for inventory changes  
4. **Lesson 2.4 Exercise**: Create ScriptableObject configuration for inventory rules

### Final Module Project
**Build a Complete Shop Management System** using all patterns learned:
- Interface-driven design for modularity
- Component composition for flexibility
- Event-driven updates for UI and analytics
- ScriptableObject configuration for game balance

---

## 📈 Assessment Criteria

### Knowledge Assessment
- [ ] Can identify when to use each design pattern
- [ ] Understands trade-offs between different architectural approaches
- [ ] Recognizes code smells and anti-patterns in Unity projects

### Practical Skills
- [ ] Implements clean, maintainable component architectures
- [ ] Creates effective event systems without memory leaks
- [ ] Designs ScriptableObject systems for data management
- [ ] Applies interface segregation principles consistently

### Code Quality
- [ ] Writes code that follows SOLID principles
- [ ] Creates systems that are easy to test and extend
- [ ] Demonstrates understanding of Unity-specific patterns

---

## 🔗 Connections to Other Modules

### From Module 1 (Behavior Designer)
- Behavior tree tasks benefit from interface-driven design
- Component composition enables complex AI behaviors
- Events coordinate between AI and game systems

### To Module 3 (Unity Component Architecture)
- Advanced patterns provide foundation for larger systems
- Component composition scales to system-level architecture
- Event patterns enable system-to-system communication

### Integration with Project
- All patterns are demonstrated in working game systems
- Real performance and maintainability benefits are visible
- Evolution of codebase shows practical application of patterns

---

## 📚 Additional Resources

### Design Pattern References
- **SOLID Principles**: [Microsoft Documentation](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- **Component Pattern**: [Game Programming Patterns](https://gameprogrammingpatterns.com/component.html)
- **Observer Pattern**: [Unity Event System](https://docs.unity3d.com/Manual/UnityEvents.html)

### Unity-Specific Resources
- [Unity Architecture Best Practices](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html)
- [ScriptableObject Documentation](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [Component Communication](https://docs.unity3d.com/Manual/ComponentInteraction.html)

### Advanced Reading
- **Clean Architecture**: Robert Martin's principles applied to game development
- **Unity DOTS**: Entity Component System architecture
- **Functional Programming**: F# patterns applicable to C# game development

---

*Module Version: 1.0*  
*Last Updated: December 29, 2025*  
*Author: Claude with Project Context*