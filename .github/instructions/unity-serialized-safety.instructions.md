---
description: "Use when editing Unity prefabs, scenes, ScriptableObject assets, or YAML-serialized project files. Enforces narrow, serialization-safe changes for WebGL and UI tasks."
applyTo: "Assets/**/*.{prefab,unity,asset,meta}"
---
# Unity Serialized Safety Rules

- Keep edits minimal and targeted to the requested object/component/asset.
- Do not reorder unrelated YAML blocks or rewrite whole files when a small change is enough.
- Preserve GUID and fileID references exactly; never regenerate references unless required by the task.
- Avoid broad scene-wide updates when a prefab or script-level fix is sufficient.
- For prefab variants, edit the nearest variant that owns the override instead of changing the base prefab by default.
- If a base prefab must change, call out expected downstream variant impact in the summary.
- When modifying UI objects, preserve anchors, pivots, and Canvas scaling behavior unless the task is explicitly about layout changes.
- Keep platform-specific behavior explicit in scripts instead of embedding risky serialized defaults.

# Verification Expectations

- Validate that modified assets still reference existing scripts and dependencies.
- Check for obvious serialization drift by scanning nearby unchanged blocks before finalizing edits.
- Report manual Unity Editor checks needed after the patch (prefab overrides, scene load, inspector warnings).
