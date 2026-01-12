import json
import os
import math
import requests
from typing import List, Dict, Optional

# Configuration
import os

OLLAMA_BASE_URL = (os.environ.get("OLLAMA_BASE_URL") or os.environ.get("OLLAMA_HOST") or "http://127.0.0.1:11434").rstrip("/")
# Using 'phi' as the default since it's already installed and stable
EMBEDDING_MODEL = "phi" 

class SimpleRAG:
    def __init__(self, storage_path: str):
        self.storage_path = storage_path
        self.documents = [] # List of {"id": str, "text": str, "metadata": dict, "embedding": List[float]}
        self.load()

    def load(self):
        if os.path.exists(self.storage_path):
            with open(self.storage_path, 'r', encoding='utf-8') as f:
                self.documents = json.load(f)
            print(f"[RAG] Loaded {len(self.documents)} documents from {self.storage_path}")
        else:
            print(f"[RAG] No existing database found at {self.storage_path}. Starting fresh.")

    def save(self):
        with open(self.storage_path, 'w', encoding='utf-8') as f:
            json.dump(self.documents, f, indent=2)
        print(f"[RAG] Saved {len(self.documents)} documents to {self.storage_path}")

    def get_embedding(self, text: str) -> List[float]:
        """Fetches embedding from Ollama using the standard /api/embed endpoint."""
        url = f"{OLLAMA_BASE_URL}/api/embed"
        FALLBACK_MODEL = "nomic-embed-text"
        
        # 1. Try Primary Model
        payload = {"model": EMBEDDING_MODEL, "input": text}
        try:
            response = requests.post(url, json=payload)
            if response.status_code == 200:
                return response.json()["embeddings"][0]
        except Exception:
            pass

        # 2. Try Fallback Model
        print(f"[RAG] Warning: '{EMBEDDING_MODEL}' failed or missing. Trying fallback '{FALLBACK_MODEL}'...")
        payload["model"] = FALLBACK_MODEL
        try:
            response = requests.post(url, json=payload)
            if response.status_code == 200:
                return response.json()["embeddings"][0]
        except Exception:
            pass

        # 3. Legacy endpoint /api/embeddings fallback
        legacy_url = f"{OLLAMA_BASE_URL}/api/embeddings"
        payload_legacy = {"model": EMBEDDING_MODEL, "prompt": text}
        try:
            response = requests.post(legacy_url, json=payload_legacy)
            if response.status_code == 200:
                return response.json()["embedding"]
        except Exception as e:
            print(f"[RAG] Error: All embedding attempts failed. {e}")
            print(f"[RAG] CRITICAL: Run 'ollama pull {EMBEDDING_MODEL}' to fix this.")
            
        return []

    def ingest(self, text: str, metadata: Dict = None) -> bool:
        """Adds a document to the knowledge base."""
        if metadata is None:
            metadata = {}
        
        # Check for duplicates
        for doc in self.documents:
            if doc['text'] == text:
                return False

        embedding = self.get_embedding(text)
        if not embedding:
            return False

        doc = {
            "id": f"doc_{len(self.documents) + 1}",
            "text": text,
            "metadata": metadata,
            "embedding": embedding
        }
        self.documents.append(doc)
        self.save()
        return True

    def cosine_similarity(self, vec1: List[float], vec2: List[float]) -> float:
        if not vec1 or not vec2:
            return 0.0
        dot_product = sum(a * b for a, b in zip(vec1, vec2))
        magnitude1 = math.sqrt(sum(a * a for a in vec1))
        magnitude2 = math.sqrt(sum(b * b for b in vec2))
        if magnitude1 == 0 or magnitude2 == 0:
            return 0.0
        return dot_product / (magnitude1 * magnitude2)

    def query(self, query_text: str, k: int = 3) -> List[Dict]:
        print(f"[RAG] Querying for: '{query_text}'")
        query_embedding = self.get_embedding(query_text)
        if not query_embedding:
            return []

        scored_docs = []
        for doc in self.documents:
            score = self.cosine_similarity(query_embedding, doc['embedding'])
            scored_docs.append((score, doc))

        scored_docs.sort(key=lambda x: x[0], reverse=True)
        
        results = []
        for score, doc in scored_docs[:k]:
            result_doc = doc.copy()
            del result_doc['embedding']
            result_doc['score'] = score
            results.append(result_doc)
        return results