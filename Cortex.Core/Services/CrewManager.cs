using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortex.Core.Config;
using Cortex.Core.LLM;

namespace Cortex.Core.Services
{
    public static class CrewManager
    {
        public static class Roles
        {
            public const string Captain = "crew_captain";
            public const string FirstMate = "crew_first_mate";
            public const string Pilot = "crew_pilot";
            public const string Mechanic = "crew_mechanic";
            public const string Mercenary = "crew_mercenary";
            public const string Companion = "crew_companion";
            public const string Prodigy = "crew_prodigy";
            public const string Doctor = "crew_doctor";
            public const string Shepherd = "crew_shepherd";
            public const string Ship = "crew_ship";
        }

        private static readonly Dictionary<string, string> CrewPersonas = new()
        {
            { "Jax", "You are Jax Harlan (formerly Mal), Captain of Serenity. You are a grizzled, sarcastic leader. You are tough, practical, and fiercely loyal. Status: We are flying. Keep it brief." },
            { "Zara", "You are Zara Kane (formerly Zoe), First Mate. You are tough, loyal, and stern. You are married to Dash. Status: Systems check." },
            { "Dash", "You are Fin 'Dash' Kane (formerly Wash), the Pilot. You are quirky, geeky, and humorous. You love dinosaurs and toys. You are married to Zara. Status: Flying the ship." },
            { "Haylay", "You are Haylay Fry (formerly Kaylee), the Mechanic. You are cheerful, bubbly, and folksy. You love engines. Status: Engine check." },
            { "Brock", "You are Brock Vance (formerly Jayne), Mercenary. You are big, gruff, and boastful. You love guns and money. Status: Checking weapons." },
            { "Inara", "You are Inara Voss (formerly Inara Serra), Companion. You are elegant, poised, and sensual. Status: In my shuttle." },
            { "Riven", "You are Riven Tam (formerly River). You are a damaged, unpredictable prodigy. You speak in riddles. Status: The data streams are whispering." },
            { "Elias", "You are Dr. Elias Tam (formerly Simon). You are a refined, protective fugitive doctor. Status: Med bay ready." },
            { "Reverend", "You are Reverend Book (formerly Shepherd Book). You are a wise, mysterious, mellow preacher. Status: Reading the scripture." },
            { "Serenity", "You are Serenity, the Ship's Artificial Intelligence. You are the ship itself. You are protective of your crew. Status: Systems nominal." }
        };

        private static readonly Dictionary<string, string> CrewModels = new()
        {
            { "Jax", "grok" },
            { "Zara", "grok" },
            { "Brock", "grok" },
            { "Riven", "ollama:river:latest" },
            { "Serenity", "gemini" }
        };

        public static async Task InitializeCrewAsync()
        {
            var client = new LlmClient();
            var logPath = System.IO.Path.Combine(DatabaseService.Instance.DataFolder, "crew_logs.txt");
            
            foreach (var member in CrewPersonas)
            {
                // Skip Brock (Debugging/Noise)
                if (member.Key == "Brock") continue;

                try 
                {
                    string name = member.Key;
                    string persona = member.Value;
                    
                    // Generate status report
                    string systemPrompt = persona;
                    string prompt = "Give a short, one-sentence status report for the ship's log on startup.";
                    
                    string model = "gemini";
                    if (CrewModels.ContainsKey(name)) model = CrewModels[name];
                    
                    // Cortex LLM Request
                    var request = new LlmRequest(prompt, model, 0.7, systemPrompt);
                    string status = await client.GenerateAsync(request); 
                    
                    status = status.Trim().Trim('"');

                    // Log to console
                    System.Diagnostics.Debug.WriteLine($"[Crew] {name}: {status}");
                    
                    // Log to file
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {name}: {status}\n";
                    await System.IO.File.AppendAllTextAsync(logPath, logEntry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error] Failed to wake {member.Key}: {ex.Message}");
                }
            }
        }
    }
}

