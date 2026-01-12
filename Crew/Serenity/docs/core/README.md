# Serenity - Visual Discord Bot Programming

**Version 1.2.0B - Desktop Installer & Unified Boot**

Serenity is a visual programming environment for creating Discord bots using Blockly. It features a Neural Link to `serenity.db`, real-time deployment, and comprehensive Discord logging.

## ✨ Key Features

- **Visual Programming**: Drag-and-drop Blockly interface for bot logic.
- **Neural Link**: Brain-in-a-database architecture (`serenity.db`).
- **Live Deployment**: Instant updates to IslaMao bot via Nervous System.
- **Dual Persona Bot**: Isla (Grumpy) and Mao (Playful) personalities.
- **Presets System**: Save, load, and share workspace configurations.
- **Custom UI**: Dark mode, "Serenity OS" boot screen, branded toolbar.
- **Captain's Log**: Automated documentation and status tracking.
- **Discord Logging** (NEW v1.1.0):
  - All system logs broadcast to Discord channels
  - `#bot-spam`: Web UI and Bot logs
  - `#brain-activity`: Brain updates and Monitor logs
  - Automatic channel creation and batched delivery
- **Desktop Installer** (NEW v1.2.0B):
  - One-click setup (`install.bat`)
  - Unified Console (`boot.bat`)
  - OTA Updater (`tools/updater.py`)


## 🚀 Getting Started (Integrated Mode)

The AI Agent manages system processes internally to keep your desktop clean.

**If you need to run manually:**
1.  **Run the Enforcer**:
    ```powershell
    ./START.ps1
    ```
    *This will spawn separate windows for monitoring.*

2.  **Access Control Panel**: Open `http://localhost:3000`.
3.  **Program**: Drag blocks to create logic.
4.  **Deploy**: Click "Deploy to IslaMao" to update the bot instantly.

## 📂 Project Structure
- `web_ui/`: The React/Blockly frontend.
- `IslaMao/`: The Discord.js bot client.
- `tools/`: Nervous System (deployment monitor) and Brain utilities.
- `SavedWorkspaces/`: Preset configurations (Default.xml, etc.).
- `serenity.db`: The Brain (BotScripts, SystemDocs, SystemLogs, Memories).
- `CaptainsLog/`: Project documentation and status reports.

## 🎮 Bot Commands (Default Preset)

- `!joke`: Mao tells a cat-related joke
- `!scold`: Isla delivers a grumpy response
- `!logtest`: Tests channel management (sends to `#general-chat`)
- `!purge <N>`: Deletes N recent messages (1-1000, skips messages >14 days old)
- `@IslaMao`: Random persona response (40% Isla grumpy, 40% Mao playful, 20% both)

## 🧠 Discord Logging Channels

**#bot-spam**: Operational logs
- Web UI startup and build events
- Bot startup and Neural Link status
- Command execution logs

**#brain-activity**: Intelligence logs
- Documentation updates (Brain)
- File detection and deployment (Monitor)
- Memory storage events

## 🔧 System Components

1. **Web UI (Blockly)**: Visual editor at `localhost:3000`
2. **IslaMao Bot**: Discord client with Neural Link to Brain
3. **Nervous System**: File watcher for deployment automation
4. **Brain (serenity.db)**: Centralized storage for scripts, docs, logs, and memories

## 📊 Version History

- **v1.1.0** (Current): Discord logging integration, `!purge` command, crash-proof bulk delete
- **v1.0.0**: Initial release with Neural Link, Presets, and Dual Persona

## 🛠️ Development

Built with:
- **Frontend**: Blockly, TypeScript, Webpack
- **Backend**: Node.js, Express, SQLite3
- **Bot**: Discord.js v14.x
- **Monitoring**: Python (watchdog, sqlite3)

## 📝 License

MIT License - See LICENSE file for details.
