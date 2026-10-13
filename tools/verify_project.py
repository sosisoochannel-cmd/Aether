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
    "Assets/Aether/Scripts/Runtime/TouchHoldButton.cs",
    "Assets/Aether/Scripts/Runtime/PrototypePlayerMotor.cs",
    "Assets/Aether/Editor/AetherProjectSetup.cs",
    "docs/ARCHITECTURE.md",
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
print("Note: Unity compilation, play-mode behavior, RTL rendering, and Android builds are not tested by this script.")
