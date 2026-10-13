#!/usr/bin/env python3
"""Fast source-structure checks for Aether; not a substitute for Unity compilation."""
import json
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
    for contract in ("AnimateLogoReveal", "SoftGlintSprite", "PlayLogoGlint", "Screen.safeArea"):
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
    for contract in ("BuildMenuAtmosphere", "ShowControls", "ShowAbout", "ShowSettings", "ToggleMenuMotion", "ToggleTextSize", "ApplyMenuPreferences", "PlayerPrefs.Save()", "FontStyle.Bold", "Mathf.Clamp01(color.a)", "RevealMenuElement", "menuEntrancePlayed"):
        if contract not in source:
            errors.append(f"Main menu quality contract missing: {contract}")
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
    for contract in ("WaitForSecondsRealtime", "Time.unscaledDeltaTime", "IsComplete", "graphic.raycastTarget", "MotionEnabled", "isButtonGraphic"):
        if contract not in reveal_source:
            errors.append(f"UI entrance animation missing contract: {contract}")
    if "button.interactable = false" in reveal_source or "button.interactable = true" in reveal_source:
        errors.append("UI entrance animation must not block interaction while the menu appears")
    if "graphic.raycastTarget = isButtonGraphic" not in reveal_source:
        errors.append("UI reveal must preserve the appropriate raycast target for buttons and labels")
    if "graphic.raycastTarget = false" in reveal_source:
        errors.append("UI reveal must not disable button raycasts during fade-in")

feedback_file = root / "Assets/Aether/Scripts/Runtime/AetherUIButtonFeedback.cs"
if feedback_file.exists():
    feedback_source = feedback_file.read_text(encoding="utf-8")
    for contract in ("IPointerDownHandler", "IPointerUpHandler", "HashSet<int>", "Mathf.Exp", "IsComplete", "MotionEnabled"):
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
    for contract in ("IPointerDownHandler", "IPointerUpHandler", "activePointers", "ResetAll"):
        if contract not in touch_source:
            errors.append(f"Touch control missing contract: {contract}")

motor_file = root / "Assets/Aether/Scripts/Runtime/PrototypePlayerMotor.cs"
if motor_file.exists():
    motor_source = motor_file.read_text(encoding="utf-8")
    for contract in ("coyoteTime", "jumpBufferTime", "groundAcceleration", "airAcceleration", "fallGravityMultiplier"):
        if contract not in motor_source:
            errors.append(f"Movement controller missing mechanic: {contract}")

# Normalized rectangles must remain disjoint so neighboring controls cannot steal touches.
rectangles = {
    "left": (0.09 - 0.105 / 2, 0.16 - 0.16 / 2, 0.09 + 0.105 / 2, 0.16 + 0.16 / 2),
    "right": (0.215 - 0.105 / 2, 0.16 - 0.16 / 2, 0.215 + 0.105 / 2, 0.16 + 0.16 / 2),
    "jump": (0.89 - 0.14 / 2, 0.17 - 0.19 / 2, 0.89 + 0.14 / 2, 0.17 + 0.19 / 2),
    "menu": (0.94 - 0.08 / 2, 0.92 - 0.07 / 2, 0.94 + 0.08 / 2, 0.92 + 0.07 / 2),
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
print("Source contracts and normalized hitbox separation passed. Unity compilation, real pixel hit-testing, RTL rendering, and Android builds still require Unity/device QA.")
