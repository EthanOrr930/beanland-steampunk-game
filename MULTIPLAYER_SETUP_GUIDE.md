# BeanBox Multiplayer Setup Guide

This guide will walk you through setting up the multiplayer system for your BeanBox game.

## Prerequisites
- Unity project with BeanBox game
- Netlify account (for hosting)
- Basic knowledge of Unity scene management

## Step 1: Unity Scene Setup

### 1.1 Create MultiplayerMenu Scene
1. In Unity, go to **File → New Scene**
2. Save as `MultiplayerMenu` in your Scenes folder
3. Add a Canvas with UI elements:
   - **Title Text**: "BeanBox Multiplayer"
   - **Create Room Button**: Connect to `MultiplayerMenu.CreateRoom()`
   - **Room Code Input Field**: 6-character limit, uppercase
   - **Join Room Button**: Connect to `MultiplayerMenu.JoinRoom()`
   - **Back Button**: Load "Main Menu" scene
   - **Status Text**: For displaying messages

### 1.2 Create MultiplayerDefaultLevel Scene
1. Create new scene: **File → New Scene**
2. Save as `MultiplayerDefaultLevel` in your Scenes folder
3. Add the following GameObjects:
   - **Ground** (with Collider2D)
   - **Walls** (left and right boundaries)
   - **Platforms** (for jumping)
   - **EspressoMachine** (goal object)
   - **Player** (with PlayerController script)

### 1.3 Setup Player for Multiplayer
1. Select your Player GameObject
2. Add these components:
   - `MultiplayerGravityController` script
   - `MultiplayerUI` script (for UI display)
3. In PlayerController, set:
   - `useMultiplayerGravity = true`
   - Assign `MultiplayerGravityController` reference

## Step 2: Script Configuration

### 2.1 MultiplayerManager Setup
1. Create empty GameObject named "MultiplayerManager"
2. Add `MultiplayerManager` script
3. Configure settings:
   - `multiplayerSceneName = "MultiplayerDefaultLevel"`
   - `mainMenuSceneName = "Main Menu"`
   - `roomCodeLength = 6`

### 2.2 MainMenu Updates
1. Open your Main Menu scene
2. Add a "Multiplayer" button
3. Connect button to `MainMenu.OpenMultiplayer()`
4. Set `multiplayerSceneName = "MultiplayerMenu"`

### 2.3 LevelManager Updates
1. In your level scenes, add `MultiplayerLevelManager` script
2. Configure:
   - `mainMenuSceneName = "Main Menu"`
   - `multiplayerMenuSceneName = "MultiplayerMenu"`

## Step 3: Build Settings

### 3.1 Add Scenes to Build
1. Go to **File → Build Settings**
2. Add these scenes in order:
   - Main Menu
   - MultiplayerMenu
   - MultiplayerDefaultLevel
   - Your existing levels

### 3.2 WebGL Build Settings
1. Platform: **WebGL**
2. Player Settings:
   - **Company Name**: Your company name
   - **Product Name**: BeanBox
   - **Version**: 1.0
3. **Publishing Settings**:
   - **Compression Format**: Gzip
   - **Data Caching**: Enabled

## Step 4: Netlify Deployment

### 4.1 Prepare Files
1. Build your Unity project for WebGL
2. Copy the build files to your project root
3. Ensure these files are present:
   - `index.html`
   - `multiplayer.html`
   - `multiplayer-room-manager.js`
   - `netlify/functions/room-manager.js`
   - `netlify.toml`

### 4.2 Deploy to Netlify
1. **Option A - Git Integration**:
   - Push your code to GitHub
   - Connect Netlify to your repository
   - Netlify will auto-deploy

2. **Option B - Manual Deploy**:
   - Zip your project folder
   - Drag and drop to Netlify dashboard
   - Or use Netlify CLI: `netlify deploy --prod`

### 4.3 Enable Functions
1. In Netlify dashboard, go to **Functions**
2. Ensure `room-manager.js` is deployed
3. Test the function at: `https://your-site.netlify.app/.netlify/functions/room-manager`

## Step 5: Testing

### 5.1 Local Testing
1. Open your game in two different browsers
2. Navigate to `multiplayer.html`
3. Create room on one browser
4. Join room on second browser
5. Test gravity alignment

### 5.2 Mobile Testing
1. Deploy to Netlify
2. Open on two different phones
3. Test phone orientation controls
4. Verify gravity synchronization

## Step 6: UI Setup

### 6.1 MultiplayerMenu Scene UI
```
Canvas
├── Title (Text): "BeanBox Multiplayer"
├── CreateRoomButton (Button)
├── RoomCodeInput (InputField)
├── JoinRoomButton (Button)
├── BackButton (Button)
└── StatusText (Text)
```

### 6.2 MultiplayerDefaultLevel Scene UI
```
Canvas
├── RoomCodeText (Text)
├── PlayerCountText (Text)
├── GravityStatusText (Text)
├── LeaveRoomButton (Button)
└── ReturnToMenuButton (Button)
```

## Step 7: Script References

### 7.1 MultiplayerMenu Script References
- `multiplayerPanel`: Main UI panel
- `createRoomButton`: Create room button
- `joinRoomButton`: Join room button
- `roomCodeInput`: Room code input field
- `statusText`: Status display text

### 7.2 MultiplayerUI Script References
- `roomCodeText`: Room code display
- `playerCountText`: Player count display
- `gravityStatusText`: Gravity status display
- `leaveRoomButton`: Leave room button

## Step 8: Build and Deploy

### 8.1 Final Build
1. **File → Build Settings**
2. **Build** (not Build and Run)
3. Choose output folder
4. Wait for build to complete

### 8.2 Deploy to Netlify
1. Copy all files to Netlify
2. Ensure functions are working
3. Test multiplayer functionality

## Troubleshooting

### Common Issues:
1. **"MultiplayerManager not found"**: Ensure MultiplayerManager is in the scene
2. **"Room not found"**: Check Netlify Functions are deployed
3. **"Gravity not aligning"**: Verify phone orientation permissions
4. **"UI not updating"**: Check script references in Inspector

### Debug Steps:
1. Open browser console (F12)
2. Check for JavaScript errors
3. Verify Unity console for C# errors
4. Test Netlify Functions directly

## Next Steps

Once setup is complete:
1. Test with two devices
2. Customize the multiplayer level
3. Add more multiplayer features
4. Optimize for performance

## Support

If you encounter issues:
1. Check Unity console for errors
2. Verify browser console for JavaScript errors
3. Test Netlify Functions manually
4. Ensure all script references are assigned
