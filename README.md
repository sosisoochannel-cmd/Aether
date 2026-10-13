# Aether

Aether is an Android-first 2D action-platformer by Varellon, built with Unity 6.3 LTS (6000.3.24f1) and URP.

## Current branch
The first foundation is being developed on the draft pull request linked from GitHub. The main branch is intentionally untouched until the work passes Unity compilation and device checks.

## What's in this foundation
- Varellon logo reveal with one restrained glint.
- Warning screen, then a black-and-gold main menu.
- Early movement room with left/right touch buttons, jump, keyboard support, basic platforms, and a follow camera.
- Source-structure checks on GitHub Actions.
- Manual Android ARM64 APK workflow.

## Important validation note
The source-structure workflow passing does not prove Unity compilation or gameplay correctness. This branch has not yet been run in the Unity Editor or tested on a physical Android device. The current movement room is graybox; Persian/Arabic shaping is not verified and must not be considered complete.

## Android build
See docs/ANDROID_BUILD.md. The manual APK workflow requires correctly configured Unity license secrets. Do not commit account credentials or license files.

## Project setup in Unity
After opening the project with the pinned Unity version and allowing packages to resolve, use the menu command Aether > Setup > Create or Repair Boot Scene, then press Play. The automated Android build method generates the boot scene automatically.
