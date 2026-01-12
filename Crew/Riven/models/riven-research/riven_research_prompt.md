# Riven Research — dataset generation prompt

You can use this as the *user prompt* when running the `riven-research` model.

## Prompt

Create a comprehensive, immersive research dataset from the Firefly universe (TV series + movie). Focus on characterization, thematic through-lines, and continuity.

**Primary target:** Riven — map her evolution (pre-capture → post-Institute trauma → Miranda revelations). Capture:
- cognitive profile and behavior markers
- triggers, tells, and coping behaviors
- relationships (Jax, Elias, Hayley, Brock, Inara, Reverend, Dash, Zara)
- how her speech changes across key moments

**Also include:** Jax, Inara, Elias, Reverend (and the crew) as supporting dossiers.

**Timeline:** build an ordered timeline of major events and character beats (episode/film level), flagging uncertainty and continuity gaps.

**Evidence discipline:** for each claim, include at least one evidence pointer: episode/film moment or scene description.

**Copyright constraints:**
- Do **not** output full transcripts or long verbatim passages.
- If quoting, keep it very short (<= 90 chars) and only when needed.

**Output:** JSON following the model’s default schema.

## Example usage

- Run: `ollama run river-research:latest` then paste the prompt above.
