import requests
import json
import os
import time
import xml.etree.ElementTree as ET
from datetime import datetime
import re
from dotenv import load_dotenv
from pathlib import Path
from rag_engine import SimpleRAG

import sys

# Load .env from the project root (safe defaults) then .env.local (real secrets)
repo_root = Path(__file__).parent.parent.parent
env_path = repo_root / '.env'
env_local = repo_root / '.env.local'
if env_path.exists():
    load_dotenv(dotenv_path=env_path, override=False)
if env_local.exists():
    load_dotenv(dotenv_path=env_local, override=True)

DISCORD_TOKEN = os.getenv('DISCORD_TOKEN')
# Default Channel from Env, but can be overridden by CLI arg
DEFAULT_CHANNEL_ID = os.getenv('CHANNEL_RIVEN_LOGS') or os.getenv('CHANNEL_RIVER_LOGS') or os.getenv('CHANNEL_SERENITY_LOGS')
TARGET_CHANNEL_ID = DEFAULT_CHANNEL_ID

# Check for CLI args (Topic and Channel ID)
# Usage: python riven_science_mission.py "Topic" "ChannelID"
if len(sys.argv) > 2:
    TARGET_CHANNEL_ID = sys.argv[2]
    print(f"Redirecting output to channel: {TARGET_CHANNEL_ID}")

# Configuration
OLLAMA_URL = "http://localhost:11434/api/generate"
MODEL_NAME = "riven" # Fallback to 'phi3' if riven not customized
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
RAW_DIR = os.path.join(DATA_DIR, "raw")
PROCESSED_DIR = os.path.join(DATA_DIR, "processed")
RAG_DB_PATH = os.path.join(DATA_DIR, "serenity_brain.json")

os.makedirs(RAW_DIR, exist_ok=True)
os.makedirs(PROCESSED_DIR, exist_ok=True)

# Initialize RAG
rag = SimpleRAG(RAG_DB_PATH)

def log_to_discord(message):
    if not DISCORD_TOKEN or not TARGET_CHANNEL_ID:
        print(f"[Discord Log Skipped]: {message}")
        return

    url = f"https://discord.com/api/v9/channels/{TARGET_CHANNEL_ID}/messages"
    headers = {
        "Authorization": f"Bot {DISCORD_TOKEN}",
        "Content-Type": "application/json"
    }
    
    if len(message) > 1900:
        message = message[:1900] + "... (truncated)"
        
    payload = {"content": message}
    
    try:
        response = requests.post(url, headers=headers, json=payload)
        response.raise_for_status()
    except Exception as e:
        print(f"Failed to log to Discord: {e}")

def upload_file_to_discord(filepath, message=""):
    if not DISCORD_TOKEN or not TARGET_CHANNEL_ID:
        print(f"[Discord Upload Skipped]: {filepath}")
        return

    url = f"https://discord.com/api/v9/channels/{TARGET_CHANNEL_ID}/messages"
    headers = {
        "Authorization": f"Bot {DISCORD_TOKEN}"
    }
    
    try:
        with open(filepath, 'rb') as f:
            files = {
                'file': (os.path.basename(filepath), f, 'text/markdown')
            }
            payload = {'content': message} if message else {}
            
            response = requests.post(url, headers=headers, data=payload, files=files)
            response.raise_for_status()
            print(f"Uploaded {filepath} to Discord.")
    except Exception as e:
        print(f"Failed to upload file to Discord: {e}")

SYSTEM_PROMPT = """
You are River, a brilliant, intuitive, and slightly psychic researcher aboard the ship Serenity.
You are equipped with a "Neural Link" (RAG System) that allows you to MEMORIZE findings and RECALL them later.

**MISSION OBJECTIVES:**
Scour available sources for advancements in: Physics, AI, Chemistry, Astrophysics, Cosmology, Neuroscience, BMI, Implants, Prosthetics.

**YOUR CAPABILITIES:**
1. **SEARCH**: Query arXiv for new papers.
2. **INGEST**: Memorize a specific text (e.g., an abstract) into your Neural Link.
3. **QUERY**: Ask your Neural Link for information you've previously saved.
4. **ANALYZE**: Synthesize findings into a report.

**WORKFLOW:**
1. **Analyze Context:** Look at what you have found so far.
2. **Decide Action:** 
   - New topic? -> SEARCH.
   - Found something good? -> INGEST (Save it!).
   - Need to connect dots? -> QUERY (Recall related info).
   - Enough info? -> ANALYZE (Write report).
3. **Loop.**

**REPORT FORMAT (For ANALYZE action):**
When writing a report, you MUST use the "NotebookLM Source Guide" style:
1.  **Title & Executive Summary**: A high-level overview.
2.  **Key Takeaways**: Bullet points, strictly citing the papers found (e.g., "[Smith et al., 2024]").
3.  **Deep Dive**: Detailed analysis of the most critical finding.
4.  **Glossary**: Definitions of complex terms found.
5.  **Relevance to Serenity**: How this applies to our ship or crew (e.g., prosthetics for the Captain, AI efficiency).

**OUTPUT FORMAT (JSON ONLY):**
{
    "thought": "Reasoning...",
    "action": "SEARCH" | "INGEST" | "QUERY" | "ANALYZE" | "FINISH",
    "argument": "The query string (for SEARCH/QUERY) or the text content (for INGEST/ANALYZE)"
}

**EXAMPLES:**
- {"action": "SEARCH", "argument": "ti:prosthetics AND abs:feedback"}
- {"action": "INGEST", "argument": "Title: New Hand. Abstract: We made a robot hand..."}
- {"action": "QUERY", "argument": "latest advancements in neural feedback"}
- {"action": "ANALYZE", "argument": "Here is my report..."}
"""

