# Serenity Task List

## Current Version: v1.2.0B (Desktop Installer & Unified Boot)

### Completed Features
- [x] **Core Visual Programming**
    - [x] Blockly-based bot programming interface.
    - [x] Discord.js custom blocks (reply, persona say, message content).
    - [x] Real-time deployment to IslaMao bot.
    - [x] Verify end-to-end flow (Block -> DB -> Bot).
    - [x] Document system in `serenity.db`.
- [x] **Bi-Directional Control Foundation**
    - [x] Implement Presets System (Load/Save/Default).
    - [x] Integrate Branding (Logo, Header, About Screen).
    - [x] Migrate Core Logic to Visual Blocks (`Default.xml`).
- [x] **Emergency & Tools**
    - [x] **CRITICAL FIX**: Stopped infinite spam loop (Listener Caching).
    - [x] Added `!purge <N>` command (Variable bulk delete with 14-day filter).
    - [x] Added Channel Management blocks.
- [x] **Discord Logging Integration (v1.1.0)**
    - [x] Centralized logging to `serenity.db` (`SystemLogs` table).
    - [x] All services (Web UI, Bot, Monitor) log to database.
    - [x] Automatic Discord broadcast to `#bot-spam` and `#brain-activity`.
    - [x] Auto-create `#brain-activity` channel for Brain/Monitor logs.
    - [x] Batched log delivery (5s intervals, 1900 char chunks).
- [x] **Unified Boot System**
    - [x] Create `tools/boot_serenity.py` (Single window console).
    - [x] Aggregate logs from Web UI, Bot, and Nervous System.
- [x] **Installer & OTA System**
    - [x] Create `tools/setup_desktop.py` (Create Desktop Shortcuts).
    - [x] Create `tools/updater.py` (Git pull + DB migration).
    - [x] Create `tools/updater.py` (Git pull + DB migration).
    - [x] Create `install.bat` (One-click setup).
- [x] **Electron App Conversion (v1.3.0)**
    - [x] Create `electron-main.js` (Process Manager).
    - [x] Update `package.json` (Build config).
    - [x] Build `Serenity Setup 1.0.0.exe`.



## Next Phase: Advanced Interactions (v1.2.0)

- [ ] Enable external control via Discord dialogs, popups, and commands.
- [ ] Implement "Memory" blocks (reading/writing to DB).
- [ ] Add conditional logic for persona switching.
- [ ] Implement timed tasks and scheduled events.

## BF6Portal Tool Integration (v1.3.0)
- [ ] **Tabbed Menu System**: Implement sliding "Neural Link" (Gemini CLI) in BF6Portal.
- [ ] **Brain Sync**: Ensure BF6Portal blocks use `serenity.db` logic and core rules.


## System Health

**All Systems Functional:**
- ✅ Web UI (Blockly Editor): http://localhost:3000
- ✅ IslaMao Bot: Online, Neural Link Active
- ✅ Nervous System: Monitoring Dump directory
- ✅ Discord Logging: `#bot-spam` and `#brain-activity` active
