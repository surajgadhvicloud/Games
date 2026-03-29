---
name: Readme-update
agent: README Maintainer
description: "Update the repository root README.md after major changes; also update the Roadmap section when the user provides future work items."

---

Update the repository root `README.md` (at repo root) to reflect the current state of the codebase.

## Inputs

- If the user provides a change summary (PR notes), use it as a hint but **verify in code**.
- If the user provides roadmap/future work items, update **only** the `## Roadmap (Next)` section.

## Requirements

- Edit the existing root `README.md` in-place.
- Do not invent implemented features.
- Keep it skimmable (short bullets; avoid long prose).
- Commands must be executable in PowerShell and use repo-relative paths.

## When the user says “add to roadmap”

- Add the item(s) as unchecked checklist bullet(s) under `## Roadmap (Next)`.
- Do not change other sections unless the user explicitly asks.

## Output

- Apply the minimal patch necessary.
- Return a short summary of what changed in the README.