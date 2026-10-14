Aether Gameplay Control Textures

Four separate transparent PNG sprites are authored for runtime use:
- JoystickBase.png — fixed-position movement ring, cyan/blue crystal accents.
- JoystickKnob.png — independent draggable knob, matching crystal palette.
- Jump.png — readable jump silhouette with blue/cyan energy accents.
- Pause.png — high-contrast pause glyph with violet/blue accents.

These are original project assets, not a concept-sheet crop. Each PNG has its own Unity TextureImporter metadata and is loaded from Resources/GameplayControls. Keep the base and knob separate so the joystick can move the knob without moving its hit area. The sprites use transparent outer margins so they remain visible over varied scenes.
