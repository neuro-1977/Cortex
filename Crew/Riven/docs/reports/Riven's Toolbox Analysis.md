# Toolbox Analysis (web_ui/src/toolbox.ts)

## Overview

The `toolbox.ts` file defines the structure and content of the Blockly toolbox presented to the user. It is organized into several high-level categories, demonstrating the hybrid nature of the Serenity project (general programming logic, Battlefield Portal logic, and Discord bot logic).

## Key Findings

1.  **Hybrid Project Confirmation**: The top-level categories "Home", "BF6 Portal", and "Discord" clearly indicate that Serenity is designed to support multiple domains of visual programming.

2.  **"Home" Category (General Blockly)**:
    *   Contains standard Blockly categories: Logic, Loops, Math, Text, Colour, Lists, Variables, and Functions.
    *   Blocks like `logic_null` are explicitly marked `disabled: "true"`, suggesting intentional exclusion or work-in-progress.
    *   "Variables" and "Functions" categories use `custom: "VARIABLE"` and `custom: "PROCEDURE"` respectively, implying dynamic loading or management of these block types by Blockly's built-in systems.

3.  **"BF6 Portal" Category (Battlefield Specific)**:
    *   **Core Structure**: The "MOD" category groups `MOD_BLOCK`, `RULE_HEADER`, and `CONDITION_BLOCK`, reinforcing their foundational roles in constructing BF6 Portal logic.
    *   **Event Handling**: The "EVENTS" category is well-structured with "Game Events" (`ON_START`, `ON_PLAYER_JOIN`) and "Event Payloads" (`EVENTATTACKER`, `EVENTDAMAGE`, etc.). This indicates how event triggers and their associated data are organized.
    *   **Comprehensive BF6 Logic**: Extensive categories cover various aspects of BF6 Portal scripting:
        *   LOGIC (Control Flow, Boolean Logic, Comparison, Loops)
        *   PLAYER (Player manipulation, data retrieval)
        *   AI (Behavior, Spawning, State)
        *   MATH (BF6-specific mathematical operations)
        *   ARRAYS (Array manipulation)
        *   GAMEPLAY (Game mode, score, time limits)
        *   SUBROUTINE (Definition and calling of subroutines)
        *   UI (Messages, HUD)
        *   TRANSFORM (Vector operations, directions)
        *   AUDIO (Sound and music control)
        *   CAMERA (Camera manipulation, modes)
        *   EFFECTS (Visual and screen effects)
        *   OBJECTIVE (Objective state management)
        *   VEHICLES (Contains a `PLACEHOLDER_BLOCK` for unimplemented vehicle logic)
        *   OTHER (Currently only `COMMENT`)
    *   **Explicit Placeholders**: A dedicated "Placeholders" category explicitly includes `PLACEHOLDER_BLOCK` and `PLACEHOLDER_VALUE`, highlighting their use for unimplemented or unrecognized blocks. The "VEHICLES" category also uses a `PLACEHOLDER_BLOCK` with a specific message "Vehicle blocks not implemented".

4.  **"Discord" Category (Discord Bot Specific)**:
    *   Organized into: Events, Personas, Actions, Utils, Channels, Commands, and Bot Management.
    *   This confirms that Serenity includes a robust set of blocks for building Discord bot functionality.

## Implications for Block Connection Issues

*   **Expected Hierarchy**: The toolbox structure, especially within "BF6 Portal", strongly implies a hierarchical connection model. For instance, `RULE_HEADER` is nested under "MOD", and `CONDITION_BLOCK` is alongside them, suggesting a relationship. Events, like `ON_PLAYER_JOIN`, are logically distinct from `MOD_BLOCK`s but are typically associated with `RULE_HEADER`s.
*   **Unimplemented Blocks**: The explicit use of `PLACEHOLDER_BLOCK` for "Vehicle blocks not implemented" directly points to areas where Blockly definitions are missing. This is a primary cause for blocks importing but not connecting correctly, as they lack proper connection points.
*   **Connection Checks**: The sheer number of BF6-specific blocks means there's a vast array of potential input and output types. Correctly defined `check` arrays for `previousStatement`, `nextStatement`, `output`, and `input` connections are critical. Any mismatch will result in blocks not connecting, even if they render.

---
**Next Step**: Analyze `web_ui/src/index.ts`.
