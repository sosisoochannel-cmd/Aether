# Aether Main Menu — Source-Only Design Review

## Visual direction

The menu has one clear primary action, two real secondary destinations, and one atmospheric focal point to the right. Typography is white only; hierarchy comes from size, bold weight, spacing, shadow, and alpha. Gold is reserved for non-text accents such as the primary outline and a restrained rift highlight.

Pattern-level references researched for this pass:

- Hollow Knight: dark negative space, restrained white title typography, and low-density floating particles.
- Prince of Persia: The Lost Crown: a clear primary selection framed by an atmospheric landscape.
- EA SPORTS FC Mobile: clearly distinguishable navigation choices and a strong primary action.
- Apple game-interface guidance: legibility, adaptable layout, and sufficiently large touch targets across screen shapes.
- Unity's legacy UI Graphic API: purpose-built meshes through VertexHelper; Unity also exposes a bold text style for legacy UI Text.

These are design patterns, not a request to copy another game's art, exact layout, marks, or assets.

## Current implementation in source

- Procedural background depth: dark layered silhouettes, a broken elliptical rift, a subtle core glow, a fine divider, and sparse drifting motes.
- Motion is unscaled-time driven and mesh updates are throttled to roughly 25 updates/second rather than regenerated every rendered frame.
- Main title and UI text normalize to white RGB; opacity is used for secondary information.
- Text uses the bold legacy UI style, a restrained shadow on larger lines, and staggered fade/slide reveals.
- Button plates use a custom slanted outline, with raycasting matched to the visible polygon.
- Buttons have a low-amplitude focus/press scale response and stay non-interactive during their reveal.
- Start, Controls, and About each have real destinations; the old “return to warning” menu action has been removed.

## Honest source-only score

| Area | Provisional score | Why |
|---|---:|---|
| Composition and hierarchy | 7/10 | A defined left text column balances the rift and silhouettes on the right. |
| Typography system | 7/10 | White-only text, bold weight, and opacity levels are implemented, but rendered sizing and Persian/RTL glyphs are not yet verified. |
| Interaction details | 7/10 | Polygon hit-testing and press response exist in source; event order and device behavior still need Play Mode testing. |
| Atmospheric motion | 7/10 | The backdrop has depth and restrained motion, but it is procedural art rather than a final hand-crafted environment illustration. |
| Responsive behavior | 6/10 | Safe-area anchors and relative positioning exist; no real device screenshots have been inspected. |
| Persian/Arabic correctness | 3/10 validation confidence | A shaping fallback is present, but it is not a full Unicode bidirectional implementation and has not been validated in rendered Unity output. |
| Actual in-game visual quality | Not scored | No Unity render or device capture is available, so claiming a definitive visual score would be dishonest. |

Overall, this is a **provisional 6.5/10 source implementation**, not a final visual acceptance score. The largest remaining uncertainty is how the UI actually renders and feels on devices, not whether more decorative code can be added.

## Acceptance checklist before a higher score

1. Open with Unity 6000.3.24f1 and resolve every compiler/package error.
2. Capture landscape screenshots at 16:9 and ultrawide phone ratios; inspect the actual visual hierarchy and safe-area margins.
3. Confirm every text glyph is white, secondary text remains readable, and bold weight does not crowd Persian/Arabic glyphs.
4. Verify the rift/particle motion is subtle, does not flicker, and does not cause visible frame-time spikes.
5. Test pointer-down/up order, fast taps during reveal, repeated taps, and transitions between Start, Controls, About, and Menu.
6. Test Persian text with punctuation, mixed-script phrases, and multi-line wrapping on a physical Android device.
7. Replace procedural silhouettes only if a deliberate art review shows that bespoke environment art would meaningfully improve the composition.

No APK or Unity build has been run for this pass. Source-check success must not be interpreted as runtime validation.
