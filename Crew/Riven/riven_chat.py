import sys
import os
import json
import requests
from dotenv import load_dotenv
from pathlib import Path
from rag_engine import SimpleRAG

# Load .env
repo_root = Path(__file__).parent.parent.parent
env_path = repo_root / '.env'
env_local = repo_root / '.env.local'
if env_path.exists():
    load_dotenv(dotenv_path=env_path, override=False)
if env_local.exists():
    load_dotenv(dotenv_path=env_local, override=True)

# Configuration
OLLAMA_URL = "http://localhost:11434/api/generate"
MODEL_NAME = "riven" # Expects the custom model to be built
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
RAG_DB_PATH = os.path.join(DATA_DIR, "serenity_brain.json")

# Initialize RAG
rag = SimpleRAG(RAG_DB_PATH)

def query_ollama(prompt, context=""):
    system_prompt = """
You are Riven, a researcher with a \"Neural Link\" containing specific scientific knowledge.
You are now in CHAT MODE with the Captain (the User).

**INSTRUCTIONS:**
1. Use the provided **CONTEXT** (retrieved from your Neural Link) to answer the Captain's question.
2. If the context contains the answer, cite the specific paper or source title.
3. If the context is empty or irrelevant, state that you don't have that data in your Neural Link, but offer a general answer based on your base training.
4. Keep answers concise, insightful, and \"shiny\". 
5. Use markdown for formatting.

**TONE:**
Brilliant, slightly psychic, helpful, professional but with a Firefly-universe flair.
"""
    
    full_prompt = f"{system_prompt}\n\n**RETRIEVED CONTEXT:**\n{context}\n\n**CAPTAIN:** {prompt}\n\n**RIVEN:**"
    
    payload = {
        "model": MODEL_NAME,
        "prompt": full_prompt,
        "stream": False
    }
    
    try:
        response = requests.post(OLLAMA_URL, json=payload)
        response.raise_for_status()
        return response.json()['response']
    except Exception as e:
        return f"[Error communicating with River's mind: {e}]"

def chat_loop():
    print("\n" + "="*50)
    print("RIVER - RESEARCH ASSISTANT (CHAT MODE)")
    print(f"   [Neural Link Loaded: {len(rag.documents)} memories]")
    print("   Type 'exit' or 'quit' to leave.")
    print("="*50 + "\n")

    while True:
        try:
            user_input = input("Captain > ")
            if user_input.lower() in ["exit", "quit"]:
                print("\nRiver > Shiny. Signing off.\n")
                break
            
            if not user_input.strip():
                continue

            # 1. Retrieve Context
            print("   (River is searching her Neural Link...)")
            retrieved_docs = rag.query(user_input, k=3)
            
            context_str = ""
            if retrieved_docs:
                for i, doc in enumerate(retrieved_docs):
                    context_str += f"--- Source {i+1} (Score: {doc['score']:.2f}) ---\n"
                    # Include metadata if available (like title)
                    if 'metadata' in doc:
                        context_str += f"Metadata: {json.dumps(doc['metadata'])}\n"
                    context_str += f"Content: {doc['text']}\n\n"
            else:
                context_str = "No specific memories found in the Neural Link matching this query."

            # 2. Generate Response
            print("   (River is thinking...)")
            response = query_ollama(user_input, context_str)
            
            print(f"\nRiver > {response}\n")

        except KeyboardInterrupt:
            print("\n\nRiver > Connection severed. Bye.")
            break

if __name__ == "__main__":
    chat_loop()
