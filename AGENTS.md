# Agent Instructions — Unity Virtual Camera Publisher

Two things in one repo: a reusable Unity package (`com.meta.xr.virtualcamerapublisher/`) that lets a Quest app publish virtual cameras to Horizon OS, and a sample Unity project (`VirtualCameraPublisherSample/`) that consumes it.

## Source-of-truth files (read these first, do not duplicate their contents in this file)

For setup, build steps, SDK versions, and project layout, read:

- `README.md` — top-level overview and install paths (git URL vs local folder)
- `com.meta.xr.virtualcamerapublisher/README.md` — package integration instructions, manifest requirements, API usage
- `com.meta.xr.virtualcamerapublisher/package.json` — UPM package id, version, dependencies
- `VirtualCameraPublisherSample/ProjectSettings/ProjectVersion.txt` — sample's Unity editor version
- `VirtualCameraPublisherSample/Packages/manifest.json` — sample's Unity package versions
- `LICENSE` and `notices.html` — license terms

## Quest / Horizon-specific notes

- Default branch is `dev`, not `main` — don't assume `main` when scripting checkouts.
- To open the demo, point Unity at `VirtualCameraPublisherSample/` (not the repo root). To consume the package elsewhere, use the git URL with `?path=com.meta.xr.virtualcamerapublisher` shown in the README.
- The feature requires a real APK install — it does **not** work over Meta Horizon Link.
- `AndroidManifest.xml` must declare the `horizonos` XML namespace, the `CREATE_VIRTUAL_CAMERA` permission, and `<horizonos:uses-horizonos-sdk>` matching the package's documented minimum. Stripping or downgrading any of these silently breaks the feature.
- The top-level `README.md` description mis-titles this repo as a Passthrough Camera sample — it's actually about *publishing* virtual cameras. Ignore that line.

# Meta Quest tooling

This is a Meta Quest / Horizon OS sample. The bespoke intro above is the source of truth for what this project is and how it's built — use it (and the files it points at) instead of restating facts from memory.

When the user asks anything about Quest device behavior, build / deploy / debug / capture flows, on-device performance, or Horizon OS APIs, reach for these tools instead of generic Unity answers:

- **`hzdb`** — Quest-aware ADB wrapper (device list, install / launch / stop, logs, screenshots, Perfetto traces, on-device docs search). Already wired up as an MCP server via `.mcp.json`, `.vscode/mcp.json`, and `.cursor/mcp.json`. Also runnable directly: `npx -y @meta-quest/hzdb <subcommand>`.
- **Meta Quest Agentic Tools** — the full skill set, including Unity-specific skills: <https://github.com/meta-quest/agentic-tools>. Install per your client (Claude Code: `/plugin install meta-vr@meta-quest`; Gemini CLI: `gemini extensions install https://github.com/meta-quest/agentic-tools`; Cursor / VS Code: install the **Meta Horizon** extension from the Marketplace).

A few behavior expectations:

- **Read this repo's files first.** Before answering anything project-specific, read `README.md` and whichever source-of-truth files the intro above points at. Don't restate their contents in chat — quote or link instead.
- **Use `hzdb` for device-side work.** Anything that touches an attached Quest (install, launch, logs, screenshot, capture, manifest inspection) goes through `hzdb`, not raw `adb`.
- **Check live Horizon OS docs before answering API questions.** `hzdb docs search "..."` queries the live docs; training data on Horizon OS APIs goes stale fast.
- **Don't fabricate SDK / engine versions.** If a version isn't visible in this repo's files, say so rather than guessing.
