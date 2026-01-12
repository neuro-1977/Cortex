---
description: Protocols for interacting with the Serenity Brain (serenity.db)
---

# Brain Interaction Protocols

## 1. Prime Directive: Read BRAIN First
Before starting any task, answering a question, or planning a solution, the AI Agent MUST consult the `serenity.db` (The Brain).
- **Why**: To ensure continuity, avoid redundant work, and maintain alignment with the project vision and user preferences.
- **How**: Use `tools/toolbox/check_brain.py` or specific query scripts to retrieve relevant context.

## 2. Memory Persistence: Save to BRAIN
Any temporary note, scratchpad, research finding, or "random text file" created to remember something MUST be saved to the Brain.
- **Why**: To prevent data loss and ensure that knowledge gained in one session is available in future sessions.
- **How**:
    1.  Write the content to a file (e.g., in `Dump/`).
    2.  Run `python tools/ingest_to_brain.py "<filepath>" "<Document Name>"`.

## 3. History Archival
Regularly summarize and archive conversation history to the Brain to maintain a long-term memory of project evolution.
