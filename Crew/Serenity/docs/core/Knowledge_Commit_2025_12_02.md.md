# Knowledge Commit - 2025-12-02

## 1. JSON Import Issues
We identified why `custom_conquest_template_7.2.json` fails to import correctly.

### Root Causes
1.  **Case Sensitivity**: The JSON file uses `camelCase` for block types (e.g., `modBlock`, `ruleBlock`), while `bf6portal.ts` defines them in `UPPER_CASE` (e.g., `MOD_BLOCK`, `RULE_HEADER`).
2.  **Missing Block Definitions**: A significant number of standard blocks used in the JSON are missing from `bf6portal.ts`.

### Missing Blocks List
The following block types were found in the JSON but are likely missing or need aliases in `bf6portal.ts`:
-   `SetVariable`, `GetVariable`, `variableReferenceBlock`
-   `EventPlayer`, `EventTeam`, `EventOtherPlayer`
-   `GetTeam`, `GetPlayer`, `GetSoldierState`
-   `GetCapturePoint`, `GetCaptureProgress`, `GetCurrentOwnerTeam`
-   `PlaySound`, `PlayVO`
-   `Wait`, `WaitUntil`, `While`, `If`
-   `And`, `Or`, `Not`, `Equals`, `NotEqualTo`
-   `Add`, `Subtract`, `Multiply`, `Divide`

## 2. Toolbox Contents
The `tools/toolbox` directory contains various utility scripts for maintaining the Serenity environment and Brain.

### Key Scripts
-   `boot_serenity.py`: Likely for starting the main application.
# Knowledge Commit - 2025-12-02

## 1. JSON Import Issues
We identified why `custom_conquest_template_7.2.json` fails to import correctly.

### Root Causes
1.  **Case Sensitivity**: The JSON file uses `camelCase` for block types (e.g., `modBlock`, `ruleBlock`), while `bf6portal.ts` defines them in `UPPER_CASE` (e.g., `MOD_BLOCK`, `RULE_HEADER`).
2.  **Missing Block Definitions**: A significant number of standard blocks used in the JSON are missing from `bf6portal.ts`.

### Missing Blocks List
The following block types were found in the JSON but are likely missing or need aliases in `bf6portal.ts`:
-   `SetVariable`, `GetVariable`, `variableReferenceBlock`
-   `EventPlayer`, `EventTeam`, `EventOtherPlayer`
-   `GetTeam`, `GetPlayer`, `GetSoldierState`
-   `GetCapturePoint`, `GetCaptureProgress`, `GetCurrentOwnerTeam`
-   `PlaySound`, `PlayVO`
-   `Wait`, `WaitUntil`, `While`, `If`
-   `And`, `Or`, `Not`, `Equals`, `NotEqualTo`
-   `Add`, `Subtract`, `Multiply`, `Divide`

## 2. Toolbox Contents
The `tools/toolbox` directory contains various utility scripts for maintaining the Serenity environment and Brain.

### Key Scripts
-   `boot_serenity.py`: Likely for starting the main application.
-   `check_brain.py`, `cleanup_brain.py`, `organize_brain.py`, `restore_brain.py`, `update_brain.py`: Brain maintenance tools.
-   `commit_firefly_to_brain.py`, `commit_personal_context.py`, `commit_user_context.py`: Context ingestion tools.
-   `sync_all_workspaces.py`, `sync_from_serenity.py`: Workspace synchronization.

## Debugging "Invisible Blocks" (2025-12-02)
- **Symptom:** Blocks are imported (count is correct, e.g., 765 blocks), but are not visible in the workspace.
- **Coordinates:** Blocks are placed at valid coordinates (e.g., 100, 100), verified by logs.
- **Rendering:** `blocklyDiv` has valid dimensions (not 0x0).
- **Theme:** Switching between `bf6Theme` and standard `DarkTheme` did not resolve the issue, ruling out a broken theme definition.
- **Hypothesis:**
    1.  **Z-Index/Overlay:** A custom UI element (like `terminal-drawer` or a modal) might be overlaying the workspace.
    2.  **Build Cache:** The dev server might be serving stale code, ignoring recent fixes.
    3.  **CSS Visibility:** A global CSS rule might be inadvertently hiding SVG elements.
- **Next Steps:** Restart environment to clear cache. If issue persists, inspect DOM for overlays.

## 3. Current Plan Status
-   **Importer Upgrade**: We are moving to a TypeScript SDK native parser.
-   **JSON Support**: We are fixing the JSON parser to handle legacy/standard block types by making matching case-insensitive and adding missing definitions.
