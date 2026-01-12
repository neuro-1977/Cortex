# serialization.ts Analysis (Blockly Workspace Persistence)

## Overview

The `serialization.ts` file handles the saving and loading of the Blockly workspace state, ensuring that user-created block configurations can be persisted across sessions. It employs a dual storage mechanism and utilizes Blockly's native serialization APIs.

## Key Findings

1.  **Dual Storage Mechanism**:
    *   **Browser's Local Storage**: The workspace state is saved to and loaded from `window.localStorage` using the key `mainWorkspace`. This provides a client-side, ephemeral form of persistence.
    *   **Backend NeDB / API**: The workspace state is also posted to a backend API endpoint (`/api/serenity/workspace`) via a `fetch` request. This suggests a more robust, server-side persistence mechanism, likely using NeDB as mentioned in other project documentation.
    *   **Loading Priority**: When `load()` is called, it first attempts to retrieve the workspace state from the backend API. If the backend response is not `ok` or if no state is returned, it falls back to loading from `window.localStorage`.

2.  **Blockly Serialization API Usage**:
    *   `Blockly.serialization.workspaces.save(workspace)`: Used to convert the current state of the Blockly workspace into a serializable JSON object.
    *   `Blockly.serialization.workspaces.load(state, workspace, undefined)`: Used to reconstruct the Blockly workspace from a previously saved JSON state.

3.  **Event Handling during Load**:
    *   `Blockly.Events.disable()` and `Blockly.Events.enable()`: These calls are correctly used to wrap the `Blockly.serialization.workspaces.load()` operation. This is a best practice to prevent a flood of Blockly events (e.g., block creation, movement, connection events) from firing during a bulk load, which can lead to performance issues or unintended side effects.

## Connections to Block Connection Issues

1.  **Data Integrity and Consistency**:
    *   **Core Problem**: The most significant implication for "blocks import but don't connect" lies in the *consistency* of the serialized `state` data with the *currently loaded Blockly block definitions*.
    *   If a workspace was saved when a particular block (e.g., `MOD_BLOCK`, `RULE_HEADER`) had a specific set of connection inputs/outputs (`check` arrays), but is then loaded into a Blockly environment where that block's definition has changed (e.g., input names are different, `check` types are stricter/looser, input is removed), the Blockly engine will not be able to re-establish the connections. It might load the blocks visually but leave them disconnected.
2.  **Backend State Outdated**: The `CaptainsLog` entry "Loading Default from dropdown loads old content" directly relates to this. If the `/api/presets/Default.xml` or `/api/serenity/workspace` backend endpoints are serving outdated workspace data (either XML or the JSON `state` object), then the loaded blocks will be based on old definitions. Even if the frontend has updated block definitions, the deserialized blocks will have connection properties from the old definitions, leading to connection failures.
3.  **BF6JSONParser Interaction**: The `BF6JSONParser` in `index.ts` is responsible for parsing raw BF6 JSON. If the output of this parser creates blocks with connection properties that don't match the current definitions loaded from `bf6portal.ts`, `definitions.ts`, etc., then those newly imported blocks will also fail to connect. The `serialization.ts` primarily deals with Blockly's *internal* serialization format, but the *source* of that data (user saves, API presets, or BF6 imports) is where consistency issues arise.

## Summary for River:

`serialization.ts` effectively manages Blockly workspace persistence. However, it's a critical point where discrepancies between *saved block state* and *current block definitions* can lead to connection issues. The mechanism correctly uses Blockly's API, so the problem is unlikely with the serialization process itself, but rather with the *data content* being serialized or deserialized. Outdated data from the backend (`/api/serenity/workspace` or `Default.xml` presets) is a strong candidate for causing "old content" and connection problems upon loading.