# Cortex Project Bible
**The Complete Guide to Everything Cortex**  
**Generated:** January 2026  
**Status:** Master Documentation - Comprehensive Reference

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Architecture](#architecture)
4. [Systems & Components](#systems--components)
5. [Feature Inventory](#feature-inventory)
6. [Current State](#current-state)
7. [Future Upgrades from Serenity](#future-upgrades-from-serenity)
8. [Integration with Serenity](#integration-with-serenity)
9. [Crew Personas](#crew-personas)
10. [Development Roadmap](#development-roadmap)
11. [Reference Files](#reference-files)

---

## Executive Summary

**Cortex** is a NotebookLM-inspired research engine designed to be a standalone application. Currently being extracted from Serenity, Cortex will operate independently with its own UI, REST API, and data storage.

**Mission:** Build a modular research application that mirrors NotebookLM's capabilities (Sources, Studio, Main Stage) while integrating advanced AI (Grok/Gemini) and media synthesis.

**Current Status:** ⏳ Extraction in progress  
**Future Status:** Standalone app with REST API, upgraded by Serenity  
**Upgrades Coming:** EchoBay UI theme, IDE features, Code analysis, Automation, REST API, Self-healing

---

## Project Overview

### Mission Statement

Cortex is a research engine that enables users to:
- **Ingest:** Upload PDFs, web pages, text files, and images
- **Research:** Chat with sources using RAG (Retrieval-Augmented Generation)
- **Generate:** Create artifacts (Slide Decks, Mind Maps, Audio/Video Overviews, Quizzes, Flashcards)
- **Collaborate:** Use AI personas (Crew) for research assistance

### Technology Stack

- **UI Framework:** Avalonia UI 11.2.1 (Cross-platform)
- **Runtime:** .NET 8 (targeting .NET 10.0)
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Database:** SQLite with Entity Framework Core
- **AI Providers:** Gemini, Grok/xAI, Ollama
- **Voice:** ElevenLabs TTS, Edge TTS
- **Video:** LibVLCSharp, MoviePy (Python)
- **Image Generation:** Stable Diffusion (local), Grok/xAI
- **Media:** NAudio, ffmpeg

### Key Features

- **Sources Panel:** Document ingestion and management
- **Stage (Chat):** Interactive research with citations
- **Studio:** Artifact generation (Slide Decks, Mind Maps, Audio/Video Overviews, Quizzes, Flashcards)
- **RAG System:** Retrieval-Augmented Generation for context-aware responses
- **Crew Personas:** AI personas with distinct voices and roles
- **Media Generation:** Video/audio overviews, slide decks, visuals

---

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    CORTEX (Standalone App)                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Sources Panel (Left)                                 │  │
│  │  - PDF/Web/Text ingestion                             │  │
│  │  - Document management                                 │  │
│  │  - Source metadata                                     │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Stage (Center) - Chat                                │  │
│  │  - Interactive research chat                           │  │
│  │  - Citations display                                   │  │
│  │  - Mind map visualization                              │  │
│  │  - Media preview                                       │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Studio Panel (Right) - Artifact Generation          │  │
│  │  - Audio Overview                                     │  │
│  │  - Video Overview                                     │  │
│  │  - Slide Deck                                         │  │
│  │  - Mind Map                                           │  │
│  │  - Quiz                                               │  │
│  │  - Flashcards                                         │  │
│  │  - Visuals                                            │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Crew Panel (Bottom/Right)                            │  │
│  │  - AI persona selection                               │  │
│  │  - Voice configuration                                │  │
│  │  - Persona bios                                       │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────────┬─────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │   REST API      │
                    │  (Future)       │
                    └────────┬────────┘
                             │
                    ┌────────┴────────┐
                    │   Serenity      │
                    │   (Master)      │
                    └─────────────────┘
```

### Project Structure

```
D:\_Code_\Cortex\
├── Cortex.App\                    # Avalonia UI
│   ├── Views\
│   │   ├── MainWindow.axaml
│   │   ├── SourcesView.axaml
│   │   ├── StageView.axaml
│   │   └── StudioView.axaml
│   ├── ViewModels\
│   │   ├── MainViewModel.cs
│   │   ├── CortexViewModel.cs
│   │   └── ...
│   ├── Controls\
│   │   └── Common\
│   │       └── CrewAvatarView.axaml
│   └── Cortex.App.csproj
├── Cortex.Core\                   # Business Logic
│   ├── Services\
│   │   ├── CortexChatService.cs
│   │   ├── CortexIngestionService.cs
│   │   ├── RetrievalService.cs
│   │   ├── CortexStudioService.cs
│   │   └── ... (all Cortex services)
│   ├── Models\
│   │   ├── CortexProject.cs
│   │   ├── CortexArtifact.cs
│   │   ├── SourceDocument.cs
│   │   └── ...
│   ├── LLM\
│   │   ├── LlmClient.cs
│   │   ├── ILlmClient.cs
│   │   └── ...
│   ├── Persistence\
│   │   └── CortexStorageService.cs
│   └── Cortex.Core.csproj
├── Cortex.API\                    # REST API Server (Future)
│   ├── Controllers\
│   │   ├── ProjectsController.cs
│   │   ├── ChatController.cs
│   │   └── ArtifactsController.cs
│   ├── Models\
│   │   └── ApiModels.cs
│   └── Cortex.API.csproj
├── Controls\                      # UI Controls
├── Services\                      # Service Classes
├── Models\                        # Data Models
├── ViewModels\                    # View Models
├── Views\                         # Views
├── Assets\                        # Images, icons
├── Cortex.Data\                   # Data Storage
│   ├── cortex.db                  # SQLite database
│   ├── projects\                  # Project files
│   └── artifacts\                 # Generated artifacts
├── data\                          # Additional data
│   ├── projects\
│   ├── artifacts\
│   └── logs\
├── config\                        # Configuration
│   └── cortex.env
├── tools\                         # Scripts
├── Docs\                          # Documentation
│   └── PROJECT_BIBLE.md           # This file
├── Crew\                          # Crew Personas (from Serenity)
│   ├── Riven\
│   ├── Dash\
│   ├── Rev\
│   ├── Hayley\
│   └── Serenity\
├── Cortex.slnx                    # Solution file
├── Directory.Build.props          # Build properties
└── README.md                      # Project README
```

---

## Systems & Components

### 1. Sources Panel

**Location:** `Services/Core/CortexIngestionService.cs`, `Models/Core/SourceDocument.cs`

**Purpose:** Document ingestion and management

**Features:**
- **PDF Ingestion:** Extract text from PDF files
- **Web Ingestion:** Extract content from URLs
- **Text Ingestion:** Plain text file support
- **Image Ingestion:** Image upload and analysis (via Gemini Vision)
- **Document Management:** View, delete, search sources
- **Metadata Storage:** Document metadata (title, date, source, etc.)

**Services:**
- `CortexIngestionService.cs` - Document ingestion logic
- `RetrievalService.cs` - RAG retrieval (uses ingested documents)

**Status:** ✅ Fully Implemented (copied from Serenity)

**Files:**
- `Services/Core/CortexIngestionService.cs`
- `Models/Core/SourceDocument.cs`
- `Cortex.Core/Persistence/CortexStorageService.cs`

---

### 2. Stage (Chat Interface)

**Location:** `Services/Core/CortexChatService.cs`, `Models/Core/CortexChatMessage.cs`

**Purpose:** Interactive research chat with citations

**Features:**
- **Chat Interface:** Text-based research chat
- **Citations:** Source citations in chat responses
- **RAG Integration:** Uses RAG system for context-aware responses
- **Mind Map Visualization:** Interactive mind map view (Vis.js)
- **Media Preview:** Preview generated artifacts (audio, video, slides, etc.)
- **Context Awareness:** Maintains conversation context across sessions

**Services:**
- `CortexChatService.cs` - Main chat logic
- `RetrievalService.cs` - RAG retrieval system
- `Cortex.Core/LLM/LlmClient.cs` - AI provider integration

**Status:** ✅ Fully Implemented (copied from Serenity)

**Files:**
- `Services/Core/CortexChatService.cs`
- `Services/Core/RetrievalService.cs`
- `Cortex.Core/LLM/LlmClient.cs`
- `Models/Core/CortexChatMessage.cs`
- `Models/Core/CortexChatResult.cs`
- `Models/Core/CortexCitation.cs`

---

### 3. Studio (Artifact Generation)

**Location:** `Services/Core/CortexStudioService.cs`, `Services/Media/`

**Purpose:** Generate research artifacts from sources

**Features:**
- **Audio Overview:** Generate narrated audio overviews
- **Video Overview:** Generate video overviews with slides and narration
- **Slide Deck:** Generate PowerPoint slide decks
- **Mind Map:** Generate interactive mind maps
- **Quiz:** Generate quizzes from sources
- **Flashcards:** Generate flashcards for study
- **Visuals:** Generate visual cards and graphics

**Services:**
- `CortexStudioService.cs` - Main artifact generation logic
- `CortexVideoService.cs` - Video overview generation
- `CortexVisualsService.cs` - Visual artifact generation
- `CortexImageGenerationService.cs` - Image generation (Stable Diffusion/Grok)
- `ElevenLabsTtsService.cs` - Text-to-speech for audio/video

**Status:** ✅ Fully Implemented (copied from Serenity)

**Files:**
- `Services/Core/CortexStudioService.cs`
- `Services/Core/CortexVideoService.cs`
- `Services/Core/CortexVisualsService.cs`
- `Services/Core/CortexImageGenerationService.cs`
- `Services/Core/ElevenLabsTtsService.cs`
- `Services/Core/CortexLipSyncService.cs`
- `Services/Core/CortexFileCleanupService.cs`
- `Models/Core/CortexArtifact.cs`

---

### 4. RAG System (Retrieval-Augmented Generation)

**Location:** `Services/Core/RetrievalService.cs`, `Cortex.Core/Persistence/CortexStorageService.cs`

**Purpose:** Context-aware responses using source documents

**Features:**
- **Vector Embeddings:** Document embeddings for semantic search
- **Similarity Search:** Find relevant source documents for queries
- **Context Injection:** Inject relevant document chunks into prompts
- **Citation Generation:** Generate citations for used sources
- **Chunking Strategy:** Intelligent document chunking for optimal retrieval

**Services:**
- `RetrievalService.cs` - RAG retrieval logic
- `CortexStorageService.cs` - Embedding storage

**Status:** ✅ Fully Implemented (copied from Serenity)

**Files:**
- `Services/Core/RetrievalService.cs`
- `Cortex.Core/Persistence/CortexStorageService.cs`
- `Models/Core/SourceDocument.cs`

---

### 5. AI Integration

**Location:** `Cortex.Core/LLM/`

**Purpose:** Multi-provider AI support

**Features:**
- **Gemini API:** Primary provider for analysis and research
- **Grok/xAI API:** Creative writing and image generation
- **Ollama:** Local LLM inference for privacy
- **Provider Routing:** Automatic provider selection based on task
- **Vision Support:** Gemini Vision / Grok Vision for image analysis
- **Streaming Responses:** Real-time streaming chat responses

**Services:**
- `LlmClient.cs` - Main LLM client
- `ILlmClient.cs` - LLM interface
- `LlmProvider.cs` - Provider enumeration
- `LlmRequest.cs` - Request model

**Status:** ✅ Fully Implemented (copied from Serenity)

**Future Upgrades:**
- Circuit breakers (from Scribe)
- Token optimization (from Scribe)
- Multi-AI relay (from Scribe)
- Learning database (from Scribe)

**Files:**
- `Cortex.Core/LLM/LlmClient.cs`
- `Cortex.Core/LLM/ILlmClient.cs`
- `Cortex.Core/LLM/LlmProvider.cs`
- `Cortex.Core/LLM/LlmRequest.cs`

---

### 6. Media Generation

**Location:** `Services/Media/`, `Services/Core/CortexVideoService.cs`

**Purpose:** Generate audio/video/media artifacts

**Features:**
- **Audio Overview:** TTS-generated audio with script
- **Video Overview:** Video with slides, images, and narration
- **Slide Deck:** PowerPoint presentations with images
- **Image Generation:** Stable Diffusion (local) or Grok/xAI
- **Video Concatenation:** MoviePy-based video assembly
- **Lip-sync:** Lip-sync integration for video generation

**Services:**
- `CortexVideoService.cs` - Video generation
- `CortexImageGenerationService.cs` - Image generation
- `CortexLipSyncService.cs` - Lip-sync integration
- `ElevenLabsTtsService.cs` - Text-to-speech
- `CortexFileCleanupService.cs` - Temporary file cleanup

**Status:** ✅ Fully Implemented (copied from Serenity)

**Files:**
- `Services/Core/CortexVideoService.cs`
- `Services/Core/CortexImageGenerationService.cs`
- `Services/Core/CortexLipSyncService.cs`
- `Services/Core/ElevenLabsTtsService.cs`
- `Services/Core/CortexFileCleanupService.cs`
- Python scripts in `tools/` (generate_video.py, generate_slides.py, etc.)

---

### 7. Crew Personas

**Location:** `Services/Core/CrewManager.cs`, `Models/Core/EpisodeProfile.cs`, `Crew/` folder (to be moved from Serenity)

**Purpose:** AI personas with distinct voices and roles

**Features:**
- **Persona Management:** Define and manage AI personas
- **Voice Integration:** Assign voices to personas (ElevenLabs/Edge TTS)
- **Bios:** Persona bios and descriptions
- **Avatar Management:** Video avatars for personas
- **Persona Selection:** Choose persona for chat/generation

**Status:** ✅ Implemented (copied from Serenity), ⏳ To be moved from Serenity

**Files:**
- `Services/Core/CrewManager.cs`
- `Models/Core/EpisodeProfile.cs`
- `Controls/Common/CrewAvatarView.axaml`
- `Crew/` folder (to be moved from `D:\_Code_\Serenity\Crew/`)

**Future Location:** `D:\_Code_\Cortex/Crew/` (will receive from Serenity)

---

### 8. Database & Persistence

**Location:** `Services/Core/DatabaseService.cs`, `Cortex.Core/Persistence/CortexStorageService.cs`

**Purpose:** Data storage and management

**Features:**
- **SQLite Database:** Entity Framework Core integration
- **Project Storage:** Store Cortex projects
- **Source Storage:** Store ingested documents
- **Chat History:** Store chat conversations
- **Artifact Metadata:** Store artifact metadata and file paths

**Services:**
- `DatabaseService.cs` - Database management
- `CortexStorageService.cs` - Cortex-specific storage
- `StateService.cs` - Application state persistence (if copied)

**Status:** ✅ Fully Implemented (copied from Serenity)

**Files:**
- `Services/Core/DatabaseService.cs`
- `Cortex.Core/Persistence/CortexStorageService.cs`
- `Models/Core/CortexProject.cs`
- `Models/Core/SourceDocument.cs`
- `Models/Core/CortexChatMessage.cs`
- `Models/Core/CortexArtifact.cs`

**Database Location:** `Cortex.Data/cortex.db` (future)

---

### 9. REST API (Future)

**Location:** `Cortex.API/` (to be created)

**Purpose:** API for external applications (Serenity, etc.)

**Features:**
- **REST Endpoints:** HTTP API for Cortex operations
- **Authentication:** API key or OAuth authentication
- **Webhooks:** Event webhooks for artifact generation
- **Rate Limiting:** API rate limiting
- **Documentation:** OpenAPI/Swagger documentation

**Status:** ⏳ To be implemented (planned for Serenity integration)

**Planned Endpoints:**
- `POST /api/projects` - Create project
- `GET /api/projects/{id}` - Get project
- `POST /api/projects/{id}/sources` - Ingest document
- `POST /api/projects/{id}/chat` - Chat with sources
- `POST /api/projects/{id}/artifacts` - Generate artifact
- `GET /api/projects/{id}/artifacts` - List artifacts
- `GET /api/health` - Health check

---

## Feature Inventory

### ✅ Fully Implemented Features (From Serenity)

1. **Document Ingestion**
   - PDF ingestion ✅
   - Web ingestion ✅
   - Text ingestion ✅
   - Image ingestion ✅
   - Document management ✅

2. **Chat Interface**
   - Interactive chat ✅
   - Citations ✅
   - RAG integration ✅
   - Context awareness ✅
   - Streaming responses ✅

3. **Artifact Generation**
   - Audio overviews ✅
   - Video overviews ✅
   - Slide decks ✅
   - Mind maps ✅
   - Quizzes ✅
   - Flashcards ✅
   - Visuals ✅

4. **AI Integration**
   - Gemini API ✅
   - Grok/xAI API ✅
   - Ollama integration ✅
   - Vision support ✅
   - Provider routing ✅

5. **Media Generation**
   - Image generation ✅
   - Video generation ✅
   - Audio generation ✅
   - Slide generation ✅
   - Lip-sync ✅

6. **Database & Persistence**
   - SQLite integration ✅
   - Project storage ✅
   - Chat history ✅
   - Artifact metadata ✅

7. **Crew Personas**
   - Persona management ✅
   - Voice integration ✅
   - Avatar management ✅

---

### 🚀 Upcoming Upgrades from Serenity

#### 1. EchoBay UI Theme Integration

**Status:** ⏳ Planned

**Features:**
- **Modern ChatPill Interface:** Sleek chat interface from EchoBay
- **Sleek Avalonia Theme:** Enhanced visual design
- **ChatPill Control:** Floating chat interface
- **Status Bar:** Enhanced status bar
- **Settings Dialog:** Improved settings UI

**Source:** `Reference/EchoBay/Controls/ChatPillControl.*`, `Reference/EchoBay/Controls/StatusBarControl.*`

**Timeline:** After basic app is functional

---

#### 2. IDE Features (From EchoBay)

**Status:** ⏳ Planned

**Features:**
- **Code Editor:** Built-in code editor with syntax highlighting
- **Terminal Integration:** Built-in terminal with multiple tabs
- **File Browser:** Git-aware file navigation
- **Build System:** `dotnet build`, `dotnet run`, etc.
- **Project Management:** Open/build/test projects

**Source:** `Reference/EchoBay/Controls/CodeEditorControl.*`, `Reference/EchoBay/Controls/TerminalView.*`, `Reference/EchoBay/Controls/BrowserPane.*`

**Timeline:** After UI theme integration

---

#### 3. Code Analysis Integration (From Scribe)

**Status:** ⏳ Planned

**Features:**
- **Local Static Analysis:** Fast, token-free static analysis
- **AI-Powered Analysis:** AI analysis when needed
- **Auto-Fix Service:** Safe single-line fixes
- **Backup Before Fix:** Creates backup before fix
- **Rollback Capability:** Restore from backup
- **File Monitoring:** Background file monitoring

**Source:** `Reference/Scribe/Services/LocalCodeAnalyzer.cs`, `Reference/Scribe/Services/CodeAnalysisService.cs`, `Reference/Scribe/Services/AutoFixService.cs`

**Timeline:** After IDE features

---

#### 4. Automation Features (From PyGPT/DecisionsAI)

**Status:** ⏳ Planned

**Features:**
- **Voice Control:** Control Cortex with voice commands
- **GUI Automation:** Automate Cortex UI operations
- **Action Service:** GUI actions automation
- **Tool System:** Tool execution system

**Source:** PyGPT/DecisionsAI integration (via Serenity)

**Timeline:** After code analysis integration

---

#### 5. Enhanced AI Features (From Scribe)

**Status:** ⏳ Planned

**Features:**
- **Circuit Breakers:** Prevents retry loops on API failures
- **Token Optimization:** Local-first AI, tokens only when needed
- **Multi-AI Relay:** Fallback mechanisms (Gemini → Grok → Ollama)
- **Learning Database:** Records successful fixes and solutions

**Source:** `Reference/Scribe/Services/CircuitBreakerService.cs`, `Reference/Scribe/Services/TokenOptimizationService.cs`, `Reference/Scribe/Services/MultiAiRelayService.cs`, `Reference/Scribe/Services/LearningDatabaseService.cs`

**Timeline:** After automation features

---

#### 6. REST API

**Status:** ⏳ Planned

**Features:**
- **REST Endpoints:** Full HTTP API for Cortex operations
- **Authentication:** API key or OAuth
- **Webhooks:** Event webhooks
- **Rate Limiting:** API rate limiting
- **OpenAPI Documentation:** Swagger/OpenAPI docs

**Timeline:** After enhanced AI features

---

#### 7. Self-Healing (From Scribe)

**Status:** ⏳ Planned

**Features:**
- **Advanced Error Monitoring:** Comprehensive error tracking
- **Automatic Recovery:** Auto-recovery from errors
- **Health Checks:** API, disk, memory, Git checks
- **Crash Recovery:** State persistence and auto-restart
- **Statistics Dashboard:** Real-time metrics tracking

**Source:** `Reference/Scribe/Services/HealthCheckService.cs`, `Reference/Scribe/Services/CrashRecoveryService.cs`, `Reference/Scribe/Services/StatisticsService.cs`

**Timeline:** After REST API

---

#### 8. Mobile App

**Status:** ⏳ Long-term

**Features:**
- **iOS App:** Native iOS application
- **Android App:** Native Android application
- **Cross-Platform:** Shared codebase
- **Core Features:** Access to Sources, Stage, Studio

**Timeline:** Long-term future

---

## Current State

### Extraction Status

**Location:** `D:\_Code_\Cortex\`

**Files Copied:**
- ✅ Cortex services (12 files)
- ✅ Cortex models (7 files)
- ✅ LLM client files (4 files)
- ✅ Database and persistence files (3 files)
- ✅ Views and ViewModels (4 files)
- ✅ Controls (2 files)
- ✅ Configuration files

**Status:** ⏳ Basic structure created, files copied, project setup pending

**Remaining:**
- ⏳ Create project files (.csproj, .slnx)
- ⏳ Fix namespaces (from Serenity to Cortex)
- ⏳ Update references
- ⏳ Create App.axaml, Program.cs
- ⏳ Set up build system
- ⏳ Test build

---

### Files Structure

**Current Files:**
- `Services/Core/` - 12 Cortex service files
- `Cortex.Core/LLM/` - 4 LLM client files
- `Models/Core/` - 7 model files
- `Cortex.Core/Persistence/` - Storage service
- `Views/` - 2 view files (CortexView.axaml, CortexView.axaml.cs)
- `ViewModels/` - 2 view model files
- `Controls/Common/` - 2 control files

**Status:** ✅ Files copied, ⏳ Needs namespace updates and project file creation

---

## Future Upgrades from Serenity

### Major Upgrades Planned

**Cortex will receive major upgrades courtesy of Serenity:**

1. **EchoBay UI Theme** - Modern ChatPill interface and sleek theme
2. **IDE Features** - Code editor, terminal, file browser
3. **Code Analysis** - Scribe integration for intelligent code understanding
4. **Automation** - Voice control and GUI automation
5. **Enhanced AI** - Circuit breakers, token optimization, multi-AI relay
6. **REST API** - Full API for integration
7. **Self-Healing** - Advanced error monitoring and automatic recovery
8. **Mobile App** - Cross-platform mobile access

### Upgrade Timeline

**Phase 1: Basic Setup** (Current)
- ✅ Extract files from Serenity
- ⏳ Create project structure
- ⏳ Fix namespaces
- ⏳ Test build

**Phase 2: UI Theme** (Next)
- ⏳ Integrate EchoBay ChatPill UI
- ⏳ Apply EchoBay theme
- ⏳ Enhance visual design

**Phase 3: IDE Features** (Future)
- ⏳ Integrate code editor
- ⏳ Integrate terminal
- ⏳ Integrate file browser

**Phase 4: Advanced Features** (Future)
- ⏳ Code analysis integration
- ⏳ Automation features
- ⏳ Enhanced AI features

**Phase 5: API & Infrastructure** (Future)
- ⏳ REST API implementation
- ⏳ Self-healing systems
- ⏳ Health monitoring

**Phase 6: Mobile** (Long-term)
- ⏳ Mobile app development

---

## Integration with Serenity

### Current Relationship

**Parent Project:** Serenity (`D:\_Code_\Serenity\`)

**Cortex Status:**
- Extracted from Serenity
- Standalone application
- Own data storage and configuration

### Future Integration

**Master Desktop Model:**
- Serenity controls Cortex via REST API
- Serenity launches Cortex as managed app
- Cortex reports status to Serenity
- Shared Crew personas (moved from Serenity)

### Communication Architecture

**Planned:** REST API

```
Serenity ←→ Cortex (HTTP REST API on localhost:5000)
```

**Endpoints:**
- `GET /api/health` - Health check
- `POST /api/projects` - Create project
- `GET /api/projects/{id}` - Get project
- `POST /api/projects/{id}/chat` - Chat with sources
- `POST /api/projects/{id}/artifacts` - Generate artifact
- `GET /api/projects/{id}/artifacts` - List artifacts

---

## Crew Personas

### Current Personas (In Serenity)

**Location:** `D:\_Code_\Serenity/Crew/` (to be moved to Cortex)

1. **Riven** (`riven:latest`)
   - Role: Deep research / synthesis
   - Base: DeepSeek R1 (pulled into Ollama as `deepseek-r1:14b`)
   - Source: `Crew/Riven/Modelfile`

2. **Dash** (`dash:latest`)
   - Role: General ops, quick troubleshooting
   - Base: `phi3:latest`
   - Source: `Crew/Dash/Modelfile`

3. **Rev** (`rev:latest`)
   - Role: Creative writing, lore, scripts, dialogue
   - Base: `llama3:latest`
   - Source: `Crew/Rev/Modelfile`

4. **Hayley** (`hayley:latest`)
   - Role: Engineering/coding for interactive tasks
   - Base: `qwen3-coder:30b`
   - Source: `Crew/Hayley/Modelfile`

5. **Hayley (automation)** (`hayley-code:latest`)
   - Role: Long unattended debug/build runs
   - Driver: `hayley_driver.py`
   - Base: `qwen2.5-coder:32b`

6. **Serenity (dispatcher)** (`serenity:latest`)
   - Role: Routes work to the correct crew, outputs paste-ready prompts
   - Base: `phi3:latest`
   - Source: `Crew/Serenity/Modelfile`

### Future in Cortex

- **All personas moved to Cortex**
- **Persona management UI in Cortex**
- **Voice configuration per persona**
- **Avatar management per persona**
- **Bios and descriptions per persona**

**Location:** `D:\_Code_\Cortex/Crew/` (future)

**Status:** ⏳ To be moved from Serenity

---

## Development Roadmap

### Phase 1: Basic Setup (Current)

**Status:** ⏳ In progress

**Tasks:**
- [x] Create folder structure
- [x] Copy files from Serenity
- [ ] Create project files (.csproj, .slnx)
- [ ] Update namespaces (Serenity → Cortex)
- [ ] Fix references
- [ ] Create App.axaml, Program.cs
- [ ] Set up build system
- [ ] Test build
- [ ] Initialize Git repos (GitLab + GitHub)
- [ ] Create CI/CD pipeline
- [ ] Add initial tags

**Timeline:** January 2026

---

### Phase 2: UI Theme Integration (Next)

**Status:** ⏳ Planned

**Tasks:**
- [ ] Integrate EchoBay ChatPill UI
- [ ] Apply EchoBay theme
- [ ] Enhance visual design
- [ ] Update all views with new theme
- [ ] Test UI consistency

**Timeline:** After Phase 1 complete

**Source:** `Reference/EchoBay/Controls/ChatPillControl.*`, `Reference/Serenity/Reference/EchoBay/` (via Serenity)

---

### Phase 3: IDE Features (Future)

**Status:** ⏳ Planned

**Tasks:**
- [ ] Integrate code editor
- [ ] Integrate terminal
- [ ] Integrate file browser
- [ ] Integrate build system
- [ ] Create IDE view

**Timeline:** After Phase 2

**Source:** `Reference/EchoBay/Controls/CodeEditorControl.*`, `Reference/EchoBay/Controls/TerminalView.*`

---

### Phase 4: Advanced Features (Future)

**Status:** ⏳ Planned

**Tasks:**
- [ ] Code analysis integration (Scribe)
- [ ] Automation features (PyGPT/DecisionsAI)
- [ ] Enhanced AI features (Scribe)
- [ ] Statistics dashboard (Scribe)

**Timeline:** After Phase 3

**Source:** `Reference/Scribe/Services/`, `Reference/Serenity/Reference/Scribe/` (via Serenity)

---

### Phase 5: REST API (Future)

**Status:** ⏳ Planned

**Tasks:**
- [ ] Design API endpoints
- [ ] Implement REST API server
- [ ] Add authentication
- [ ] Add rate limiting
- [ ] Create API documentation
- [ ] Create Serenity API client

**Timeline:** After Phase 4

---

### Phase 6: Self-Healing (Future)

**Status:** ⏳ Planned

**Tasks:**
- [ ] Health monitoring (Scribe)
- [ ] Crash recovery (Scribe)
- [ ] Statistics dashboard (Scribe)
- [ ] Error tracking

**Timeline:** After Phase 5

**Source:** `Reference/Scribe/Services/HealthCheckService.cs`, `Reference/Scribe/Services/CrashRecoveryService.cs`

---

## Reference Files

### Serenity Cortex Files

**Location:** `D:\_Code_\Serenity\Services/Core/Services/Cortex*.cs`

**Files:**
- `CortexChatService.cs`
- `CortexIngestionService.cs`
- `CortexStudioService.cs`
- `CortexVideoService.cs`
- `CortexVisualsService.cs`
- `CortexImageGenerationService.cs`
- `CortexLipSyncService.cs`
- `CortexFileCleanupService.cs`
- `RetrievalService.cs`

**Status:** ✅ Copied to Cortex

---

### Serenity Cortex Models

**Location:** `D:\_Code_\Serenity\Services/Core/Models/Cortex*.cs`

**Files:**
- `CortexProject.cs`
- `CortexChatMessage.cs`
- `CortexChatResult.cs`
- `CortexCitation.cs`
- `CortexArtifact.cs`
- `SourceDocument.cs`
- `EpisodeProfile.cs`

**Status:** ✅ Copied to Cortex

---

### Serenity Cortex Views

**Location:** `D:\_Code_\Serenity\Views/CortexView.axaml`

**Files:**
- `CortexView.axaml` / `.axaml.cs`
- `ViewModels/CortexViewModel.cs`
- `ViewModels/CortexViewModel.Artifacts.cs`

**Status:** ✅ Copied to Cortex

---

### EchoBay Reference Files (Upgrades)

**Location:** `Reference/EchoBay/` (in Serenity's Reference folder)

**Files:**
- `Controls/ChatPillControl.*` - ChatPill UI
- `Controls/CodeEditorControl.*` - Code editor
- `Controls/TerminalView.*` - Terminal
- `Controls/BrowserPane.*` - File browser
- `Brain/*` - AI services

**Status:** ⏳ To be integrated into Cortex

---

### Scribe Reference Files (Upgrades)

**Location:** `Reference/Scribe/` (in Serenity's Reference folder)

**Files:**
- `Services/LocalCodeAnalyzer.cs` - Code analysis
- `Services/CircuitBreakerService.cs` - AI optimization
- `Services/HealthCheckService.cs` - Health monitoring
- `Services/StatisticsService.cs` - Statistics

**Status:** ⏳ To be integrated into Cortex

---

## Appendix

### Key Documentation Files

- **This File:** `Docs/PROJECT_BIBLE.md` - Complete Cortex reference
- **Parent Bible:** `D:\_Code_\Serenity\Reference/Cortex/PROJECT_BIBLE.md` - Cortex Project Bible in Serenity's Reference folder
- **Serenity Bible:** `D:\_Code_\Serenity\Reference/Serenity/PROJECT_BIBLE.md` - Serenity Project Bible (parent project)

### External Resources

- **GitLab:** https://gitlab.com/Neuro1977/cortex (to be initialized)
- **GitHub:** https://github.com/neuro-1977/Cortex (Public - to be initialized)
- **Parent Project:** Serenity (`D:\_Code_\Serenity\`)
- **NotebookLM:** Inspiration for UI/UX design

### Configuration

**Environment Variables:** `config/cortex.env`

```env
GEMINI_API_KEY=your_gemini_key
XAI_API_KEY=your_grok_key
GROK_API_KEY=your_grok_key (alias)
OLLAMA_URL=http://localhost:11434
ELEVENLABS_API_KEY=your_elevenlabs_key (optional)
STABLE_DIFFUSION_URL=http://127.0.0.1:7860
CORTEX_MODEL=gemini-2.0-flash-exp
```

---

*"The research engine that never sleeps. Upgraded by Serenity."*  
**Last Updated:** January 2026  
**Version:** 0.1.0 (Pre-Release)  
**Status:** Extraction in progress, upgrades planned
