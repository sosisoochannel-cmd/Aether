# Aether — Technical Direction

## Product constraints
- Android-first 2D action-platformer, landscape orientation.
- Built with Unity 6.3 LTS and URP 2D.
- Mobile touch controls: distinct left and right buttons; no virtual joystick.
- Studio identity: Varellon. In-game title: Aether (never “Project Aether”).
- Visual direction: restrained black-and-gold identity, strong readability, premium hand-crafted atmosphere. Inspiration may inform quality, never copy protected art or assets.
- Persian is the default language for the initial warning flow; localization must be designed for RTL and complex-script validation, not assumed correct from string translation alone.

## Engineering rules
1. Keep `main` stable. Work on short-lived branches and review through pull requests.
2. Do not claim Unity compilation, device behavior, or APK success without a real run and recorded evidence.
3. Prefer small, testable systems with explicit ownership and clear dependencies.
4. Separate gameplay rules, presentation, input, save data, and platform-specific services.
5. No placeholder systems presented as finished features.
6. Do not spend Actions minutes on APK builds until source checks pass and the build workflow/license path is confirmed.
7. Preserve the old repository as a reference; copy code only after inspection and validation.

## Planned implementation order
1. Establish a bootable Unity project and reproducible CI checks.
2. Build a playable movement test room and validate mobile controls.
3. Implement combat fundamentals and enemy interactions.
4. Validate Persian/Arabic shaping and RTL on actual Android rendering.
5. Build the studio intro, warning screen, and main menu.
6. Deliver one polished vertical slice before scaling content or boss count.

## Current validation status
The scaffold has not yet been opened or compiled by Unity. The presence of configuration files is not proof of a valid build.
