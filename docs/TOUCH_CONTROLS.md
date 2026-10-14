# Gameplay touch controls

## Visual assets
Gameplay controls use individual authored PNG sprites in `Assets/Aether/Resources/GameplayControls/`:
- `JoystickBase.png`: fixed circular base with Aether's engraved dark-metal and gold finish.
- `JoystickKnob.png`: separate thumb-knob texture.
- `Jump.png`: dedicated jump emblem.
- `Pause.png`: gameplay pause emblem.

Import each PNG as a Sprite (2D and UI), preserve alpha, use Clamp wrap mode and Bilinear filtering, and keep Preserve Aspect enabled on the corresponding UI Image.

## Input rules
- Movement uses one **fixed-origin** virtual joystick. The base stays at its saved location; only the knob moves inside its travel radius.
- The joystick owns one active pointer and applies a dead zone. Its base never teleports to a touch point.
- Jump is a separate image-based control; its Image bounds are the touch target.
- Movement and jump can be used at the same time.
- Touch input resets when the app loses focus or pauses.
- Gameplay controls use normalized anchors inside the live safe area and a 1920×1080 landscape reference Canvas.
- Keyboard A/D or arrow keys remain available for movement, and Space remains available for jump.

## PUBG-style control layout editor
Open the gameplay pause button and choose **تنظیم چیدمان کنترل‌ها**.
- Drag the joystick or jump control to move it.
- Select joystick/jump, then use the size buttons to resize the selected control.
- Save persists normalized positions and scale in PlayerPrefs.
- Reset returns to authored defaults. Cancel exits without saving current edits.
- Position anchors are clamped away from the extreme screen edges to reduce system-gesture conflicts.

## Source-level default layout
Coordinates are normalized to the safe-area content (0..1).
- Joystick center: (0.145, 0.205), size: 218×218 reference pixels.
- Jump center: (0.875, 0.19), size: 154×154 reference pixels.
- Pause center: (0.955, 0.92), size: 76×76 reference pixels.

## Required manual QA before calling controls finished
1. Drag the joystick and verify the base stays fixed while the knob moves.
2. Test dead zone and full left/right tilt with several finger sizes.
3. Hold movement and jump simultaneously; releasing jump must not cancel joystick movement.
4. Receive an app focus interruption while moving and verify no stuck input.
5. Drag/resize/save/reset/cancel the layout; verify values persist only after Save.
6. Test small and large Android screens, gesture-navigation insets, and 16:9 / 20:9 aspect ratios.
7. Tap near each control edge and verify no unintended activation or overlap.
8. Import and visually inspect all four PNGs in Unity. Source checks cannot validate rendered image quality or real-device touch feel.
