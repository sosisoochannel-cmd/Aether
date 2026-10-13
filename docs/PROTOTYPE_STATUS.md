# First playable slice — status

## Implemented in source
- Landscape-first flow: Varellon logo reveal, minimal Persian warning, black-and-gold menu, then the movement room.
- Main-menu refresh: all game UI text is white; hierarchy comes from weight, size, opacity, and spacing, never gold or gray text.
- Reusable slanted menu plates with a fine gold border; the hit test matches the visible polygon rather than the surrounding rectangle.
- Animated procedural backdrop with layered dark silhouettes, a restrained rift/arc composition, a slow core glow, and low-density drifting motes; the mesh refresh is throttled rather than regenerated every frame.
- Staggered menu text/button entrances use unscaled time, and buttons stay non-interactive until their reveal finishes.
- Menu now offers working Start, Controls, and About destinations rather than linking back to the warning screen.
- Typography uses Unity's bold legacy-text style, subtle text shadow for larger labels, and white opacity levels for hierarchy. Persian/Arabic shaping still requires rendered device validation.
- Logo presentation in source: a short fade/scale settle and one softened gold glint, with no repeated flashes or extra logo geometry.
- Warning copy expanded to explain the prototype state, possible changes, and save-compatibility risks; the screen has no visible countdown/progress bar/continue button and advances automatically.
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
