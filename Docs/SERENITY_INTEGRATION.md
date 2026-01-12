# Serenity Integration Guide
**How Cortex Integrates with Serenity Master Desktop**  
**Generated:** January 2026

---

## Overview

**Cortex** is designed to work as a standalone application but integrates seamlessly with **Serenity** (Master Desktop). This document describes the integration architecture and communication model.

---

## Integration Architecture

### Current State

**Cortex:**
- Standalone application
- Own data storage (`Cortex.Data/`)
- Own configuration (`config/cortex.env`)
- Independent operation

**Serenity:**
- Master Desktop / Control Panel
- Controls Cortex via planned REST API
- Launches Cortex as managed app
- Monitors Cortex status

### Future State

**Communication Model:**
```
Serenity (Master) ←→ Cortex (Standalone) via REST API
```

**REST API:**
- Cortex exposes REST API (localhost:5000)
- Serenity communicates via HTTP
- Event-based architecture
- Webhooks for events

---

## REST API (Planned)

### Endpoints

**Health Check:**
- `GET /api/health` - Health status

**Projects:**
- `POST /api/projects` - Create project
- `GET /api/projects/{id}` - Get project
- `DELETE /api/projects/{id}` - Delete project
- `GET /api/projects` - List all projects

**Sources:**
- `POST /api/projects/{id}/sources` - Ingest document
- `GET /api/projects/{id}/sources` - List sources
- `DELETE /api/projects/{id}/sources/{sourceId}` - Delete source

**Chat:**
- `POST /api/projects/{id}/chat` - Chat with sources
- `GET /api/projects/{id}/chat/history` - Get chat history

**Artifacts:**
- `POST /api/projects/{id}/artifacts` - Generate artifact
- `GET /api/projects/{id}/artifacts` - List artifacts
- `GET /api/projects/{id}/artifacts/{artifactId}` - Get artifact

**Events:**
- Webhooks for artifact generation completion
- Webhooks for source ingestion completion

---

## Crew Personas

**Crew Personas** will be moved from Serenity to Cortex:

**Location:** `D:\_Code_\Cortex/Crew/` (future)

**Migration:**
- All Crew personas moved from Serenity
- Cortex has own persona management
- Serenity references Cortex personas via API

**Benefits:**
- Cortex becomes persona research hub
- Serenity orchestrates personas across apps
- Unified persona management

---

## Data Sharing

### Shared Data (Future)

**Crew Personas:**
- Cortex: Primary storage (`Crew/`)
- Serenity: API access to Cortex personas

**Projects:**
- Cortex: Own projects (`Cortex.Data/projects/`)
- Serenity: API access to Cortex projects

**Artifacts:**
- Cortex: Own artifacts (`Cortex.Data/artifacts/`)
- Serenity: API access to Cortex artifacts

---

## Serenity Upgrades to Cortex

Cortex will receive major upgrades from Serenity:

1. **EchoBay UI Theme** - Modern ChatPill interface
2. **IDE Features** - Code editor, terminal, file browser
3. **Code Analysis** - Scribe integration
4. **Automation** - Voice control, GUI automation
5. **Enhanced AI** - Circuit breakers, token optimization
6. **REST API** - Full API integration
7. **Self-Healing** - Advanced error monitoring

See `Docs/UPGRADE_ROADMAP.md` for detailed upgrade plan.

---

## Communication Flow

### Launch Flow

1. Serenity launches Cortex as separate process
2. Cortex starts REST API server (localhost:5000)
3. Serenity connects to Cortex API
4. Cortex reports status to Serenity

### Operation Flow

1. Serenity sends commands to Cortex via API
2. Cortex executes operations (chat, artifact generation, etc.)
3. Cortex reports progress/status to Serenity
4. Serenity displays status in dashboard

### Event Flow

1. Cortex generates artifact
2. Cortex sends webhook to Serenity
3. Serenity updates dashboard
4. Serenity notifies user (if configured)

---

## Benefits of Integration

### For Cortex

- **Managed by Serenity:** Serenity handles launch/stop/monitoring
- **Shared Resources:** Access to Serenity's enhanced features
- **Unified Experience:** Part of larger Serenity ecosystem

### For Serenity

- **Research Capabilities:** Access to Cortex research engine
- **Modular Architecture:** Cortex as independent component
- **Extensibility:** Easy to add more managed apps

---

## Future Enhancements

### Planned

- Real-time collaboration between Serenity and Cortex
- Shared context across apps
- Unified authentication
- Cross-app workflows

---

*"Cortex: Powered by Serenity. Enhanced for excellence."*  
**Last Updated:** January 2026
