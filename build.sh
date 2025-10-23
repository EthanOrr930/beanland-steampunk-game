#!/bin/bash
# Build script for Netlify deployment

echo "🔧 Preparing BeanBox for Netlify deployment..."

# Ensure all Unity build files exist
if [ ! -f "Build/BeanBoxBuild.loader.js" ]; then
    echo "❌ Error: Unity loader file not found!"
    exit 1
fi

if [ ! -f "Build/BeanBoxBuild.framework.js.br" ]; then
    echo "❌ Error: Unity framework file not found!"
    exit 1
fi

if [ ! -f "Build/BeanBoxBuild.data.br" ]; then
    echo "❌ Error: Unity data file not found!"
    exit 1
fi

if [ ! -f "Build/BeanBoxBuild.wasm.br" ]; then
    echo "❌ Error: Unity WASM file not found!"
    exit 1
fi

echo "✅ All Unity build files found"
echo "📦 Ready for Netlify deployment!"
echo ""
echo "🚀 Deploy instructions:"
echo "1. Push to GitHub"
echo "2. Connect repository to Netlify"
echo "3. Set build command: 'echo Static site'"
echo "4. Set publish directory: '.'"
