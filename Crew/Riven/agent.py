import os
import requests
import json
import asyncio
from pathlib import Path
from .rag_engine import SimpleRAG

# Configuration
OLLAMA_BASE_URL = (os.environ.get("OLLAMA_BASE_URL") or os.environ.get("OLLAMA_HOST") or "http://127.0.0.1:11434").rstrip("/")
OLLAMA_URL = f"{OLLAMA_BASE_URL}/api/chat"
MODEL_NAME = "river:latest"
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
RAG_DB_PATH = os.path.join(DATA_DIR, "serenity_brain.json")

class RiverAgent:
    def __init__(self):
        self.rag = SimpleRAG(RAG_DB_PATH)
        # We rely on the Modelfile for the System Prompt now.
        # But we can add a dynamic context prompt if needed.

    async def ask(self, query: str, user_name: str = "Captain") -> str:
        """
        Asks River a question. She will check her RAG memory first.
        Returns the text response.
        """
        # 1. RAG Retrieval (Run in thread to avoid blocking)
        docs = await asyncio.to_thread(self.rag.query, query, k=2)
        context = "\n".join([d['text'] for d in docs]) if docs else "No specific data found."

        # 2. Construct Messages
        # We inject the RAG context as a system or user message
        messages = [
            {
                "role": "system", 
                "content": f"CONTEXT DATA FROM SHIP'S DATABASE:\n{context}\n\nUse this data if relevant. Keep response short."
            },
            {
                "role": "user", 
                "content": query
            }
        ]
        
        payload = {"model": MODEL_NAME, "messages": messages, "stream": False}
        
        # 3. Call Ollama (Run in thread)
        def _call_ollama():
            try:
                resp = requests.post(OLLAMA_URL, json=payload, timeout=30)
                resp.raise_for_status()
                return resp.json()['message']['content']
            except Exception as e:
                return f"[Thinking error: {e}]"

        response = await asyncio.to_thread(_call_ollama)
        
        # 4. Clean up response for speech/text
        clean_text = response.replace("**", "").replace("*", "").replace("#", "").replace("_", "")
        return clean_text
