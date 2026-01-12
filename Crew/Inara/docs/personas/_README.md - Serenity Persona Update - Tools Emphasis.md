# Serenity's Logbook - Your Guide to the 'Verse

### Gemini/LLM Chat Integration - Our AI Crew

Serenity now boasts a built-in chat panel on the HOME tab, ready for the likes of Gemini or any other capable LLM backend. Consider it our dedicated Comms for advanced intel:

- To link up Gemini or another LLM, just update the backend endpoint `/api/serenity/chat` in `blockly-workspace/src/server.ts` to call your local or cloud LLM API.
- This Comms panel will transmit your messages and display the intel received.
- Use it for navigating our workspace, getting a hand with a tricky bit, or just a friendly chat.

**A Quick Guide:**

1.  Get your local Gemini/LLM server running, or hook into a cloud API.
2.  In `server.ts`, replace the chat endpoint logic with an HTTP request to your chosen LLM.
3.  Restart our backend, and you'll find the Comms panel waiting in Serenity's HOME tab.

For the deeper workings and cunning plans, cast an eye on `_INTERNALS.md`.

# Serenity Help System - Your Chart to the Stars (v1.0.2beta)

Serenity now features a robust, local-first help system, a true treasure map for navigating our operations:

- All the charts, guides, and references are stored right here in our local data vault (`serenity_help.db`), ready even when we're off the grid.
- Access your help via the Help tab/button in the UI, or by clicking those handy “?” icons scattered throughout the workspace.
- Search for topics, browse through categories, or check the pinned quick references (Getting Started, FAQ, etc.).
- Add or edit your own wisdom directly from the UI (local only).
- Quick links to `_README.md`, `_TROUBLESHOOTING.md` (How we deal with the inevitable bumps and scrapes), `_FILES.md` (Our cargo manifest, detailing every hold and cranny), `_VISION.md` (The grand plan, where we're headed), and `_INTERNALS.md` (The inner workings, for those who like to peek under the hood) are right there in the help panel.
- All this intel is available offline and can be expanded by any of our crew.

## Using the Help System - Charting Your Course

1.  Click the Help button/tab in the Serenity UI.
2.  Search for topics, browse categories, or view pinned quick references (Getting Started, FAQ, etc.).
3.  Click any “?” icon in the UI for contextual help.
4.  To contribute your own insights, use the “Add Topic” or “Edit” buttons in the help panel.

## Contributing to the Logbooks - Sharing the Knowledge

- All help content is stored locally and can be updated by any of our crew.
- Use Markdown for formatting, code samples, and links – keep it clean and readable.
- Help topics are versioned, just like our ship's upgrades, and can be updated as our workspace evolves.

## API Endpoints - Our Comms Channels

- `POST /api/serenity/help` — To add or update a help topic.
- `GET /api/serenity/help/:topic` — To retrieve a help topic by name.
- `GET /api/serenity/help?q=search` — To search our help logs.

## See Also - Other Important Charts

- `_TROUBLESHOOTING.md` — How we deal with the inevitable bumps and scrapes.
- `_FILES.md` — Our cargo manifest, detailing every hold and cranny.
- `_VISION.md` — The grand plan, where we're headed.
- `_INTERNALS.md` — The inner workings, for those who like to peek under the hood.

# SERENITY WORKSPACE - READY FOR LAUNCH! (v1.0.2beta)

Welcome aboard Serenity, Captain! This here is your all-in-one workspace for cunning block-based logic, keeping our Discord bots shiny, and all the BF6Portal intel you could ask for.

---

## 🚀 Quick Start - Setting Our Course (v1.0.2beta)

1.  **Run Everything:**
    - Right-click `_WORKSPACE-START.ps1` and select **Run with PowerShell**.
    - This will fire up all our core services (Blockly, Serenity_Bot) in separate terminals, with health checks and status messages to keep you informed.

2. **Manual Start (for a closer look, should you need it):**
   - Blockly: `cd blockly-workspace && npm run start -- --port 8081`
   - Serenity_Bot: `cd Serenity_Bot && node index.js`

---

## 📂 Folder Guide - Our Ship's Compartments
- `blockly-workspace/` — The main controls for our Serenity UI, where all the cunning logic and menus reside.
- `Serenity_Bot/` — Our trusty Serenity_Bot, keeping an ear to the ground (check `.env` for its comms token).
- `BF6Resources/` — All the vital reference data and assets for our BF6Portal operations.
- `database/` — The secure vault for our NeDB data files.
- `public/`, `src/`, `tools/` (Our very own toolbox, filled with essential scripts for maintaining the ship and processing data), etc. — Supporting gear and assets, making everything run smooth.

---

## 📝 Documentation - Our Logbooks
- Take a look at `_README.md` (this very log) for a quick overview of our workspace and how to get going.
- For the full, detailed history, policies, and upgrades, consult `README.md`.
- See `_VISION.md` to remember the grand journey we're on, our goals, and the path ahead.

---

## 🛠️ Tips - A Bit of Pilot's Wisdom
- All our vital charts and logs (`.md` files) are marked with an `_` to keep them front and center in your view.
- To keep your console clear, make sure any logs or console messages that pop up are marked with a `DEBUG: ` prefix. This keeps the chatter from filling up your screen too fast.
- For best results, keep this workspace shipshape and up to date after every change you make. It helps us all fly true.

---

_Last updated: 2025-11-30 - Persona Infusion Complete._