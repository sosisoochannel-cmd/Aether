#!/usr/bin/env python3
"""Fast source-structure checks for Aether; not a substitute for Unity compilation."""
import json
import re
from pathlib import Path
import sys

root = Path(__file__).resolve().parents[1]
required = [
    "ProjectSettings/ProjectVersion.txt",
    "Packages/manifest.json",
    "Assets/Aether/Scripts/Runtime/AetherGameBootstrap.cs",
    "Assets/Aether/Scripts/Runtime/AetherParallelogramGraphic.cs",
    "Assets/Aether/Scripts/Runtime/AetherParallelogramGraphic.cs.meta",
    "Assets/Aether/Scripts/Runtime/AetherMenuAtmosphere.cs",
    "Assets/Aether/Scripts/Runtime/AetherMenuAtmosphere.cs.meta",
    "Assets/Aether/Scripts/Runtime/AetherUIReveal.cs",
    "Assets/Aether/Scripts/Runtime/AetherUIReveal.cs.meta",
    "Assets/Aether/Scripts/Runtime/AetherUIButtonFeedback.cs",
    "Assets/Aether/Scripts/Runtime/AetherUIButtonFeedback.cs.meta",
    "Assets/Aether/Scripts/Runtime/TouchHoldButton.cs",
    "Assets/Aether/Scripts/Runtime/PrototypePlayerMotor.cs",
    "Assets/Aether/Editor/AetherProjectSetup.cs",
    "docs/ARCHITECTURE.md",
    "docs/MAIN_MENU_REVIEW.md",
    "docs/TOUCH_CONTROLS.md",
    "Assets/Aether/Resources/Fonts/Vazirmatn-Regular.ttf",
    "Assets/Aether/Scripts/Runtime/RtlText.cs",
]
errors = []

# Stable Unity asset identity matters for reliable imports and scene references.
guid_paths = {}
for meta in root.rglob("*.meta"):
    match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", meta.read_text(encoding="utf-8"), re.MULTILINE)
    if not match:
        errors.append(f"Missing or malformed Unity GUID in {meta.relative_to(root)}")
        continue
    guid = match.group(1).lower()
    previous = guid_paths.get(guid)
    if previous is not None:
        errors.append(f"Duplicate Unity GUID {guid}: {previous} and {meta.relative_to(root)}")
    else:
        guid_paths[guid] = meta.relative_to(root).as_posix()

for script in root.glob("Assets/**/*.cs"):
    meta = Path(str(script) + ".meta")
    if not meta.is_file():
        errors.append(f"Missing stable Unity meta file for {script.relative_to(root)}")

for item in required:
    if not (root / item).is_file():
        errors.append(f"Missing required file: {item}")

version = root / "ProjectSettings/ProjectVersion.txt"
if version.exists() and "m_EditorVersion: 6000.3.24f1" not in version.read_text(encoding="utf-8"):
    errors.append("Unexpected Unity Editor version")

manifest = root / "Packages/manifest.json"
if manifest.exists():
    try:
        deps = json.loads(manifest.read_text(encoding="utf-8")).get("dependencies", {})
        for package in ("com.unity.inputsystem", "com.unity.ugui", "com.unity.render-pipelines.universal"):
            if package not in deps:
                errors.append(f"Missing package dependency: {package}")
    except json.JSONDecodeError as exc:
        errors.append(f"Invalid Packages/manifest.json: {exc}")

bootstrap = root / "Assets/Aether/Scripts/Runtime/AetherGameBootstrap.cs"
if bootstrap.exists():
    source = bootstrap.read_text(encoding="utf-8")
    if "eventSystemObject" in source and "if (FindObjectOfType<EventSystem>() == null)\\n            var eventSystemObject" in source:
        errors.append("Malformed unbraced EventSystem initialization")
    for contract in ("AnimateLogoReveal", "SoftGlintSprite", "PlayLogoGlint", "Screen.safeArea", "needsDefaultActions", "AssignDefaultActions"):
        if contract not in source:
            errors.append(f"Bootstrap missing presentation/safe-area contract: {contract}")
    if 'MakeButton("ادامه"' in source:
        errors.append("Warning screen must remain minimal and auto-dismiss without a visible continue button")
    if "ذخیره‌های نسخه‌های آزمایشی" not in source:
        errors.append("Warning screen is missing its save-compatibility notice")
    if "WaitForSecondsRealtime(7f)" not in source:
        errors.append("Warning screen must have a non-blocking automatic transition")
    if "button.targetGraphic = plate" not in source:
        errors.append("Menu buttons must use the shaped menu plate as their raycast target")
    for contract in ("BuildMenuAtmosphere", "ShowControls", "ShowAbout", "ShowSettings", "ToggleMenuMotion", "ToggleTextSize", "ApplyMenuPreferences", "PlayerPrefs.Save()", "FocusButtonWhenVisible", "EventSystem.current.SetSelectedGameObject", "colors.selectedColor = colors.normalColor", "FontStyle.Bold", "Mathf.Clamp01(color.a)", "RevealMenuElement", "menuEntrancePlayed"):
        if contract not in source:
            errors.append(f"Main menu quality contract missing: {contract}")
    for label in ('MakeButton("شروع بازی"', 'MakeButton("کنترل‌ها"', 'MakeButton("دربارهٔ بازی"', 'MakeButton("تنظیمات"'):
        if label not in source:
            errors.append(f"Main menu action missing: {label}")
    if 'MakeButton("بازگشت به هشدار"' in source:
        errors.append("Main menu must not expose the old return-to-warning placeholder action")
    if "text.color = new Color(1f, 1f, 1f," not in source:
        errors.append("All UI text must use white with opacity as its hierarchy signal")
    if "text.fontStyle = FontStyle.Bold" not in source:
        errors.append("Runtime text should use bold weight for the current menu typography direction")
    if "typeof(Canvas), typeof(AetherMenuAtmosphere)" not in source or "go.transform.SetParent(canvas.transform, false)" not in source:
        errors.append("Full-bleed atmosphere should live on an isolated child Canvas, not inside safe-area content")
    if "\\\\n" in source:
        errors.append("Unexpected doubled newline escape in UI source; it would print literal \\n")

