# Tabletop Shop Simulator MVP - Game Design Brief

## Core Concept
A first-person shop simulation game where players manage a tabletop gaming store. Stock shelves with miniatures, paints, and rulebooks, set prices, and serve customers to grow your business.

## Target MVP Experience (4 Hours Development)
Players should be able to:
1. Walk around a small shop space
2. Stock 3 product types on shelves 
3. Set prices for products
4. Watch customers enter, browse, purchase items, and leave
5. See money increase from sales
6. End the day and start fresh

## Core Game Loop
**Stock → Price → Sell → Profit → Restock**
- Player places products from inventory onto shop shelves
- Player sets prices for each product
- Customers autonomously enter, browse, and purchase items
- Money increases with each sale
- Player can end day and start new cycle

## Setting & Theme
- Modern tabletop hobby shop
- Focus on wargaming products (miniatures, paints, rulebooks)
- Cozy, authentic game store atmosphere
- Original fictional game universes (avoid licensing issues)

## Product Types (MVP)
1. **Miniature Boxes** - Core wargaming armies ($45 base price)
2. **Paint Pots** - Hobby paints for miniatures ($3 base price)  
3. **Rulebooks** - Game rules and lore books ($25 base price)

## Customer Behavior (MVP)
- Single customer type with basic AI
- Spawns every 30-90 seconds
- Enters shop → Walks to random shelf → Browses briefly → Purchases if available → Leaves
- Simple purchase decision (buys if item is stocked)

## Player Actions (MVP)
- **Movement**: First-person WASD + mouse look
- **Interact**: E key to interact with shelves and products
- **Stock Shelves**: Click empty shelf slot to place selected product
- **Set Prices**: Right-click product to open price input
- **Manage Inventory**: Tab to toggle inventory panel
- **End Day**: Button to reset daily cycle

## UI Elements (MVP)
- **Money Display**: Current cash amount
- **Daily Sales**: Items sold today
- **Inventory Panel**: Available products and quantities
- **Price Setting**: Simple input popup
- **Day Management**: End day button

## Technical Scope
- **Engine**: Unity 3D
- **Perspective**: First-person
- **Environment**: Single small shop room
- **AI**: Basic NavMesh customer pathfinding
- **Data**: ScriptableObject product system
- **Save System**: None (MVP resets each session)

## Success Metrics for MVP
- Player can complete full loop: stock → price → sell → profit
- Customers successfully navigate and purchase items
- UI clearly shows game state and money changes
- Game feels like running a basic shop
- Core systems work reliably without crashes

## Post-MVP Expansion Hooks
- Multiple customer types with different preferences
- Shop expansion and decoration
- More product varieties
- Customer dialogue and satisfaction
- Day/night cycles with planning phases
- Competition and market dynamics

## Art Style (MVP)
- Simple, clean 3D graphics
- Unity primitives with basic materials
- Placeholder textures acceptable
- Focus on functionality over visuals
- Consistent color scheme for product types

## Audio (MVP)
- Optional: Basic sound effects for purchases and interactions
- No music required
- Simple audio feedback for successful actions

---

**This MVP proves the core concept and establishes the foundation for the full tabletop shop simulation experience outlined in the main GDD.**