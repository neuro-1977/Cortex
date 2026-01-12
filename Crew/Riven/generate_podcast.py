import asyncio
import os
import json
import re
import sys
import edge_tts
import requests
from pathlib import Path

# Configuration
OLLAMA_URL = "http://localhost:11434/api/generate"
MODEL_NAME = "river"
OUTPUT_DIR = os.path.join(os.path.dirname(__file__), "podcast_output")
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Voices (Edge TTS)
# Mal (Host) - Male, confident
VOICE_HOST = "en-US-ChristopherNeural" 
# River (Guest) - Female, soft, slightly mystic
VOICE_RIVER = "en-US-AriaNeural"

SYSTEM_PROMPT = """
You are an expert audio producer.
Convert the provided Research Report into a lively, 2-person podcast script.

**CHARACTERS:**
1. **Mal** (Host): Charming, asks layman questions, keeps it moving.
2. **River** (Expert): The researcher. Intelligent, slightly cryptic but informative.

**INSTRUCTIONS:**
- Turn the report's dry facts into a dialogue.
- Mal should introduce the topic and ask questions.
- River should explain the findings using the report's data.
- Keep it under 5 minutes of dialogue.
- **FORMAT:** Output strict JSON list of objects: `[{"speaker": "Mal", "text": "Hello folks..."}, {"speaker": "River", "text": "Hi Mal..."}]`
"""

def generate_script(report_text):
    print("Generating podcast script from report...")
    
    prompt = f"{SYSTEM_PROMPT}\n\n**REPORT:**\n{report_text}\n\n**JSON SCRIPT:**"
    
    payload = {
        "model": MODEL_NAME,
        "prompt": prompt,
        "format": "json",
        "stream": False
    }
    
    try:
        response = requests.post(OLLAMA_URL, json=payload)
        response.raise_for_status()
        result = response.json()['response']
        # Sanitize json if needed (sometimes models add text before/after)
        start = result.find('[')
        end = result.rfind(']') + 1
        if start != -1 and end != -1:
            return json.loads(result[start:end])
        return json.loads(result)
    except Exception as e:
        print(f"Error generating script: {e}")
        return None

async def synthesize_audio(script):
    print("Synthesizing audio...")
    files = []
    
    for i, line in enumerate(script):
        if isinstance(line, str):
            # Fallback if the model returned ["Speaker: text", ...]
            parts = line.split(':', 1)
            if len(parts) == 2:
                speaker = parts[0].strip().title()
                text = parts[1].strip()
            else:
                continue
        elif isinstance(line, dict):
            speaker = line.get("speaker", "Mal").title()
            text = line.get("text", "")
        else:
            continue
        
        if not text: continue
        
        voice = VOICE_RIVER if "River" in speaker else VOICE_HOST
        
        filename = f"{i:03d}_{speaker}.mp3"
        filepath = os.path.join(OUTPUT_DIR, filename)
        
        print(f"  Generating line {i+1}: {speaker} -> {filepath}")
        
        communicate = edge_tts.Communicate(text, voice)
        await communicate.save(filepath)
        files.append(filename)
        
    return files

def create_playlist(files):
    playlist_path = os.path.join(OUTPUT_DIR, "play_podcast.m3u")
    with open(playlist_path, 'w') as f:
        for file in files:
            f.write(file + "\n")
    print("Open the .m3u file with VLC, Windows Media Player, or any audio player to listen.")

    # Concatenate with FFmpeg
    final_output = os.path.join(OUTPUT_DIR, "podcast_final.mp3")
    print(f"Merging into {final_output}...")
    
    # Create input list for ffmpeg
    list_path = os.path.join(OUTPUT_DIR, "ffmpeg_list.txt")
    with open(list_path, 'w', encoding='utf-8') as f:
        for file in files:
            f.write(f"file '{file}'\n")
            
    # Run FFmpeg
    try:
        import subprocess
        # Check if ffmpeg is available
        subprocess.run(["ffmpeg", "-version"], check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        
        cmd = ["ffmpeg", "-f", "concat", "-safe", "0", "-i", list_path, "-c", "copy", final_output, "-y"]
        subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        print(f"Successfully created: {final_output}")
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("FFmpeg not found or failed. Returning separate files.")

    # Generate Video
    try:
        video_script = os.path.join(os.path.dirname(__file__), "generate_video.py")
        print("Generating video...")
        subprocess.run([sys.executable, video_script, OUTPUT_DIR], check=True)
    except Exception as e:
        print(f"Video generation failed: {e}")

def main():
    if len(sys.argv) < 2:
        print("Usage: python generate_podcast.py <path_to_report.md>")
        print("Or provide text via stdin.")
        return

    report_path = sys.argv[1]
    
    if os.path.exists(report_path):
        with open(report_path, 'r', encoding='utf-8') as f:
            report_text = f.read()
    else:
        print("File not found.")
        return

    script = generate_script(report_text)
    if script:
        # Save script for debug
        with open(os.path.join(OUTPUT_DIR, "script.json"), 'w') as f:
            json.dump(script, f, indent=2)
            
        asyncio.run(synthesize_audio(script))
        
        # Re-list files to ensure order (glob might differ, but our numbering 001_ helps)
        files = sorted([f for f in os.listdir(OUTPUT_DIR) if f.endswith(".mp3")])
        create_playlist(files)

if __name__ == "__main__":
    main()
