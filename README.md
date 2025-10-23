# 🎮 BeanBox - Steampunk Gravity Adventure

A steampunk-themed pixel art game with gravity controls that respond to your phone's orientation and motion. Built with Unity WebGL and designed for mobile browsers.

## 🚀 Live Demo

**[Play BeanBox on Netlify](https://beanbox-steampunk.netlify.app)** (Coming Soon!)

## 🔗 Links

- **GitHub Repository**: [ModdingMachine/beanbox-steampunk-game](https://github.com/ModdingMachine/beanbox-steampunk-game)
- **Live Demo**: [Netlify Deployment](https://beanbox-steampunk.netlify.app)

## ✨ Features

- **Steampunk Aesthetic**: Brass, copper, and mechanical styling
- **Gravity Controls**: Tilt your phone to change gravity direction
- **Mobile Optimized**: Designed for portrait mode on mobile devices
- **Real-time Motion**: Accelerometer and orientation data integration
- **Responsive Design**: Works on both desktop and mobile
- **Auto Fullscreen**: Automatically enters fullscreen when motion permissions granted
- **Phone Dimensions**: Constrained to 800×1800 with black bars for wider screens

## 🚀 Quick Start

### Option 1: Node.js (Recommended)
```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

### Option 2: Windows (Easy Setup)
1. Double-click `setup-and-run.bat` (installs dependencies and starts server)
2. Open your mobile browser and go to `https://localhost:8443`
3. Allow motion permissions when prompted
4. Tilt your phone to control gravity!

### Option 3: Manual Python Server
```bash
# Install Python 3.7+ if not already installed
python start-local-server.py
```

### Option 4: Alternative Local Servers

#### Using Node.js (if you have it installed):
```bash
npx http-server -p 8443 -S -C localhost.pem -K localhost-key.pem
```

#### Using PHP (if you have it installed):
```bash
php -S localhost:8443
```

## 📱 Mobile Setup

1. **Connect to the same network**: Make sure your phone and computer are on the same WiFi network
2. **Find your computer's IP**: Run `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
3. **Access from phone**: Go to `https://[YOUR_IP]:8443` in your mobile browser
4. **Accept security warning**: The self-signed certificate will show a warning - click "Advanced" and "Proceed"
5. **Allow motion permissions**: Grant access to device motion when prompted

## 🎯 How to Play

1. **Load the game**: The Unity WebGL build will load automatically
2. **Grant permissions**: Allow device motion and orientation access
3. **Tilt to control**: Rotate your phone to change gravity direction
4. **Watch the indicators**: The orientation display shows current tilt values

## 🔧 Technical Details

### Unity Integration
The website communicates with Unity through these methods:
- `SetOrientationData(float orientation)` - Device rotation (0-360°)
- `SetMotionData(float x, float y, float z)` - Accelerometer data
- `SetDeviceData(float orientation, float x, float y, float z)` - Combined data

### Device APIs Used
- **Screen Orientation API**: Detects device rotation
- **Device Motion API**: Accesses accelerometer data
- **Permission API**: Requests motion sensor access (iOS 13+)

### Browser Compatibility
- ✅ Chrome/Edge (Android/iOS)
- ✅ Safari (iOS 13+)
- ✅ Firefox (Android)
- ⚠️ Desktop browsers (limited motion support)

## 🎨 Steampunk Theme

The website features:
- **Color Palette**: Brass (#B87333), Copper, Bronze, Steel
- **Typography**: Cinzel (serif) and Orbitron (monospace) fonts
- **Visual Elements**: Rotating gears, brass borders, steam effects
- **Responsive Design**: Adapts to mobile and desktop screens

## 🛠️ Development

### File Structure
```
BeanBoxBuild/
├── index.html              # Main game page
├── steampunk-style.css     # Steampunk theme styles
├── start-local-server.py    # Python HTTPS server
├── start-server.bat        # Windows batch file
├── Build/                  # Unity WebGL build files
│   ├── BeanBoxBuild.data.br
│   ├── BeanBoxBuild.framework.js.br
│   ├── BeanBoxBuild.wasm.br
│   └── BeanBoxBuild.loader.js
└── TemplateData/           # Unity template assets
```

### Customization
- **Colors**: Edit CSS variables in `steampunk-style.css`
- **Fonts**: Change Google Fonts imports
- **Layout**: Modify HTML structure in `index.html`
- **Unity Communication**: Update JavaScript functions

## 🐛 Troubleshooting

### Common Issues

**"Motion permission denied"**
- Make sure you're using HTTPS
- Try refreshing the page and granting permission again
- Check browser settings for motion sensor access

**"Game not loading"**
- Verify Unity build files are in the `Build/` directory
- Check browser console for errors
- Ensure all `.br` files are present

**"Orientation not updating"**
- Test on actual mobile device (not desktop)
- Check browser compatibility
- Verify JavaScript console for errors

**"Can't connect from phone"**
- Ensure both devices are on same WiFi network
- Check firewall settings
- Try using your computer's IP address instead of localhost

### Debug Mode
Open browser developer tools (F12) to see:
- Unity loading progress
- Device orientation values
- Motion sensor data
- JavaScript errors

## 📋 Requirements

- **Python 3.7+** (for local server)
- **Modern mobile browser** (Chrome, Safari, Firefox)
- **HTTPS connection** (required for device APIs)
- **Unity WebGL build files** (in Build/ directory)

## 🚀 Deployment

For production deployment on Netlify:

1. **Upload files**: Upload entire `BeanBoxBuild/` folder to Netlify
2. **Configure build**: Set build command to `echo "Static site"`
3. **Set redirects**: Add `_redirects` file with `/* /index.html 200`
4. **Enable HTTPS**: Netlify provides free SSL certificates
5. **Test mobile**: Access from mobile devices to verify motion controls

## 📄 License

This project is part of the BeanBox game development. The steampunk theme and device integration code is provided for educational and development purposes.

---

**🎮 Ready to experience gravity-defying steampunk adventure? Start the server and tilt your way to victory!**