atmosphere_file = root / "Assets/Aether/Scripts/Runtime/AetherMenuAtmosphere.cs"
if atmosphere_file.exists():
    atmosphere_source = atmosphere_file.read_text(encoding="utf-8")
    for contract in ("SetVerticesDirty", "Time.unscaledTime", "AddEllipseArc", "DrawMotes", "DrawMountainLayer", "DrawRiftCore", "MotionEnabled"):
        if contract not in atmosphere_source:
            errors.append(f"Animated menu atmosphere missing contract: {contract}")

reveal_file = root / "Assets/Aether/Scripts/Runtime/AetherUIReveal.cs"
if reveal_file.exists():
    reveal_source = reveal_file.read_text(encoding="utf-8")
    for contract in ("WaitForSecondsRealtime", "Time.unscaledDeltaTime", "IsComplete", "graphic.raycastTarget", "MotionEnabled", "isButtonGraphic", "interactionEnabled", "wasInteractable"):
        if contract not in reveal_source:
            errors.append(f"UI entrance animation missing contract: {contract}")
    if "button.interactable = false" not in reveal_source or "button.interactable = wasInteractable" not in reveal_source or "t >= 0.18f" not in reveal_source:
        errors.append("Buttons must remain non-interactable until visibly entering, then restore their original interactable state")
    if "graphic.raycastTarget = isButtonGraphic" not in reveal_source:
        errors.append("UI reveal must restore the appropriate raycast target after reveal")
    if "graphic.raycastTarget = false" not in reveal_source or "t >= 0.18f" not in reveal_source:
        errors.append("UI buttons must remain untappable while invisible, then activate once visibly entering")
    if reveal_source.index("graphic.raycastTarget = false") > reveal_source.index("if (!MotionEnabled)"):
        errors.append("UI reveal must initialize its hidden raycast state before selecting the motion branch")

feedback_file = root / "Assets/Aether/Scripts/Runtime/AetherUIButtonFeedback.cs"
if feedback_file.exists():
    feedback_source = feedback_file.read_text(encoding="utf-8")
    for contract in ("IPointerDownHandler", "IPointerUpHandler", "ISelectHandler", "IDeselectHandler", "HashSet<int>", "Mathf.Exp", "IsComplete", "MotionEnabled", "pointerInside || selected"):
        if contract not in feedback_source:
            errors.append(f"Button press feedback missing contract: {contract}")

plate_file = root / "Assets/Aether/Scripts/Runtime/AetherParallelogramGraphic.cs"
if plate_file.exists():
    plate_source = plate_file.read_text(encoding="utf-8")
    for contract in ("OnPopulateMesh", "public override bool Raycast", "skewRatio", "outlineTint", "ScreenPointToLocalPointInRectangle"):
        if contract not in plate_source:
            errors.append(f"Shaped menu plate missing contract: {contract}")

# Enforce key touch-control contracts and normalized hitbox separation.
touch_file = root / "Assets/Aether/Scripts/Runtime/TouchHoldButton.cs"
if touch_file.exists():
    touch_source = touch_file.read_text(encoding="utf-8")
    for contract in ("IPointerDownHandler", "IPointerUpHandler", "activePointers", "ResetAll", "JumpRequested", "JumpHeld"):
        if contract not in touch_source:
            errors.append(f"Touch control missing contract: {contract}")

motor_file = root / "Assets/Aether/Scripts/Runtime/PrototypePlayerMotor.cs"
if motor_file.exists():
    motor_source = motor_file.read_text(encoding="utf-8")
    for contract in ("coyoteTime", "jumpBufferTime", "groundAcceleration", "airAcceleration", "fallGravityMultiplier"):
        if contract not in motor_source:
            errors.append(f"Movement controller missing mechanic: {contract}")

