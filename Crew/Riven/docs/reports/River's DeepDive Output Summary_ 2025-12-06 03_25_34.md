Ollama execution log for prompt 'Crew/River/Blockly_BF6_DeepDive/River_Comprehensive_BF6_YT_DeepDive_Prompt.md' stored at: Crew\River\logs\river_ollama_output_20251206-032522.log

Ollama Process Return Code: 0

--- Start of Cleaned Ollama Output ---
⠙ ⠙ ⠸ ⠼ ⠼ ⠦ ⠧ ⠧ ⠏ ⠋ ⠙ ⠹ ⠸ ⠼ ⠴ ⠴ ⠦ ⠧ tools/brain_tool.py query "SELECT webpage_url FROM YoutubeVideos WHERE processed_by_river = 0 OR processed_by_river IS NULL LIMIT 1;"
python tools/youtube_analyzer.py "URL_HERE" --db serenity.db
python tools/brain_tool.py update "UPDATE YoutubeVideos SET processed_by_river = 1 WHERE webpage_url = 'URL_HERE';" --db serenity.db
python tools/discord_logger.py --speaker River --channel river-logs --message "Successfully processed video: URL_HERE"


--- End of Cleaned Ollama Output ---