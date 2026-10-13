# Mobile touch-control contract

## Non-negotiable interaction rules
- There is no virtual joystick. Movement is driven by separate left and right buttons.
- Each button's Image rectangle is its exact interactive area. No invisible full-screen gesture region or enlarged transparent hitbox is used.
- Label text is not a raycast target, so tapping the visible glyph still reaches the button beneath it.
- Left and right controls may be held simultaneously; each control tracks pointer IDs independently.
- Releasing one finger must not cancel another finger still holding the same button.
- Jump is edge-triggered and buffered briefly; holding jump controls jump height.
- Controls visibly respond to press state without bright opaque fills.
- Touch state resets when the app loses focus or pauses, preventing stuck movement after notifications/app switching.
- Canvas scales from a 1920x1080 landscape reference layout; controls use normalized anchors and explicit separated rectangles.
- UI content is inset to the device's live `Screen.safeArea`; backgrounds remain full-bleed. Safe-area anchors refresh if the display dimensions change.

## Current normalized control bounds
Coordinates are normalized to screen width/height (0..1).
- Left: center (0.09, 0.16), size (0.105, 0.16).
- Right: center (0.215, 0.16), size (0.105, 0.16).
- Jump: center (0.89, 0.17), size (0.14, 0.19).
- Menu: center (0.94, 0.92), size (0.08, 0.07).

The movement buttons have a 2% screen-width gap, and the jump control is separated from them. The menu control is in the top-right corner and does not overlap the jump control. This is a source-level layout check only; pixel hit-testing must still be verified on physical devices and multiple aspect ratios.

## Required manual QA before calling controls finished
1. Hold left and right with two fingers, then release each in either order.
2. Hold a direction, receive an app focus interruption, return, and confirm movement is not stuck.
3. Tap near every edge and corner of each control; only the intended control should activate.
4. Tap immediately before landing and verify buffered jump; walk off a ledge and verify coyote jump.
5. Test small and large Android screens, gesture-navigation insets, and 16:9 / 20:9 aspect ratios.
6. Verify the menu button never overlaps a gameplay control.
