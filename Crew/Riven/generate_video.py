import os
import sys
from moviepy import *
from pathlib import Path

# Configuration
SIZE = (1280, 720) # 720p
FONT_COLOR = 'green'

def create_simple_video(output_dir):
    print("Initializing Simple Video Generation...")
    
    audio_path = os.path.join(output_dir, "podcast_final.mp3")
    if not os.path.exists(audio_path):
        print("Final audio not found.")
        return None
        
    # Create simple background (Black)
    # In a real scenario, we could load a 'serenity_logo.png' if it existed
    bg_clip = ColorClip(size=SIZE, color=(0, 0, 0), duration=1) # Duration fixed later
    
    # Add Title Text
    txt_clip = TextClip(
        text="SERENITY CORTEX\nAUDIO BRIEFING",
        font="Courier",
        font_size=70,
        color=FONT_COLOR,
        size=SIZE,
        method='caption',
        text_align='center'
    ).with_position('center')
    
    # Load Audio
    audio = AudioFileClip(audio_path)
    
    # Set durations
    video = CompositeVideoClip([bg_clip, txt_clip])
    video = video.with_duration(audio.duration)
    video = video.with_audio(audio)
    
    output_path = os.path.join(output_dir, "podcast_video.mp4")
    
    # Write file (faster preset)
    video.write_videofile(
        output_path, 
        fps=1, # 1 FPS is enough for a static image
        codec='libx264', 
        audio_codec='aac',
        preset='ultrafast',
        logger=None
    )
    
    print(f"Video generated: {output_path}")
    return output_path

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python generate_video.py <output_dir>")
    else:
        create_simple_video(sys.argv[1])