# Unity C# Learning Syllabus
## Tabletop Wargaming Shop Simulator - Educational Framework

### Version: 1.0
### Last Updated: December 29, 2025
### Project Context: Real-world Unity game development with Behavior Designer Pro

---

## 📚 Syllabus Overview

This comprehensive learning syllabus is built from actual Unity C# development work on the Tabletop Wargaming Shop Simulator project. Each lesson is grounded in real implementations, providing practical, applicable knowledge for Unity game development.

### 🎯 Learning Philosophy
- **Practice-First**: Every concept is demonstrated through working code from our project
- **Progressive Complexity**: Lessons build upon each other systematically
- **Real-World Context**: Solutions address actual game development challenges
- **Component-Based**: Emphasizes Unity's component architecture and best practices

---

## 🗺️ Learning Path Structure

### Prerequisites
- Basic C# knowledge (variables, methods, classes)
- Unity Editor familiarity (Inspector, Hierarchy, Project windows)
- Understanding of GameObject and Component concepts

### Difficulty Levels
- 🟢 **Beginner**: New to Unity/Game Development
- 🟡 **Intermediate**: Comfortable with Unity basics, learning advanced patterns
- 🔴 **Advanced**: Experienced developer learning specialized techniques

---

## 📖 Module Breakdown

### Module 1: Behavior Designer Integration 🤖
**Focus**: AI System Development with Visual Behavior Trees
**Project Context**: Customer AI system migration from state machines to Behavior Designer Pro

| Lesson | Topic | Difficulty | Duration | Status |
|--------|-------|------------|----------|---------|
| 1.1 | Understanding Action vs Conditional Tasks | 🟢 | 45 min | ✅ Complete |
| 1.2 | Customer AI State Management | 🟡 | 60 min | ✅ Complete |
| 1.3 | NavMesh Integration with Behavior Trees | 🟡 | 75 min | ✅ Complete |
| 1.4 | Complex Checkout Workflow Implementation | 🔴 | 90 min | ✅ Complete |
| 1.5 | Debugging and Testing Behavior Trees | 🟡 | 45 min | ✅ Complete |

### Module 2: Advanced C# Patterns 🔧
**Focus**: Object-Oriented Design and Unity-Specific Patterns
**Project Context**: Customer system architecture and component composition

| Lesson | Topic | Difficulty | Duration | Status |
|--------|-------|------------|----------|---------|
| 2.1 | Interface Segregation in Unity | 🟡 | 60 min | 📝 Planned |
| 2.2 | Component Composition Patterns | 🟡 | 75 min | 📝 Planned |
| 2.3 | Event-Driven Architecture | 🔴 | 90 min | 📝 Planned |
| 2.4 | ScriptableObject Data Management | 🟡 | 60 min | 📝 Planned |

### Module 3: Unity Component Architecture 🏗️
**Focus**: Building Maintainable, Scalable Unity Systems
**Project Context**: Product, Customer, and Shop systems integration

| Lesson | Topic | Difficulty | Duration | Status |
|--------|-------|------------|----------|---------|
| 3.1 | Single Responsibility Components | 🟢 | 45 min | 📝 Planned |
| 3.2 | Cross-Component Communication | 🟡 | 60 min | 📝 Planned |
| 3.3 | Manager Pattern Implementation | 🔴 | 75 min | 📝 Planned |
| 3.4 | Testing Component Systems | 🔴 | 90 min | 📝 Planned |

---

## 🔗 Project Integration

### File References
All lessons reference actual project files with specific line numbers:
- **Customer System**: `/Assets/Scripts/3 - Systems/AI/Customer/`
- **Behavior Designer Tasks**: `/Assets/Scripts/6 - Testing/Prototyping/`
- **Product System**: `/Assets/Scripts/2 - Entities/Products/`
- **Shop Management**: `/Assets/Scripts/2 - Entities/Shop/`

### Code Examples
Every lesson includes:
1. **Real Implementation**: Working code from the project
2. **Explained Concepts**: Why specific approaches were chosen
3. **Alternative Approaches**: Other ways to solve the same problem
4. **Common Pitfalls**: Mistakes to avoid based on our experience

---

## 📈 Progress Tracking

