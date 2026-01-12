# Cortex Setup Guide
**Getting Cortex Up and Running**  
**Generated:** January 2026

---

## Prerequisites

- **.NET 8 SDK** (or .NET 10.0 SDK)
- **ffmpeg** (Required for voice/audio/video processing)
- **Stable Diffusion WebUI** (Optional - for local image generation)
- **Ollama** (Optional - for local LLM inference)

---

## Initial Setup

### 1. Clone Repository

```bash
# GitLab
git clone https://gitlab.com/Neuro1977/cortex.git

# GitHub
git clone https://github.com/neuro-1977/Cortex.git
```

### 2. Configure Environment

Copy the example environment file:

```powershell
Copy-Item config\cortex.env.example config\cortex.env
```

Edit `config/cortex.env` and add your API keys:

```env
GEMINI_API_KEY=your_gemini_key
XAI_API_KEY=your_grok_key
OLLAMA_URL=http://localhost:11434
```

### 3. Build

```powershell
dotnet build
```

### 4. Run

```powershell
dotnet run --project Cortex.App
```

---

## Project Structure

See `Docs/PROJECT_BIBLE.md` for complete structure documentation.

---

## Configuration

See `config/cortex.env.example` for all configuration options.

---

## Troubleshooting

### Build Errors

- Ensure .NET 8+ SDK is installed
- Run `dotnet restore` before building
- Check that all NuGet packages are installed

### Runtime Errors

- Check `config/cortex.env` is configured correctly
- Verify API keys are valid
- Check ffmpeg is installed and in PATH

---

## Next Steps

1. Read `Docs/PROJECT_BIBLE.md` for complete documentation
2. Check `README.md` for feature overview
3. Review `Reference/Serenity/PROJECT_BIBLE.md` for parent project context

---

*"Ready to research."*
