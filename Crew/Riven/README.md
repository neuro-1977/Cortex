# Riven Tam (Prodigy)

Riven is our deep-dive analyst. Her job is to:

- investigate weird behavior (imports, serialization, edge cases)
- analyze the Brain (`serenity.db`) for facts before guessing
- produce *actionable* findings: root cause, reproduction steps, and concrete fixes

## Prompt templates

- `river_continuous_deepdive_prompt.md` — reusable template for running a structured deep-dive.

## Models

- `river:latest` — **chatbot** (do not overwrite).
- `river-research:latest` — separate **research/dataset** persona (see `models/river-research/`).

To create/update the research model locally, see:

- `models/river-research/README.md`

## Related historical reports

These are helpful context, but they’re not “operational prompts”:

- `Crew/River/docs/reports/River's Deep Dive Report.md`
- `Crew/River/docs/reports/River's Toolbox Analysis.md`
- `Crew/River/docs/reports/River's Serialization Analysis.md`
- `Crew/River/docs/reports/System Audit Findings - River Setup.md`

(There are also `_`-sanitized duplicates like `River_s ...` now stored alongside the reports.)

## Docs organization

If you want to reorganize `Serenity/Docs/` into Crew subfolders, use:

- `tools/organize_docs_into_crew.py --dry-run`
- `tools/organize_docs_into_crew.py --apply`

## Persona research runs

To run the separate research model (`river-research:latest`) and save outputs under `Crew/River/research/runs/`:

- `python Crew/River/run_persona_research.py`

Optional environment variables (loaded from repo-root `.env` if present):

- `DISCORD_TOKEN` — Discord bot token
- `CHANNEL_RIVER_LOGS` (preferred) or `CHANNEL_SERENITY_LOGS` — channel id to post progress updates
- `RIVER_RESEARCH_MODEL` — override the default model name (defaults to `river-research:latest`)
- `OLLAMA_URL` — override Ollama endpoint (defaults to `http://localhost:11434/api/generate`)
