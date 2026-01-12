# UNKNOWN Block Handling Mechanism

## How "UNKNOWN" Blocks are Created and Displayed

The research confirms the mechanism by which unrecognized Battlefield Portal block types are handled within the Serenity project's Blockly implementation.

## Key Findings:

1.  **Detection and Warning**:
    *   **File**: `web_ui/src/logic/bf6_json_parser.ts` (L139)
    *   When the `BF6JSONParser` encounters an `originalType` from the BF6 JSON that does not have a corresponding Blockly block definition, it logs a warning:
        ```typescript
        console.warn(`BF6JSONParser: Unknown block type '${originalType}'. Creating placeholder.`);
        ```
    *   This explicitly indicates that the parser recognizes an unknown type and proactively flags it.

2.  **`PLACEHOLDER_BLOCK` Instantiation**:
    *   **File**: `web_ui/src/logic/bf6_json_parser.ts` (L141)
    *   Following the detection of an unknown type, a `PLACEHOLDER_BLOCK` is instantiated in the Blockly workspace:
        ```typescript
        const placeholder = this.workspace.newBlock('PLACEHOLDER_BLOCK') as Blockly.BlockSvg;
        ```
    *   This confirms that `PLACEHOLDER_BLOCK` serves as the visual representation for any unrecognized BF6 Portal block.

3.  **Displaying the Unknown Type**:
    *   **File**: `web_ui/src/logic/bf6_json_parser.ts` (L144)
    *   The `originalType` (the name of the unrecognized block from the BF6 JSON) is then used to set a field within the newly created `PLACEHOLDER_BLOCK`:
        ```typescript
        placeholder.setFieldValue(`UNKNOWN: ${originalType}`, 'NOTE');
        ```
    *   This is the source of the "UNKNOWN: [typename]" message observed in the `CaptainsLog`.

4.  **`PLACEHOLDER_BLOCK` Message Definition**:
    *   **File**: `web_ui/src/blocks/bf6portal.ts` (L1441)
    *   The `PLACEHOLDER_BLOCK` itself has a message definition that allows it to display this information:
        ```typescript
        "message0": "UNKNOWN BLOCK %1 Type: %2 %3 Data: %4",
        ```
    *   This message structure accommodates displaying the specific unknown type (`%2`) and potentially other data.

## Implications for Debugging and Connection Issues:

*   **Direct Debugging Pathway**: The "UNKNOWN: [typename]" message provides a direct path for debugging. Each `[typename]` represents a missing or incorrectly defined Blockly block for a BF6 Portal element.
*   **Connection Failure**: As hypothesized in the `PLACEHOLDER_BLOCK` analysis, these blocks are unlikely to have proper connection points defined. Therefore, any imported structure that includes `PLACEHOLDER_BLOCK`s will inevitably lead to broken connections, as valid Blockly blocks cannot connect to an undefined interface.
*   **Prioritization**: Identifying and implementing proper Blockly definitions for these "UNKNOWN" types is a critical step towards resolving the block connection issue.

---
**Next Step**: Consolidate initial block research findings and move to analysis of other `.ts` and `.json` files.
