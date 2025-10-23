# WebGL External Data Setup Guide

This guide explains how to set up the external website to send device orientation and motion data to your Unity WebGL game.

## Unity Side (Receiving End) - ✅ COMPLETED

The Unity side is already set up with the following public methods that can be called from external JavaScript:

### Available Methods:

1. **`SetOrientationData(float orientation)`**
   - Sets the device orientation angle in degrees
   - Range: 0-360 degrees
   - Example: `SetOrientationData(90)` for landscape left

2. **`SetMotionData(float x, float y, float z)`**
   - Sets accelerometer data
   - Values are typically in m/s² or g-force units
   - Example: `SetMotionData(0.1, -0.8, 0.2)` for slight tilt

3. **`SetDeviceData(float orientation, float x, float y, float z)`**
   - Convenience method to set both orientation and motion data at once
   - Example: `SetDeviceData(90, 0.1, -0.8, 0.2)`

## External Website Side (Sending End) - 🔧 TO BE IMPLEMENTED

### Required JavaScript Setup:

#### 1. Device Orientation Detection
```javascript
// Get device orientation
function getDeviceOrientation() {
    if (typeof screen !== 'undefined' && screen.orientation) {
        return screen.orientation.angle;
    } else if (typeof window !== 'undefined' && window.orientation !== undefined) {
        return window.orientation;
    }
    return 0;
}

// Listen for orientation changes
if (typeof screen !== 'undefined' && screen.orientation) {
    screen.orientation.addEventListener('change', sendOrientationToUnity);
} else if (typeof window !== 'undefined' && window.orientation !== undefined) {
    window.addEventListener('orientationchange', sendOrientationToUnity);
}
```

#### 2. Device Motion Detection
```javascript
// Request permission for motion data (required for iOS 13+)
function requestMotionPermission() {
    if (typeof DeviceMotionEvent !== 'undefined' && typeof DeviceMotionEvent.requestPermission === 'function') {
        DeviceMotionEvent.requestPermission().then(function(response) {
            if (response === 'granted') {
                startMotionDetection();
            }
        });
    } else {
        startMotionDetection();
    }
}

// Start motion detection
function startMotionDetection() {
    window.addEventListener('devicemotion', function(event) {
        if (event.acceleration) {
            sendMotionToUnity(
                event.acceleration.x || 0,
                event.acceleration.y || 0,
                event.acceleration.z || 0
            );
        }
    });
}
```

#### 3. Unity Communication Functions
```javascript
// Send orientation data to Unity
function sendOrientationToUnity() {
    const orientation = getDeviceOrientation();
    
    // Call Unity method
    if (typeof gameInstance !== 'undefined' && gameInstance.SendMessage) {
        gameInstance.SendMessage('PhoneOrientationController', 'SetOrientationData', orientation);
    }
}

// Send motion data to Unity
function sendMotionToUnity(x, y, z) {
    if (typeof gameInstance !== 'undefined' && gameInstance.SendMessage) {
        gameInstance.SendMessage('PhoneOrientationController', 'SetMotionData', `${x},${y},${z}`);
    }
}

// Send both orientation and motion data
function sendDeviceDataToUnity(orientation, x, y, z) {
    if (typeof gameInstance !== 'undefined' && gameInstance.SendMessage) {
        gameInstance.SendMessage('PhoneOrientationController', 'SetDeviceData', `${orientation},${x},${y},${z}`);
    }
}
```

#### 4. Unity WebGL Instance Setup
```javascript
// Wait for Unity to load
window.addEventListener('load', function() {
    // Unity WebGL instance will be available as 'gameInstance'
    // Make sure your Unity build has the correct name
    
    // Initialize data sending
    requestMotionPermission();
    sendOrientationToUnity(); // Send initial orientation
    
    // Set up periodic updates (optional)
    setInterval(function() {
        sendOrientationToUnity();
    }, 100); // Update every 100ms
});
```

### HTML Integration Example:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Your Game</title>
    <script src="Build/UnityLoader.js"></script>
</head>
<body>
    <div id="unityContainer"></div>
    
    <script>
        // Unity WebGL configuration
        var config = {
            dataUrl: "Build/YourGame.data",
            frameworkUrl: "Build/YourGame.framework.js",
            codeUrl: "Build/YourGame.wasm",
            streamingAssetsUrl: "StreamingAssets",
            companyName: "YourCompany",
            productName: "YourGame",
            productVersion: "1.0",
        };
        
        // Load Unity
        var gameInstance = UnityLoader.instantiate("unityContainer", config);
        
        // Your device orientation and motion detection code goes here
        // (Use the JavaScript functions provided above)
    </script>
</body>
</html>
```

## Data Format Specifications:

### Orientation Data:
- **Type**: Float
- **Range**: 0-360 degrees
- **Values**:
  - 0° = Portrait (normal)
  - 90° = Landscape Left
  - 180° = Portrait Upside Down
  - 270° = Landscape Right

### Motion Data:
- **Type**: Three floats (x, y, z)
- **Units**: m/s² or g-force
- **Coordinate System**: Device coordinate system
- **Typical Range**: -2.0 to +2.0 for each axis

## Testing:

1. Build your Unity project for WebGL
2. Host it on a web server (required for device APIs)
3. Access from a mobile device
4. Check browser console for debug messages
5. Verify that orientation changes affect the game

## Notes:

- Device orientation and motion APIs require HTTPS in production
- Some browsers may require user interaction before enabling motion sensors
- iOS 13+ requires explicit permission for motion data
- Test on actual devices, not desktop browsers

## Troubleshooting:

- **No data received**: Check that `gameInstance` is properly loaded
- **Permission denied**: Ensure HTTPS and user interaction
- **Orientation not updating**: Check browser compatibility
- **Motion data not working**: Verify permission was granted

---

This setup allows your external website to capture device orientation and motion data and send it to your Unity WebGL game in real-time.
