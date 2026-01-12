# river-research (Ollama model)

This model is intentionally separate from `river:latest` (the chatbot).

## Create / update the model locally

Use the Modelfile in this folder.

- Create new model:
  - `ollama create river-research -f Crew/River/models/river-research/Modelfile`

If you want to *repurpose* an existing model (e.g. a spare persona), just use that name instead of `river-research`.

## Run

- `ollama run river-research:latest`
- Paste `river_research_prompt.md` (or your own research task)

If you want runs automatically saved (and optionally posted to Discord), use:

- `python Crew/River/run_persona_research.py`

## Notes

- The system prompt forbids full transcripts/long verbatim output. Keep it analysis-first.
