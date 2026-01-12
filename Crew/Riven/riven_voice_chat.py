import os
import sys
import json
import asyncio
import threading
import time
import requests
import pygame
import speech_recognition as sr
import edge_tts
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
MODEL_NAME = "riven"
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
RAG_DB_PATH = os.path.join(DATA_DIR, "serenity_brain.json")
TEMP_AUDIO = os.path.join(DATA_DIR, "temp_speech.mp3")

# Initialize RAG
rag = SimpleRAG(RAG_DB_PATH)

# Initialize Audio
pygame.mixer.init()

# Flags
is_speaking = False
stop_speaking = False

def query_ollama(prompt, context=""):
    system_prompt = """
You are Riven, a brilliant, psychic researcher. 
Your answers will be spoken aloud.

**INSTRUCTIONS:**
1. Keep answers SHORT and CONVERSATIONAL (1-3 sentences max). Long monologues are boring in voice.
2. Use the provided **CONTEXT** from your Neural Link.
3. If interrupted or asked to stop, be polite.
4. Tone: Soft, direct, insightful, slightly abstract.

**CONTEXT:**
"""
    full_prompt = f"{system_prompt}\n{context}\n\n**CAPTAIN:** {prompt}\n\n**RIVER:**"
    
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
        return f"I can't reach my mind... {e}"

async def generate_audio(text):
    voice = "en-US-AriaNeural"
    communicate = edge_tts.Communicate(text, voice)
    await communicate.save(TEMP_AUDIO)

def play_audio():
    global is_speaking, stop_speaking
    try:
        pygame.mixer.music.load(TEMP_AUDIO)
        pygame.mixer.music.play()
        is_speaking = True
        
        while pygame.mixer.music.get_busy():
            if stop_speaking:
                pygame.mixer.music.stop()
                break
            time.sleep(0.1)
            
    except Exception as e:
        print(f"[Audio Error: {e}]")
    finally:
        is_speaking = False
        stop_speaking = False
        # Clean up to allow overwrite
        pygame.mixer.music.unload()

def listen_for_input():
    global stop_speaking
    r = sr.Recognizer()
    r.energy_threshold = 300
    r.dynamic_energy_threshold = True
    r.pause_threshold = 0.8
    
    with sr.Microphone() as source:
        print("\n(Listening...)")
        r.adjust_for_ambient_noise(source, duration=0.5)
        
        try:
            audio = r.listen(source, timeout=None)
            
            # If River is speaking and we hear something, STOP her immediately
            if is_speaking:
                print("![Interruption Detected]!")
                stop_speaking = True
                return "STOP"

            print("(Recognizing...)")
            text = r.recognize_google(audio)
            return text
        except sr.UnknownValueError:
            return None
        except sr.RequestError:
            return None
        except Exception:
            return None

def main():
    print("="*50)
    print("🌊 RIVER - VOICE INTERFACE (INTERRUPTIBLE)")
    print("   [Speak to chat. Interrupt her if she talks too much.]")
    print("="*50)

    while True:
        user_input = listen_for_input()
        
        if not user_input:
            continue
            
        if user_input == "STOP":
            # Just stop audio, loop back to listen
            continue
            
        print(f"Captain > {user_input}")
        
        if user_input.lower() in ["exit", "quit", "leave"]:
            print("River > Signing off.")
            break

        # RAG Retrieval
        docs = rag.query(user_input, k=2)
        context = "\n".join([d['text'] for d in docs]) if docs else "No specific data."

        # Generate Response
        response = query_ollama(user_input, context)
        print(f"River > {response}")
        
        # TTS
        try:
            asyncio.run(generate_audio(response))
            play_audio()
        except Exception as e:
            print(f"[TTS Error: {e}]")

if __name__ == "__main__":
    main()
