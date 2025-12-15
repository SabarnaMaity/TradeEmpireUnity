# TradeEmpireUnity
FireBase Using Game like Tading game
TRADE EMPIRE – Final Project Report (README.md)
(Markdown format – ready for GitHub submission)
# TradeEmpire – Final Project Report
Overview
TradeEmpire is a Unity-based trading simulation game integrated with Firebase Authentication and Firebase Realtime Database.
Players can log in, manage inventory, create marketplace listings, buy & sell items, and unlock achievements.
The project is built with clean architecture, modular systems, and scalable Firebase data structures.
## Scenes
1. GameStartScene
Start menu
Play button
Login panel
Registration panel
Guest login
UI Manager handles switching between panels
2. GameScene
Main gameplay
Profile panel
Inventory panel
Marketplace panel
Create Listing panel
Achievements
Real-time database interaction
## System Architecture
Client–Server Model
TradeEmpire follows a client-driven architecture:
Unity (client) → Handles UI, gameplay, inventory logic, marketplace UI
Firebase Authentication → Manages user login, registration, guest login
Firebase Realtime Database → Stores user data, marketplace listings, achievements
Architecture Components
1. Authentication System
Email-password login
Email-based registration
Guest login using Firebase Anonymous Auth
Saves default user profile on first-time account creation
Loads GameScene after successful login
Key scripts:
FireBaseAuthManager.cs
UiManager.cs
2. Profile System
Displays the logged-in player’s:
Username
Total coins
Firebase-linked UID
UI automatically loads data from:
users / UID /
    username
    coins
3. Inventory System
Handles:
Loading items from Firebase
Updating item counts after buying/selling
Real-time UI updates
Inventory schema:
inventory:
    wood
    stone
    gold
Key script: InventoryManager.cs
4. Marketplace System
Includes:
Create listing
Fetch active listings in real time
Buy from marketplace
Give coins to seller
Deduct coins from buyer
Update inventory & UI instantly
Marketplace schema:
marketplace
   listingId
      item
      amount
      price
      sellerUid
      sellerName
Key script: MarketplaceManager.cs
5. Achievement System
Tracks:
firstPurchase (bool)
firstSale (bool)
totalTrades (int)
Unlocks achievements when:
Player buys an item
Player sells an item
Player completes first trade
Database structure:
achievements:
    firstPurchase: true/false
    firstSale: true/false
    totalTrades: 0+
6. UI Management System
Handles:
Opening login panel
Switching to signup panel
Hiding/showing panels
Clean UI navigation
Key script: UiManager.cs
## Database Structure
Firebase Realtime Database Schema
users
 └─ UID
     ├─ username: "Player123"
     ├─ coins: 150
     ├─ inventory
     │    ├─ wood: 5
     │    ├─ stone: 2
     │    └─ gold: 0
     ├─ achievements
     │    ├─ firstPurchase: false
     │    ├─ firstSale: true
     │    └─ totalTrades: 3
marketplace
 └─ listingId
      ├─ item: "wood"
      ├─ amount: 1
      ├─ price: 5
      ├─ sellerUid: "abc123"
      └─ sellerName: "Player123"
## Data Flow (Important Section)
Login Flow
User enters credentials
FirebaseAuth authenticates
On success → user UID is cached
Game loads GameScene
On GameScene start → loads profile, inventory, marketplace
Inventory Update Flow
Player buys item
Coins deducted locally
Firebase updated
Inventory updated
UI refreshed
Market listing removed (if needed)
Achievements updated
Marketplace Flow
Player selects item & price
CreateListing panel sends data
Firebase creates listing node
Marketplace panel reloads
Another player buys listing
Buyer’s inventory increases
Seller receives coins
Listing removed automatically
