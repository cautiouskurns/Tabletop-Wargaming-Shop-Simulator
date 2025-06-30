# Unity C# Learning Syllabus

## 📖 Overview
This comprehensive learning syllabus is built from real Unity C# development work on the Tabletop Wargaming Shop Simulator project. Each lesson is grounded in actual implementations, providing practical, applicable knowledge for Unity game development.

## 🗂️ Syllabus Structure

### 📋 Main Documents
- **[Unity_CSharp_Learning_Syllabus.md](Unity_CSharp_Learning_Syllabus.md)** - Complete syllabus overview and structure
- **[Lesson_Template.md](Lesson_Template.md)** - Standardized format for all lessons

### 📚 Module 1: Behavior Designer Integration (✅ Complete)
**Focus**: AI System Development with Visual Behavior Trees

1. **[Lesson 1.1: Action vs Conditional Tasks](Lesson_1-1_Action_vs_Conditional_Tasks.md)** (🟢 45 min)
   - Understanding the fundamental building blocks of behavior trees
   - TaskStatus management and execution flow
   - Real examples from customer movement and state checking

2. **[Lesson 1.2: Customer AI State Management](Lesson_1-2_Customer_AI_State_Management.md)** (🟡 60 min)
   - Hybrid system design supporting legacy and modern AI
   - Shared state variables and component coordination
   - Debug logging and state visualization

3. **[Lesson 1.3: NavMesh Integration with Behavior Trees](Lesson_1-3_NavMesh_Integration.md)** (🟡 75 min)
   - Unity NavMesh pathfinding in behavior tree tasks
   - Error handling and position validation
   - Movement completion detection and failure recovery

4. **[Lesson 1.4: Complex Checkout Workflow Implementation](Lesson_1-4_Complex_Checkout_Workflow.md)** (🔴 90 min)
   - Multi-phase task design and state progression
   - External system integration and timeout management
   - Asynchronous operation coordination

5. **[Lesson 1.5: Debugging and Testing Behavior Trees](Lesson_1-5_Debugging_Testing_Behavior_Trees.md)** (🟡 45 min)
   - Comprehensive logging systems for AI analysis
   - Visual debugging integration with Behavior Designer
   - Systematic approaches to diagnosing behavior tree issues

### 📋 Module 2: Advanced C# Patterns (📝 Outlined)
**Focus**: Object-Oriented Design and Unity-Specific Patterns
- **[Module 2 Overview](Module_2_Advanced_CSharp_Patterns.md)** - Complete module structure and learning objectives

Planned Lessons:
- 2.1: Interface Segregation in Unity (🟡 60 min)
- 2.2: Component Composition Patterns (🟡 75 min)  
- 2.3: Event-Driven Architecture (🔴 90 min)
- 2.4: ScriptableObject Data Management (🟡 60 min)

### 📋 Module 3: Unity Component Architecture (📝 Outlined)
**Focus**: Building Maintainable, Scalable Unity Systems
- **[Module 3 Overview](Module_3_Unity_Component_Architecture.md)** - Complete module structure and learning objectives

Planned Lessons:
- 3.1: Single Responsibility Components (🟢 45 min)
- 3.2: Cross-Component Communication (🟡 60 min)
- 3.3: Manager Pattern Implementation (🔴 75 min)
- 3.4: Testing Component Systems (🔴 90 min)

## 🎯 Learning Philosophy

### Practice-First Approach
- Every concept demonstrated through working code from our project
- Real-world challenges and solutions from actual game development
- Progressive complexity building from simple tasks to complete systems

### Project Integration
All lessons reference actual project files:
- **Customer System**: `/Assets/Scripts/3 - Systems/AI/Customer/`
- **Behavior Designer Tasks**: `/Assets/Scripts/6 - Testing/Prototyping/`
- **Product System**: `/Assets/Scripts/2 - Entities/Products/`
- **Shop Management**: `/Assets/Scripts/2 - Entities/Shop/`

## 🏁 Getting Started

### Prerequisites
- Basic C# knowledge (variables, methods, classes)
- Unity Editor familiarity (Inspector, Hierarchy, Project windows)
- Understanding of GameObject and Component concepts

### Development Environment
- **Unity 6000.2.0b1** (or compatible version)
- **Behavior Designer Pro** by Opsive
- **Visual Studio** or **JetBrains Rider**
- **Git** for version control

### Recommended Learning Path
1. Start with **Module 1** for hands-on AI development experience
2. Progress to **Module 2** for advanced programming patterns
3. Complete with **Module 3** for large-scale system architecture
4. Apply knowledge through the progressive exercises in each module

## 📈 Progress Tracking

### Current Status
- ✅ **Module 1**: 5/5 lessons complete with detailed examples
- 📝 **Module 2**: Outlined with comprehensive structure  
- 📝 **Module 3**: Outlined with clear learning progression

### Skills Developed
- Behavior Designer task implementation and debugging
- Unity component architecture and communication patterns
- Advanced C# patterns in game development context
- Professional-level code organization and testing

## 🔗 Project Context

This syllabus emerges from real development work on a Unity 6000.2.0b1 project featuring:
- **Complex AI Systems**: Customer behavior using Behavior Designer Pro
- **Component Architecture**: Modular, maintainable Unity systems
- **System Integration**: Multiple coordinated game systems
- **Professional Practices**: Testing, debugging, and documentation

## 🤝 Contributing

### Adding Content
1. Follow the standardized lesson template
2. Include real project code examples with line numbers
3. Test all examples in the actual project
4. Update relevant index files

### Feedback
- Suggest improvements based on teaching experience
- Report unclear explanations or missing context
- Share alternative implementation approaches

---

*This learning syllabus represents the practical Unity C# knowledge gained through real project development, structured for educational use and progressive skill building.*