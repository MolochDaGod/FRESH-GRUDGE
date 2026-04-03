---
description: "Run a focused Unity WebGL release readiness pass: inspect settings, scripts, UI behavior, and deployment assumptions; then produce a concrete fix plan or apply requested edits."
name: "WebGL Release Check"
argument-hint: "Describe target build, problem symptoms, and deployment host (CDN/itch.io/custom)."
agent: "Unity WebGL Shipwright"
---
Perform a Unity WebGL release readiness check for this workspace.

Inputs to consider:
- User-reported symptoms and constraints.
- Current scripts, prefabs/scenes, and relevant project settings.
- Deployment host assumptions (compression, caching, headers, template behavior).

Tasks:
1. Identify highest-risk WebGL issues first (runtime compatibility, loading, memory, asset references, browser input/UI scaling).
2. Propose the smallest safe patch set.
3. If the user asked for implementation, apply edits directly.
4. Provide a verification checklist split into: quick checks, editor checks, and deployment checks.

Output format:
1. Top findings (severity ordered).
2. Files to change (or changed files).
3. Concrete patch plan or patch explanation.
4. Validation checklist.
5. Residual risks and follow-up recommendations.