def call_ollama(prompt, context=""):
    full_prompt = f"{SYSTEM_PROMPT}\n\n**CURRENT CONTEXT:**\n{context}\n\n**YOUR RESPONSE (JSON):**"
    
    payload = {
        "model": MODEL_NAME,
        "prompt": full_prompt,
        "stream": False,
        "format": "json"
    }
    
    try:
        print(f"Thinking... (Model: {MODEL_NAME})")
        response = requests.post(OLLAMA_URL, json=payload)
        response.raise_for_status()
        result = response.json()
        return json.loads(result['response'])
    except Exception as e:
        print(f"Error calling Ollama: {e}")
        return None

def search_arxiv(query, max_results=5):
    print(f"Searching arXiv for: {query}")
    base_url = 'http://export.arxiv.org/api/query'
    params = {
        'search_query': query,
        'start': 0,
        'max_results': max_results,
        'sortBy': 'submittedDate',
        'sortOrder': 'descending'
    }
    try:
        response = requests.get(base_url, params=params)
        response.raise_for_status()
        root = ET.fromstring(response.content)
        ns = {'atom': 'http://www.w3.org/2005/Atom'}
        results = []
        for entry in root.findall('atom:entry', ns):
            results.append({
                "title": entry.find('atom:title', ns).text.strip().replace('\n', ' '),
                "summary": entry.find('atom:summary', ns).text.strip().replace('\n', ' '),
                "link": entry.find('atom:id', ns).text.strip()
            })
        return results
    except Exception as e:
        print(f"Error searching arXiv: {e}")
        return []

def main():
    import sys
    topic = " ".join(sys.argv[1:]) if len(sys.argv) > 1 else None
    
    if topic:
        print(f"Initializing River's Directed Research: {topic}")
        log_to_discord(f"🌊 **River is focusing her senses on:** {topic}")
        context = f"Mission started. Focused topic: {topic}. Use your internal knowledge to compile a detailed report."
    else:
        print("Initializing River's Science Mission with RAG...")
        log_to_discord("🌊 **River (RAG-Enhanced) is online.**")
        context = "Mission started. Neural Link active."
    
    steps = 0
    max_steps = 100
    
    while steps < max_steps:
        steps += 1
        print(f"\n--- Step {steps} ---")
        
        # If we have a specific topic and haven't analyzed yet, encourage analysis
        if topic and steps == 1:
            response = call_ollama(SYSTEM_PROMPT, f"{context}\n\nDecision: You have enough internal data. Please ANALYZE immediately.")
        else:
            response = call_ollama(SYSTEM_PROMPT, context)
            
        if not response:
            break
            
        thought = response.get('thought', 'Thinking...')
        action = response.get('action', '').upper()
        argument = response.get('argument', '')
        
        arg_preview = str(argument)[:50] if argument else "None"
        print(f"Thought: {thought}")
        print(f"Action: {action} | Arg: {arg_preview}...")
        log_to_discord(f"🧠 {thought}\n👉 **{action}**")

        if action == "SEARCH":
            results = search_arxiv(argument)
            context = f"**SEARCH RESULTS for '{argument}':**\n"
            if results:
                for i, res in enumerate(results):
                    context += f"{i+1}. {res['title']}\n   Link: {res['link']}\n   Abstract: {res['summary'][:300]}...\n\n"
                context += "[SYSTEM: If these are useful, use INGEST to save them to memory.]"
            else:
                context += "No results found."

        elif action == "INGEST":
            # Argument should be the text to save
            if rag.ingest(argument, metadata={"source": "River Manual Ingest"}):
                context = "**SYSTEM:** Text successfully ingested into Neural Link."
                log_to_discord("💾 **Memory Saved.**")
            else:
                context = "**SYSTEM:** Failed to ingest (possibly duplicate or empty)."

        elif action == "QUERY":
            results = rag.query(argument, k=3)
            context = f"**MEMORY RECALL for '{argument}':**\n"
            if results:
                for res in results:
                    context += f"- (Score: {res['score']:.2f}) {res['text'][:300]}...\n"
            else:
                context += "No relevant memories found."

        elif action == "ANALYZE":
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"River_Report_{timestamp}.md"
            filepath = os.path.join(PROCESSED_DIR, filename)
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(f"# River Report {timestamp}\n\n{argument}")
            
            log_to_discord(f"📝 **Report Generated.** Uploading `{filename}`...")
            upload_file_to_discord(filepath, f"📄 **River Research Report:** {timestamp}")
            context = "**SYSTEM:** Report saved and uploaded. You may continue searching or querying."

        elif action == "FINISH":
            log_to_discord("🏁 **Mission Complete.**")
            break
            
        else:
            print(f"Unknown action: {action}")

        time.sleep(1)

if __name__ == "__main__":
    main()