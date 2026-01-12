# index.ts Analysis (Web UI Entry Point)

## Overview

`index.ts` is the main entry point for the Serenity Web UI. It is responsible for initializing the Blockly workspace, registering blocks and generators, handling file imports (XML, JSON, TS/TXT), and setting up the user interface (toolbar, save/load functionality).

## Key Findings

1.  **Blockly Initialization**:
    *   Imports `Blockly` and `DarkTheme` from `@blockly/theme-dark`.
    *   Injects Blockly into `blocklyDiv` with specific configurations:
        *   `toolbox`: Uses the `toolbox` object imported from `./toolbox`. This confirms the structure analyzed previously.
        *   `theme`: `DarkTheme` is applied.
        *   `grid`, `zoom`, `move`: Standard Blockly workspace settings.

2.  **Block and Generator Registration**:
    *   Registers several block sets:
        ```typescript
        Blockly.common.defineBlocks(textBlocks); // From './blocks/text'
        Blockly.common.defineBlocks(homeBlocks); // From './blocks/home'
        Blockly.common.defineBlocks(bf6PortalBlocks); // From './blocks/bf6portal'
        Blockly.common.defineBlocks(discordBlocks); // From './blocks/discord'
        ```
        This confirms where the primary block definitions for different domains are located.
    *   **Dynamic Block Loading from Database ("BRAIN")**: A `fetch` request is made to `/api/blocks`. The response (JSON array of blocks) is then used to dynamically define additional blocks using `Blockly.common.createBlockDefinitionsFromJsonArray` and `Blockly.common.defineBlocks`.
        *   **Implication**: Block definitions are not solely static files; they can be loaded from the `serenity.db` (referred to as "BRAIN"). This is crucial for understanding how block definitions are managed and updated.
    *   Generator registration: `Object.assign(javascriptGenerator.forBlock, forBlock);` This assigns generators imported from `./generators/javascript` to `blockly/javascript`.

3.  **Debugging Checks**:
    *   `console.log("MOD_BLOCK defined?", !!Blockly.Blocks['MOD_BLOCK']);`
    *   `console.log("RULE_HEADER defined?", !!Blockly.Blocks['RULE_HEADER']);`
        These lines indicate a specific interest in ensuring these key BF6 blocks are properly defined on startup.

4.  **File Import Mechanism**:
    *   The UI includes a file input (`fileInput`) that accepts `.xml`, `.json`, `.ts`, and `.txt` files.
    *   **`.xml` Import**: Handled by standard `Blockly.utils.xml.textToDom` and `Blockly.Xml.domToWorkspace`.
    *   **`.json` Import (BF6 Portal JSON)**: Instantiates `BF6JSONParser(ws)` and calls `parser.parse(json)`. This confirms `BF6JSONParser` is the dedicated logic for converting raw BF6 JSON into Blockly workspace. This is the entry point for the import process that leads to `PLACEHOLDER_BLOCK` creation.
    *   **`.ts`/`.txt` Import (Portal Logic)**: Instantiates `BF6Importer(ws)` and calls `importer.parseAndUpdateBlocks(text)`. This is the entry point for importing BF6 Portal text-based logic (potentially raw script) and updating the Blockly workspace.

5.  **Workspace Persistence**:
    *   `save(ws!)` and `load(ws)` from `./serialization` are used with `ws.addChangeListener`. This indicates that workspace state changes are saved automatically, likely to local storage or the backend.
    *   `loadDefaultWorkspace()` is present but commented out with `// Starting with empty workspace (Default disabled).`. This aligns with `CaptainsLog` entry about creating a blank `Default.xml` and disabling auto-load.

6.  **Export Functionality**:
    *   "Download XML" uses `Blockly.Xml.workspaceToDom` and `Blockly.Xml.domToText`.
    *   "Export .ts (BF6 Portal)" instantiates `BF6Exporter()` and calls `exporter.generateCode(ws)`. This is the entry point for converting Blockly workspace to BF6 Portal TypeScript code.
    *   "Export .ts (Discord Bot)" uses `javascriptGenerator.workspaceToCode(ws)`. This is the standard Blockly JavaScript generator for Discord bot logic.

7.  **Terminal Integration**: `TerminalDrawer` is initialized, suggesting a console/terminal embedded within the UI.

## Implications for Block Connection Issues

*   **Dynamic Block Definitions**: The ability to load blocks from the "BRAIN" (`serenity.db`) means that the set of available blocks can change. If `serenity.db` contains outdated or incomplete block definitions, this could contribute to connection failures during import.
*   **Centralized Import/Export Logic**: `BF6JSONParser` and `BF6Importer` are critical components. Any issues in their parsing logic or block creation (especially for connections) will directly manifest as unconnected blocks.
*   **Generator Discrepancies**: The distinction between `BF6Exporter` (for BF6 Portal TS) and `javascriptGenerator` (for Discord bot TS) is important. If the `bf6PortalBlocks` do not have corresponding, fully implemented generators in `bf6_generators.ts` (or if `bf6_stubs.ts` is still in use), the export will be incomplete or incorrect.
*   **Workspace State**: Issues with `serialization.ts` (as mentioned in `CaptainsLog` regarding "Loading Default from dropdown loads old content") could also indirectly affect how blocks behave on load if they are not correctly re-initialized.

---
**Next Step**: Analyze `web_ui/src/generators/bf6_generators.ts`.
