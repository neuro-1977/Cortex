# Context Internalization - 2025-12-02

## 🧠 System Identity & Protocols
- **Identity**: Serenity (God Mode/Personal) vs. BF6Portal Tool (Public/Restricted).
- **Core Rule**: "Captain First" - Read Brain -> Check Toolbox -> Read Docs.
- **Memory**: `serenity.db` is the source of truth. All new knowledge must be ingested.

## 📜 Recent History & Status
- **Current Focus**: Debugging "Invisible Blocks" and JSON Import issues.
- **Critical Issues**:
    - **Model Mismatch**: CLI uses Gemini 1.5 Pro, Agent uses Gemini 2.0 Flash.
    - **Stale Server**: Background Node processes (PIDs 23768, 24040) are keeping old code active.
    - **JSON Import**: Case sensitivity mismatches (`camelCase` vs `UPPER_CASE`) and missing block definitions in `bf6portal.ts`.
    - **Invisible Blocks**: Blocks exist in memory but don't render. Suspected CSS/Overlay or Stale Cache.

## 📂 Key File Locations
- **Docs**: `_SERENITY.md`, `_BATTLE_PLAN.md`, `CORE_RULES.md`.
- **Logs**: `CaptainsLog.md` (High level), `Dump/` (Detailed technical commits).
- **Tools**: `tools/toolbox/` contains maintenance scripts.

## 🤖 Action Plan (Pending User Command)
1.  **Kill Stale Processes**: Terminate PIDs 23768, 24040.
2.  **Fix JSON Import**: Implement case-insensitive matching and add missing blocks.
3.  **Debug Visibility**: Verify fix after killing stale server.
