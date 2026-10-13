# First playable slice — status

## Implemented in source
- Landscape-first boot flow: warning, menu, then movement room.
- Restrained black/gold UI styling.
- Distinct press-and-hold left/right touch controls and a jump button with pointer-ID tracking, simultaneous-finger support, pressed-state feedback, and focus-loss reset.
- Safe-area-aware layout, exact individual button rectangles, jump buffering, coyote time, acceleration/deceleration, air steering, variable jump height, and stronger fall gravity.
- Keyboard movement support when the Input System package is active.
- Rigidbody2D movement, jump impulse, simple platforms, and a smooth follow camera.
- Unity Editor command to generate a boot scene and register it in Build Settings.
- Fast source-structure, movement-contract, and normalized hitbox-separation checks on GitHub Actions.

## Not yet verified
- The project has not been opened or compiled with Unity 6000.3.24f1.
- No play-mode, Android-device, or APK test has run.
- Vazirmatn font and a contextual Arabic/Persian presentation-form fallback are now bundled. This is not a complete Unicode bidirectional implementation; mixed-script text, punctuation, and all Persian glyph combinations still require rendered regression tests on Android before RTL can be called finished.
- Runtime-generated geometric objects are a graybox used to validate the first gameplay loop, not final production art.
- Android build automation still needs a verified Unity licensing/activation path and Android module availability.

## Next quality gates
1. Open in the pinned Unity Editor and fix every compiler/package error.
2. Generate the boot scene using the Aether Setup menu command.
3. Test jump grounding, simultaneous button presses, screen scaling, pause/focus loss, and scene transitions.
4. Validate Persian and Arabic shaping on a real Android device.
5. Replace graybox visuals only after movement and camera feel are accepted.