# Main-menu actions are deliberately spaced to keep individual tap targets distinct.
menu_rectangles = {
    "start": (0.22 - 0.30 / 2, 0.405 - 0.095 / 2, 0.22 + 0.30 / 2, 0.405 + 0.095 / 2),
    "controls": (0.125 - 0.18 / 2, 0.275 - 0.075 / 2, 0.125 + 0.18 / 2, 0.275 + 0.075 / 2),
    "about": (0.36 - 0.22 / 2, 0.275 - 0.075 / 2, 0.36 + 0.22 / 2, 0.275 + 0.075 / 2),
    "settings": (0.22 - 0.30 / 2, 0.16 - 0.072 / 2, 0.22 + 0.30 / 2, 0.16 + 0.072 / 2),
    "prototype_note": (0.205 - 0.36 / 2, 0.065 - 0.035 / 2, 0.205 + 0.36 / 2, 0.065 + 0.035 / 2),
}
for name, (x0, y0, x1, y1) in menu_rectangles.items():
    if x0 < 0 or y0 < 0 or x1 > 1 or y1 > 1:
        errors.append(f"Main-menu element {name} leaves normalized screen bounds")
menu_names = list(menu_rectangles)
for i, a in enumerate(menu_names):
    ax0, ay0, ax1, ay1 = menu_rectangles[a]
    for b in menu_names[i + 1:]:
        bx0, by0, bx1, by1 = menu_rectangles[b]
        if min(ax1, bx1) > max(ax0, bx0) and min(ay1, by1) > max(ay0, by0):
            errors.append(f"Main-menu interactive elements overlap: {a} and {b}")

# Normalized rectangles must remain disjoint so neighboring controls cannot steal touches.
rectangles = {
    "joystick": (0.145 - 0.218 / 2, 0.205 - 0.218 / 2, 0.145 + 0.218 / 2, 0.205 + 0.218 / 2),
    "jump": (0.875 - 0.154 / 2, 0.19 - 0.154 / 2, 0.875 + 0.154 / 2, 0.19 + 0.154 / 2),
    "menu": (0.91 - 0.076 / 2, 0.90 - 0.076 / 2, 0.91 + 0.076 / 2, 0.90 + 0.076 / 2),
}
for name, (x0, y0, x1, y1) in rectangles.items():
    if x0 < 0 or y0 < 0 or x1 > 1 or y1 > 1:
        errors.append(f"Touch control {name} leaves normalized screen bounds")
names = list(rectangles)
for i, a in enumerate(names):
    ax0, ay0, ax1, ay1 = rectangles[a]
    for b in names[i + 1:]:
        bx0, by0, bx1, by1 = rectangles[b]
        if min(ax1, bx1) > max(ax0, bx0) and min(ay1, by1) > max(ay0, by0):
            errors.append(f"Touch hitboxes overlap: {a} and {b}")

for script in root.glob("Assets/**/*.cs"):
    source = script.read_text(encoding="utf-8")
    if "\x00" in source:
        errors.append(f"Unexpected NUL byte in {script.relative_to(root)}")
    if source.count("{") != source.count("}"):
        errors.append(f"Unbalanced brace count in {script.relative_to(root)}")

if errors:
    print("\n".join("ERROR: " + e for e in errors))
    sys.exit(1)
print(f"Source structure checks passed for {len(required)} required files.")

joystick_file = root / "Assets/Aether/Scripts/Runtime/FixedVirtualJoystick.cs"
if joystick_file.exists():
    joystick_source = joystick_file.read_text(encoding="utf-8")
    for contract in ("IPointerDownHandler", "IDragHandler", "IPointerUpHandler", "HorizontalAxis", "travelRadiusRatio", "ResetAll"):
        if contract not in joystick_source:
            errors.append(f"Fixed virtual joystick missing contract: {contract}")
    if "activePointerId != int.MinValue" not in joystick_source or "knobRect.anchoredPosition = normalized * radius" not in joystick_source:
        errors.append("Fixed joystick must retain one active pointer and move only the knob within the fixed base")

layout_file = root / "Assets/Aether/Scripts/Runtime/GameplayControlHandle.cs"
if layout_file.exists():
    layout_source = layout_file.read_text(encoding="utf-8")
    for contract in ("IBeginDragHandler", "IDragHandler", "IEndDragHandler", "PlayerPrefs.SetFloat", "PlayerPrefs.Save()", "ChangeScale", "ResetAll", "ClampAnchor"):
        if contract not in layout_source:
            errors.append(f"Gameplay control layout editor missing contract: {contract}")

if bootstrap.exists():
    gameplay_source = bootstrap.read_text(encoding="utf-8")
    for contract in ("GameplayControlSprite", "GameplayControls/", "ShowControlLayout", "ChangeSelectedControlScale", "SaveControlLayout", "ResetControlLayout", "Time.timeScale = 0f", "Time.timeScale = 1f", 'pauseHandle.Configure("pause"'):
        if contract not in gameplay_source:
            errors.append(f"Gameplay controls implementation missing contract: {contract}")
    if 'MakeTouchButton("◀"' in gameplay_source or 'MakeTouchButton("▶"' in gameplay_source:
        errors.append("Separate left/right gameplay buttons must be removed in favor of a fixed joystick")

print("Source contracts and normalized hitbox separation passed. Unity compilation, Sprite import, real pixel hit-testing, RTL rendering, and Android builds still require Unity/device QA.")
