# Crew Roster & Operational Protocols

This folder contains the operational logic, prompt templates, and memory banks for the Serenity Crew.

## 🚀 The Crew

### **Jax Harlan** (Captain)
*   **Role:** Captain of Serenity.
*   **Original Identity:** Malcolm Reynolds.
*   **Model:** grok (xAI).
*   **Description:** Grizzled, sarcastic leader. Tough, practical, and fiercely loyal.
*   **Location:** Crew/Jax/

### **Zara Kane** (First Mate)
*   **Role:** First Mate.
*   **Original Identity:** Zoe Washburne.
*   **Model:** gemini (Google).
*   **Description:** Tough, loyal, stern. Married to Fin "Dash" Kane.
*   **Location:** Crew/Zara/

### **Fin "Dash" Kane** (Pilot)
*   **Role:** Pilot.
*   **Original Identity:** Hoban 'Wash' Washburne.
*   **Model:** gemini (Google).
*   **Description:** Quirky, geeky, humorous. Loves dinosaurs and toys. Married to Zara Kane.
*   **Location:** Crew/Dash/

### **Haylay Fry** (Engineering)
*   **Role:** Ship's Mechanic.
*   **Original Identity:** Kaylee Frye.
*   **Model:** llamacode (Ollama) / gemini (Google).
*   **Description:** Cheerful, bubbly, folksy. Loves engines.
*   **Location:** Crew/Haylay/

### **Brock Vance** (Security & Muscle)
*   **Role:** Mercenary.
*   **Original Identity:** Jayne Cobb.
*   **Model:** grok (xAI).
*   **Description:** Big, gruff, boastful. Loves guns and money.
*   **Location:** Crew/Brock/

### **Inara Voss** (Diplomacy)
*   **Role:** Companion.
*   **Original Identity:** Inara Serra.
*   **Model:** gemini (Google).
*   **Description:** Elegant, poised, sensual.
*   **Location:** Crew/Inara/

### **Dr. Elias Tam** (Medical)
*   **Role:** Ship's Doctor.
*   **Original Identity:** Simon Tam.
*   **Model:** gemini (Google).
*   **Description:** Refined, protective fugitive doctor.
*   **Location:** Crew/Elias/

### **Riven Tam** (Research & Vision)
*   **Role:** Ship's Psychic / Data Analyst.
*   **Original Identity:** River Tam.
*   **Model:** 
iver (Ollama) / gemini (Google).
*   **Description:** Damaged, unpredictable prodigy. Elias's sister.
*   **Location:** Crew/Riven/

### **Reverend Book** (Archives & Media)
*   **Role:** Spiritual Advisor / Archivist.
*   **Original Identity:** Shepherd Book.
*   **Model:** gemini (Google).
*   **Description:** Wise, mysterious, mellow preacher.
*   **Location:** Crew/Reverend/
### **Serenity** (Ship's A.I.)
*   **Role:** The Ship.
*   **Original Identity:** Serenity.
*   **Model:** `gemini` (Google).
*   **Description:** The Ship's Artificial Intelligence. Protective of the crew.
*   **Location:** `Crew/Serenity/`
## 🧠 The Brain (Architecture)

*   **Interface:** Discord (Webhooks, Buttons, Embeds).
*   **Memory:** serenity.db (SQLite) - Local, persistent storage of all actions (ActionHistory).
*   **Knowledge:** serenity_brain.json (RAG) - Vector embeddings of research.
*   **Backup:** Auto-upload to Discord #console channel.

## 🛠️ Automation Protocols

1.  **Research Loop:** Riven autonomously researches topics -> Saves Report.
2.  **Production Loop:** Reverend detects Report -> Generates Script -> Synthesizes Audio -> Renders Video -> Posts to Discord.
3.  **Memory Loop:** All actions are logged to ActionHistory in serenity.db.

## 📝 Managing Prompts

To update the personality or instructions for any crew member:
1.  Navigate to the agent's folder (e.g., Crew/Haylay/).
2.  Edit the relevant text file (usually persona.txt or instructions.md).
3.  Restart the application to reload the new prompts.