### Learning Objectives Completion
- [x] Module 1 Complete (5/5 lessons) ✅
  - [x] Lesson 1.1: Action vs Conditional Tasks ✅
  - [x] Lesson 1.2: Customer AI State Management ✅  
  - [x] Lesson 1.3: NavMesh Integration ✅
  - [x] Lesson 1.4: Complex Checkout Workflow ✅
  - [x] Lesson 1.5: Debugging and Testing ✅
- [ ] Module 2 Complete (0/4 lessons) 📋 Outlined
- [ ] Module 3 Complete (0/4 lessons) 📋 Outlined

### Practical Skills Assessment
- [ ] Can implement basic Behavior Designer tasks
- [ ] Understands Unity component communication patterns
- [ ] Can debug AI behavior tree issues
- [ ] Applies object-oriented principles in Unity context
- [ ] Implements scalable system architecture

---

## 🛠️ Development Environment

### Required Tools
- **Unity 6000.2.0b1** (or compatible version)
- **Behavior Designer Pro** by Opsive
- **Visual Studio** or **JetBrains Rider**
- **Git** for version control

### Project Setup
1. Clone the Tabletop Wargaming Shop Simulator repository
2. Open project in Unity 6000.2.0b1
3. Import Behavior Designer Pro from Asset Store
4. Verify all assemblies compile without errors

---

## 📚 Additional Resources

### Unity Documentation
- [Behavior Trees Fundamentals](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- [Component Architecture Best Practices](https://docs.unity3d.com/Manual/BestPractice.html)
- [C# Scripting Reference](https://docs.unity3d.com/ScriptReference/)

### Project Documentation
**Core System Documentation:**
- [Customer AI Implementation Summary](../CustomerAI_GameManager_Integration.md) - Comprehensive customer AI system integration
- [Interface Implementation Complete](../Interface_Implementation_Complete.md) - Component interface design patterns
- [State Management Migration](../StateManagementMigration_Complete.md) - Evolution from state machines to behavior trees

**Architecture and Refactoring:**
- [Customer Dependency Injection Complete](../CustomerDependencyInjection_Complete.md) - Component composition patterns
- [Customer Refactoring Delegation Elimination](../CustomerRefactoring_DelegationElimination.md) - Code quality improvements
- [Enhanced Interface Segregation Implementation](../Enhanced_Interface_Segregation_Implementation.md) - Advanced interface design

**System Integration:**
- [Product GameManager Integration Summary](../Product_GameManager_Integration_Summary.md) - Product system architecture
- [Inventory Manager Economic Integration](../InventoryManager_Economic_Integration_Summary.md) - Economic system patterns
- [Shop Status UI Implementation](../ShopStatusUI_Implementation_Summary.md) - UI architecture patterns

**Technical Implementation:**
- [Audio System Implementation](../AudioSystemImplementation.md) - Service layer patterns
- [Customer Character Setup](../CustomerCharacterSetup.md) - Component initialization patterns
- [Customer Legacy Cleanup Complete](../CustomerLegacyCleanup_Complete.md) - Refactoring strategies

### External Learning
- **Behavior Designer**: [Opsive Documentation](https://opsive.com/support/documentation/behavior-designer/)
- **C# Patterns**: Microsoft Docs C# Programming Guide
- **Unity Architecture**: Unity Learn Premium Courses

---

## 🤝 Contributing to the Syllabus

### Adding New Lessons
1. Follow the standardized lesson template (see `/Lesson_Template.md`)
2. Include real project code examples
3. Test all code examples in the project
4. Update the main syllabus index

### Lesson Feedback
- Create issues for unclear explanations
- Suggest additional topics based on project evolution
- Share alternative implementation approaches

---

## 📝 Notes for Instructors

### Teaching Approach
- **Context First**: Always explain why before how
- **Live Coding**: Demonstrate concepts in the actual project
- **Problem-Solving**: Present real challenges we faced and solved
- **Iteration**: Show how code evolved through multiple versions

### Assessment Methods
- **Practical Implementation**: Students implement similar features
- **Code Review**: Analyze existing project code for patterns
- **Problem Solving**: Present new requirements using learned concepts
- **Documentation**: Students explain their implementation choices

---

*This syllabus is a living document that evolves with the project. As we implement new features and solve new challenges, corresponding lessons will be added to capture and share that knowledge.*