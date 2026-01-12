from __future__ import annotations

import argparse
import json
import os
import re
import time
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Iterable, Optional

import requests

try:
    from dotenv import load_dotenv
except Exception:  # pragma: no cover
    load_dotenv = None  # type: ignore


def _repo_root() -> Path:
    # .../Serenity/Crew/River/<this_file>
    return Path(__file__).resolve().parents[2]


def _load_env() -> None:
    if load_dotenv is None:
        return
    repo = _repo_root()
    # Load safe defaults first, then allow local secrets to override.
    env_path = repo / ".env"
    env_local = repo / ".env.local"
    if env_path.exists():
        load_dotenv(dotenv_path=env_path, override=False)
    if env_local.exists():
        load_dotenv(dotenv_path=env_local, override=True)


def _slugify(s: str, max_len: int = 60) -> str:
    s = s.strip().lower()
    s = re.sub(r"[^a-z0-9]+", "-", s)
    s = re.sub(r"-+", "-", s).strip("-")
    return s[:max_len] if s else "run"


def _now_stamp() -> str:
    return datetime.now().strftime("%Y-%m-%d_%H-%M-%S")


def _read_text_safely(path: Path, max_chars: int = 30_000) -> str:
    try:
        text = path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        text = path.read_text(encoding="utf-8", errors="replace")

    if len(text) <= max_chars:
        return text

    head = text[: max_chars // 2]
    tail = text[-max_chars // 2 :]
    return (
        head
        + "\n\n… [TRUNCATED] …\n\n"
        + tail
        + f"\n\n[NOTE] Source truncated from {len(text)} chars."
    )


def log_to_discord(message: str, *, token: Optional[str], channel_id: Optional[str]) -> None:
    if not token or not channel_id or "REPLACE_ME" in token or "REPLACE_ME" in str(channel_id):
        print(f"[Discord Log Skipped] {message}")
        return

    url = f"https://discord.com/api/v9/channels/{channel_id}/messages"
    headers = {
        "Authorization": f"Bot {token}",
        "Content-Type": "application/json",
    }

    if len(message) > 1900:
        message = message[:1900] + "... (truncated)"

    try:
        resp = requests.post(url, headers=headers, json={"content": message}, timeout=20)
        resp.raise_for_status()
    except Exception as e:
        print(f"Failed to log to Discord: {e}")


@dataclass
class OllamaResult:
    raw_text: str
    json_obj: Optional[dict]


def _ollama_generate(
    *,
    url: str,
    model: str,
    prompt: str,
    stream: bool,
    discord_token: Optional[str],
    discord_channel: Optional[str],
    discord_interval_s: float,
) -> OllamaResult:
    payload = {
        "model": model,
        "prompt": prompt,
        "stream": stream,
        "format": "json",
    }

    assembled = ""
    last_discord = 0.0

    try:
        if not stream:
            resp = requests.post(url, json=payload, timeout=600)
            resp.raise_for_status()
            data = resp.json()
            assembled = data.get("response", "")
        else:
            with requests.post(url, json=payload, stream=True, timeout=600) as resp:
                resp.raise_for_status()
                for line in resp.iter_lines(decode_unicode=True):
                    if not line:
                        continue
                    try:
                        chunk = json.loads(line)
                    except json.JSONDecodeError:
                        continue

                    assembled += chunk.get("response", "")

                    now = time.time()
                    if (
                        discord_interval_s > 0
                        and now - last_discord >= discord_interval_s
                        and len(assembled) > 250
                    ):
                        last_discord = now
                        preview = assembled[-1200:]
                        log_to_discord(
                            "Riven Research (in progress)…\n" + preview,
                            token=discord_token,
                            channel_id=discord_channel,
                        )

                    if chunk.get("done") is True:
                        break

        parsed: Optional[dict]
        try:
            parsed = json.loads(assembled)
        except json.JSONDecodeError:
            parsed = None

        return OllamaResult(raw_text=assembled, json_obj=parsed)

    except Exception as e:
        msg = f"Riven Research failed calling Ollama: {e}"
        log_to_discord(msg, token=discord_token, channel_id=discord_channel)
        raise


def _extract_prompt_body(markdown_text: str) -> str:
    """Best-effort extraction of the '## Prompt' section, falling back to full text."""
    m = re.search(r"^##\s+Prompt\s*$", markdown_text, flags=re.MULTILINE)
    if not m:
        return markdown_text.strip()

    start = m.end()
    tail = markdown_text[start:]
    # Stop at next top-level section.
    m2 = re.search(r"^##\s+", tail, flags=re.MULTILINE)
    if m2:
        tail = tail[: m2.start()]
    return tail.strip()


def build_prompt(
    *,
    topic: str,
    prompt_file: Path,
    sources: Iterable[Path],
) -> str:
    prompt_template = ""
    if prompt_file.exists():
        prompt_template = _extract_prompt_body(_read_text_safely(prompt_file, max_chars=20_000))

    sources_blob = ""
    sources = list(sources)
    if sources:
        chunks: list[str] = []
        for p in sources:
            if not p.exists() or not p.is_file():
                chunks.append(f"[Missing source] {p}")
                continue
            chunks.append(f"\n---\nSOURCE: {p.as_posix()}\n---\n{_read_text_safely(p)}")
        sources_blob = "\n\n# SOURCES (notes; avoid long quotes)\n" + "\n".join(chunks)

    # Keep this as a 'user prompt'. The model already has a SYSTEM prompt in its Modelfile.
    return (
        "Create a Riven Research dossier run.\n\n"
        f"RUN TOPIC: {topic}\n"
        "- If anything is uncertain, say so explicitly.\n"
        "- Do not output long verbatim passages.\n"
        "- Output JSON following the model schema.\n\n"
        + (prompt_template + "\n\n" if prompt_template else "")
        + sources_blob
    ).strip()


def main() -> int:
    _load_env()

    parser = argparse.ArgumentParser(description="Run Riven persona research and save outputs + Discord logs")
    parser.add_argument(
        "--topic",
        default="Riven persona dossier (Firefly + Serenity film)",
        help="What to research (used as run topic)",
    )
    parser.add_argument(
        "--model",
        default=os.environ.get("RIVEN_RESEARCH_MODEL", "riven-research:latest"),
        help="Ollama model name (default: riven-research:latest)",
    )
    parser.add_argument(
        "--ollama-url",
        default=os.environ.get("OLLAMA_URL", "http://localhost:11434/api/generate"),
        help="Ollama generate endpoint",
    )
    parser.add_argument(
        "--sources",
        nargs="*",
        default=[],
        help="Optional source files to include as notes (paths or globs)",
    )
    parser.add_argument(
        "--stream",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Stream from Ollama (enables progress logging)",
    )
    parser.add_argument(
        "--discord",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Send progress to Discord if token/channel are configured",
    )
    parser.add_argument(
        "--discord-interval",
        type=float,
        default=45.0,
        help="Minimum seconds between Discord progress messages (streaming only)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Create the run folder + prompt files, but do not call Ollama",
    )

    args = parser.parse_args()

    repo_root = _repo_root()
    run_root = repo_root / "Crew" / "Riven" / "research" / "runs"

    prompt_file = (
        repo_root / "Crew" / "Riven" / "models" / "riven-research" / "riven_research_prompt.md"
    )

    # Expand sources (allow simple globs). Interpret relative paths/globs from repo root
    # so the script works even if invoked from another working directory.
    expanded_sources: list[Path] = []
    for s in args.sources:
        if any(ch in s for ch in ("*", "?", "[")):
            expanded_sources.extend(repo_root.glob(s))
        else:
            p = Path(s)
            if not p.is_absolute():
                p = repo_root / p
            expanded_sources.append(p)

    run_id = f"{_now_stamp()}_{_slugify(args.topic)}"
    out_dir = run_root / run_id
    out_dir.mkdir(parents=True, exist_ok=True)

    discord_token = os.getenv("DISCORD_TOKEN")
    discord_channel = os.getenv("CHANNEL_RIVEN_LOGS") or os.getenv("CHANNEL_SERENITY_LOGS")

    discord_enabled = bool(args.discord and discord_token and discord_channel)

    meta = {
        "run_id": run_id,
        "created_at": datetime.now().isoformat(timespec="seconds"),
        "topic": args.topic,
        "model": args.model,
        "ollama_url": args.ollama_url,
        "stream": bool(args.stream),
        "sources": [str(p) for p in expanded_sources],
        "prompt_file": str(prompt_file),
        "discord_enabled": discord_enabled,
        "discord_channel_env": "CHANNEL_RIVEN_LOGS" if os.getenv("CHANNEL_RIVEN_LOGS") else "CHANNEL_SERENITY_LOGS",
    }
    (out_dir / "meta.json").write_text(json.dumps(meta, indent=2, ensure_ascii=False), encoding="utf-8")

    prompt = build_prompt(topic=args.topic, prompt_file=prompt_file, sources=expanded_sources)
    (out_dir / "prompt.txt").write_text(prompt, encoding="utf-8")

    start_msg = (
        "Riven Research starting…\n"
        f"Topic: {args.topic}\n"
        f"Model: {args.model}\n"
        f"Run folder: {out_dir.as_posix()}"
    )
    log_to_discord(start_msg, token=(discord_token if discord_enabled else None), channel_id=(discord_channel if discord_enabled else None))

    if args.dry_run:
        print(f"[DRY RUN] Prepared run folder: {out_dir}")
        return 0

    result = _ollama_generate(
        url=args.ollama_url,
        model=args.model,
        prompt=prompt,
        stream=bool(args.stream),
        discord_token=(discord_token if discord_enabled else None),
        discord_channel=(discord_channel if discord_enabled else None),
        discord_interval_s=float(args.discord_interval),
    )

    (out_dir / "ollama_response.txt").write_text(result.raw_text, encoding="utf-8")

    if result.json_obj is not None:
        (out_dir / "dossier.json").write_text(
            json.dumps(result.json_obj, indent=2, ensure_ascii=False), encoding="utf-8"
        )

    # Produce a lightweight markdown view for humans and Discord.
    summary_lines: list[str] = []
    summary_lines.append(f"# Riven Research Run: {args.topic}")
    summary_lines.append("")
    summary_lines.append(f"- Run ID: `{run_id}`")
    summary_lines.append(f"- Created: `{meta['created_at']}`")
    summary_lines.append(f"- Model: `{args.model}`")
    summary_lines.append("")

    if isinstance(result.json_obj, dict):
        scope = result.json_obj.get("scope")
        open_q = result.json_obj.get("open_questions")
        persona_seeds = result.json_obj.get("persona_seeds")

        if scope is not None:
            summary_lines.append("## Scope")
            summary_lines.append("")
            summary_lines.append("```json")
            summary_lines.append(json.dumps(scope, indent=2, ensure_ascii=False)[:8000])
            summary_lines.append("```")
            summary_lines.append("")

        if persona_seeds is not None:
            summary_lines.append("## Persona seeds")
            summary_lines.append("")
            summary_lines.append("```json")
            summary_lines.append(json.dumps(persona_seeds, indent=2, ensure_ascii=False)[:8000])
            summary_lines.append("```")
            summary_lines.append("")

        if open_q is not None:
            summary_lines.append("## Open questions")
            summary_lines.append("")
            summary_lines.append("```json")
            summary_lines.append(json.dumps(open_q, indent=2, ensure_ascii=False)[:8000])
            summary_lines.append("```")
            summary_lines.append("")

    (out_dir / "run.md").write_text("\n".join(summary_lines), encoding="utf-8")

    done_msg = f"Riven Research complete. Run folder: {out_dir.as_posix()}"
    log_to_discord(done_msg, token=(discord_token if discord_enabled else None), channel_id=(discord_channel if discord_enabled else None))

    print(done_msg)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
