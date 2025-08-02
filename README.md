# Solitaire Pyramid - Unity Implementation

A complete implementation of Solitaire Pyramid in Unity 2D.

## Game Overview

Solitaire Pyramid is a single-player card game where the goal is to clear the pyramid by removing cards that add up to 13. The game features:

- **Pyramid Layout**: 7 rows of cards arranged in a pyramid shape
- **Stock Pile**: 24 cards that can be drawn one at a time
- **Waste Pile**: Cards drawn from the stock
- **Foundation**: Where removed cards are placed
- **Scoring**: Points for each card moved to foundation
- **Undo System**: Ability to undo moves

## Game Rules

1. **Objective**: Remove all cards from the pyramid by matching pairs that sum to 13
2. **Valid Pairs**: 
   - King (13) can be removed alone
   - Queen (12) + Ace (1) = 13
   - Jack (11) + 2 = 13
   - 10 + 3 = 13
   - And so on...
3. **Card Removal**: Only face-up cards at the bottom of the pyramid can be removed
4. **Drawing**: Click "Draw" to draw a card from the stock to the waste pile
5. **Foundation**: Click on waste cards to move them to the foundation
6. **Reshuffle**: When stock is empty, waste cards are reshuffled into stock

## Setup Instructions

### 1. Scene Setup
- Open the SampleScene in Unity
- The game will automatically set up when you press Play
- If you want to set up manually, add a GameObject with the `GameSetup` component

### 2. Sprite Configuration
The game uses two sprite assets:
- `Cards trimmed.png` - Contains all 52 playing cards
- `Cards backs.png` - Card back design

**Important**: Make sure these sprites are properly configured in Unity:
1. Select the sprite files in the Project window
2. In the Inspector, set Texture Type to "Sprite (2D and UI)"
3. For `Cards trimmed.png`, set Sprite Mode to "Multiple" and slice the sprite sheet
4. Move the sprites to the `Assets/Resources/` folder

### 3. Fallback System
If sprites fail to load, the game will create simple colored rectangles as fallback cards:
- Red cards for Hearts and Diamonds
- Black cards for Clubs and Spades
- Blue card back

## Game Controls

- **Click on pyramid cards**: Remove them if they're valid (sum to 13)
- **Click "Draw" button**: Draw a card from stock to waste
- **Click on waste cards**: Move them to foundation
- **Click "Undo" button**: Undo the last action

## Implementation Details

### Core Scripts

1. **GameManager.cs**: Main game logic and state management
2. **CardUI.cs**: Visual representation and interaction for individual cards
3. **UISetup.cs**: Automatic UI creation and layout
4. **GameSetup.cs**: Scene initialization
5. **CardPrefab.cs**: Card prefab configuration

### Game State

The game maintains several card collections:
- `pyramid`: Cards in the pyramid layout
- `stock`: Cards in the draw pile
- `waste`: Cards drawn from stock
- `foundation`: Successfully removed cards

### UI Layout

- **Pyramid**: Center area with 7 rows of cards
- **Stock**: Bottom-left area for draw pile
- **Waste**: Next to stock for drawn cards
- **Foundation**: Bottom-right area for removed cards
- **Controls**: Draw and Undo buttons
- **Info**: Score and move counter

## Customization

### Game Settings
In the GameManager component, you can adjust:
- `pyramidRows`: Number of rows in the pyramid (default: 7)
- `stockCards`: Number of cards in stock (default: 24)

### Visual Customization
- Modify card sizes in `CardPrefab.cs`
- Adjust UI layout in `UISetup.cs`
- Change colors and fonts in the UI setup methods

## Troubleshooting

1. **Sprites not loading**: Check that sprites are in the Resources folder and properly configured
2. **UI not appearing**: Ensure the scene has a Canvas and the GameSetup script is running
3. **Cards not clickable**: Verify that the CardUI component is properly attached to card prefabs

## Future Enhancements

Potential improvements for the base game:
- Sound effects and music
- Animations for card movements
- Multiple difficulty levels
- Save/load game state
- Statistics tracking
- Visual themes and card designs
- Mobile touch support
- AI hints system

## Credits

This implementation provides a solid foundation for a Solitaire Pyramid game that can be extended with additional features and polish. 