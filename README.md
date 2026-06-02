# BeanBox

Steampunk gravity-puzzle game for mobile. You don't move the character directly — you tilt your phone to change which way gravity pulls, then use those gravity shifts to guide the bean through hand-built pixel-art levels. Made in Unity, exported to WebGL, tuned for mobile browsers.

Play: https://beanbox.netlify.app/

## Gameplay

Each level is a self-contained physics puzzle with one goal: get the bean to the exit. The twist is the control scheme — rotating the phone rotates gravity, and everything in the level falls toward the new "down." Solving a level means working out the right sequence and timing of gravity flips to drop, slide, and launch the bean past the steampunk obstacles (platforms, gears, hazards) and into the goal.

- Tilt to set the direction of gravity.
- Use falls and momentum to cross gaps and clear hazards.
- Read each level as a puzzle: which way to tip, and when.

## Controls

- Mobile: physical tilt via the device accelerometer / orientation sensor, portrait orientation.
- Desktop: keyboard fallback for testing on machines without tilt hardware.

## Tech

- Engine: Unity (C#), exported to WebGL.
- Input: device orientation + accelerometer, behind an iOS motion-permission prompt.
- Target: mobile browsers in portrait, locked to a phone-shaped viewport (letterboxed on wider screens).

## Run locally

```bash
npm install
npm run dev
```

Open the served HTTPS URL on your phone, grant motion permission, and tilt to play. HTTPS is required for the browser to expose the motion sensors.
