# Cortex - Research Engine
**NotebookLM-Inspired Research Application**  
**Status:** Standalone App - Extracted from Serenity

---

## 🌟 Overview

**Cortex** is a standalone research engine application inspired by NotebookLM. It enables users to ingest documents, research with AI using RAG (Retrieval-Augmented Generation), and generate artifacts (Slide Decks, Mind Maps, Audio/Video Overviews, Quizzes, Flashcards).

**Mission:** Build a modular research application that mirrors NotebookLM's capabilities (Sources, Studio, Main Stage) while integrating advanced AI (Grok/Gemini) and media synthesis.

---

## ✨ Features

### Current Features (From Serenity)

- ✅ **Sources Panel:** PDF/Web/Text/Image ingestion
- ✅ **Stage (Chat):** Interactive research chat with citations
- ✅ **Studio:** Artifact generation (Slide Decks, Mind Maps, Audio/Video Overviews, Quizzes, Flashcards)
- ✅ **RAG System:** Retrieval-Augmented Generation for context-aware responses
- ✅ **AI Integration:** Gemini, Grok/xAI, Ollama support
- ✅ **Media Generation:** Video/audio overviews, slide decks, visuals
- ✅ **Crew Personas:** AI personas with distinct voices and roles

### Coming Soon (Upgrades from Serenity)

- 🚀 **EchoBay UI Theme:** Modern ChatPill interface and sleek Avalonia theme
- 🚀 **Enhanced IDE Features:** Code editor, terminal, file browser
- 🚀 **Advanced Code Analysis:** Scribe integration for intelligent code understanding
- 🚀 **Automation:** Voice control and GUI automation
- 🚀 **REST API:** Full API for integration with Serenity and other apps
- 🚀 **Self-Healing:** Advanced error monitoring and automatic recovery
- 🚀 **Mobile App:** Cross-platform mobile access

---

## 🛠️ Technology Stack

- **UI Framework:** Avalonia UI 11.2.1 (Cross-platform)
- **Runtime:** .NET 8 (targeting .NET 10.0)
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Database:** SQLite with Entity Framework Core
- **AI Providers:** Gemini, Grok/xAI, Ollama
- **Voice:** ElevenLabs TTS, Edge TTS
- **Video:** LibVLCSharp, MoviePy (Python)
- **Image Generation:** Stable Diffusion (local), Grok/xAI
- **Media:** NAudio, ffmpeg

---

## 📂 Project Structure

```
D:\_Code_\Cortex\
├── Cortex.App\                  # Avalonia UI Layer
│   ├── Views\
│   ├── ViewModels\
│   ├── Controls\
│   └── Cortex.App.csproj
├── Cortex.Core\                 # Business Logic
│   ├── Services\
│   ├── Models\
│   ├── LLM\
│   ├── Persistence\
│   └── Cortex.Core.csproj
├── Cortex.API\                  # REST API Server (Future)
│   ├── Controllers\
│   └── Cortex.API.csproj
├── Cortex.Data\                 # Data Storage
│   ├── cortex.db
│   ├── projects\
│   └── artifacts\
├── Controls\                    # UI Controls
├── Services\                    # Service Classes
├── Models\                      # Data Models
├── Assets\                      # Images, icons
├── data\                        # Data/Config Storage
├── config\                      # Configuration
├── tools\                       # Scripts
├── Docs\                        # Documentation
│   └── PROJECT_BIBLE.md         # Complete reference
└── README.md                    # This file
```

---

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK**
- **ffmpeg** (Required for voice/audio/video processing)
- **Stable Diffusion WebUI** (Optional - for local image generation)
- **Ollama** (Optional - for local LLM inference)

### Building

```powershell
cd Cortex.App
dotnet build
```

### Running

```powershell
dotnet run --project Cortex.App
```

### Configuration

Create `config/cortex.env`:

```env
GEMINI_API_KEY=your_gemini_key
XAI_API_KEY=your_grok_key
GROK_API_KEY=your_grok_key (alias)
OLLAMA_URL=http://localhost:11434
ELEVENLABS_API_KEY=your_elevenlabs_key (optional)
STABLE_DIFFUSION_URL=http://127.0.0.1:7860
```

---

## 📖 Documentation

### Complete Reference

- **`Docs/PROJECT_BIBLE.md`** - Complete Cortex reference (systems, features, status, roadmap)
- **`Reference/Serenity/PROJECT_BIBLE.md`** - Serenity reference (parent project)
- **`Reference/Cortex/PROJECT_BIBLE.md`** - Cortex Project Bible in Serenity's Reference folder

### Key Documentation

- **Architecture:** See `Docs/PROJECT_BIBLE.md` - Section "Architecture"
- **Features:** See `Docs/PROJECT_BIBLE.md` - Section "Feature Inventory"
- **API:** See `Docs/PROJECT_BIBLE.md` - Section "REST API"
- **Roadmap:** See `Docs/PROJECT_BIBLE.md` - Section "Development Roadmap"

---

## 🔄 Integration with Serenity

**Cortex** is designed to work independently but integrates seamlessly with **Serenity** (Master Desktop).

**Current:**
- Cortex extracted from Serenity
- Standalone application
- Own data storage and configuration

**Future Integration:**
- REST API for Serenity control
- Serenity launches Cortex as managed app
- Shared Crew personas (moved from Serenity)
- Unified configuration management

---

## 🎯 Upcoming Upgrades from Serenity

Cortex will receive major upgrades courtesy of Serenity:

### UI/UX Enhancements

1. **EchoBay Theme Integration**
   - Modern ChatPill interface
   - Sleek Avalonia theme
   - Enhanced visual design

2. **IDE Features**
   - Built-in code editor
   - Terminal integration
   - File browser

### Advanced Features

3. **Code Analysis (Scribe Integration)**
   - Intelligent code understanding
   - Auto-fix capabilities
   - Git integration

4. **Automation (PyGPT/DecisionsAI)**
   - Voice control
   - GUI automation
   - Tool system

5. **Enhanced AI**
   - Circuit breakers
   - Token optimization
   - Multi-AI relay
   - Learning database

### Infrastructure

6. **REST API**
   - Full API for external integration
   - Serenity control interface
   - Webhooks for events

7. **Self-Healing**
   - Advanced error monitoring
   - Automatic recovery
   - Health checks

---

## 📊 Current Status

### ✅ Implemented

- Core research engine (Sources, Stage, Studio)
- RAG system
- AI integration (Gemini, Grok, Ollama)
- Media generation
- Crew personas

### ⏳ In Progress

- Standalone app setup
- Project structure
- Documentation

### 🚀 Planned

- EchoBay UI theme integration
- REST API
- IDE features
- Code analysis integration
- Automation features
- Mobile app

---

## 🤝 Contributing

See `Docs/PROJECT_BIBLE.md` for detailed contribution guidelines.

---

## 📝 License

[License Here]

---

## 🔗 Links

- **GitLab:** https://gitlab.com/Neuro1977/cortex
- **GitHub:** https://github.com/neuro-1977/Cortex (Public)
- **Parent Project:** Serenity (`D:\_Code_\Serenity\`)

---

*"The research engine that never sleeps."*  
**Version:** 0.1.0 (Pre-Release)  
**Last Updated:** January 2026
