using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serenity.Cortex.Core.Config;

namespace Serenity.Cortex.Core.Services
{
    public class DatabaseService
    {
        private static DatabaseService? _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private readonly string _dbPath;
        private readonly string _dataFolder;
        private readonly string _researchFile;
        private readonly string _projectsFile;
        private readonly string _personasFile;
        private readonly string _chatLogFile;
        private HashSet<string> _recentMessageIds = new HashSet<string>();

        // Constructor for DI or explicit usage with path
        public DatabaseService(string dbPath)
        {
            _dbPath = dbPath;
            _dataFolder = Path.GetDirectoryName(dbPath) ?? Path.Combine(AppContext.BaseDirectory, "data");
            _researchFile = Path.Combine(_dataFolder, "research_history.json");
            _projectsFile = Path.Combine(_dataFolder, "projects.json");
            _personasFile = Path.Combine(_dataFolder, "personas.json");
            _chatLogFile = Path.Combine(_dataFolder, "chat_logs.json");
        }

        // Singleton Parameterless Constructor (Uses Config/Defaults)
        private DatabaseService()
        {
            // Resolve Data Folder
            string? configuredData = CortexConfig.Get("DATA_FOLDER");
            if (string.IsNullOrEmpty(configuredData))
            {
                // Fallback logic similar to Native
                 string root = AppContext.BaseDirectory;
                 string foundData = Path.Combine(root, "data");
                 
                 // Walk up to find the root containing "serenity.code-workspace" or "serenity.db"
                 DirectoryInfo? dir = new DirectoryInfo(root);
                 while (dir != null)
                 {
                     if (File.Exists(Path.Combine(dir.FullName, "serenity.code-workspace")) || Directory.Exists(Path.Combine(dir.FullName, "data")))
                     {
                         string check = Path.Combine(dir.FullName, "data");
                         if (Directory.Exists(check))
                         {
                             foundData = check;
                             break;
                         }
                     }
                     dir = dir.Parent;
                 }
                 configuredData = foundData;
            }

            _dataFolder = configuredData;
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }

            _dbPath = Path.Combine(_dataFolder, "serenity.db");
            _researchFile = Path.Combine(_dataFolder, "research_history.json");
            _projectsFile = Path.Combine(_dataFolder, "projects.json");
            _personasFile = Path.Combine(_dataFolder, "personas.json");
            _chatLogFile = Path.Combine(_dataFolder, "chat_logs.json");
        }

        public string DataFolder => _dataFolder;

        // --- SQLite Methods (Existing) ---

        public List<KnowledgeItem> GetKnowledgeItems()
        {
            var items = new List<KnowledgeItem>();
            if (!File.Exists(_dbPath)) return items;

            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, filename, extension, type, workspace, path FROM Knowledge LIMIT 100";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new KnowledgeItem
                    {
                        Id = reader.GetInt32(0),
                        Filename = reader.GetString(1),
                        Extension = reader.GetString(2),
                        Type = reader.GetString(3),
                        Workspace = reader.GetString(4),
                        Path = reader.GetString(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Knowledge: {ex.Message}");
            }

            return items;
        }

        public List<SystemDocItem> GetSystemDocs()
        {
            var items = new List<SystemDocItem>();
            if (!File.Exists(_dbPath)) return items;

            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, name FROM SystemDocs LIMIT 50";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new SystemDocItem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading SystemDocs: {ex.Message}");
            }

            return items;
        }

        public List<TableInfo> GetTables()
        {
            var tables = new List<TableInfo>();
            if (!File.Exists(_dbPath)) return tables;

            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                // Get all table names
                var tableCommand = connection.CreateCommand();
                tableCommand.CommandText = @"
                    SELECT name FROM sqlite_master 
                    WHERE type='table' AND name NOT LIKE 'sqlite_%'
                    ORDER BY name";

                using var tableReader = tableCommand.ExecuteReader();
                while (tableReader.Read())
                {
                    var tableName = tableReader.GetString(0);
                    var columns = GetColumns(connection, tableName);
                    
                    tables.Add(new TableInfo
                    {
                        Name = tableName,
                        Columns = columns
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tables: {ex.Message}");
            }

            return tables;
        }

        private List<ColumnInfo> GetColumns(SqliteConnection connection, string tableName)
        {
            var columns = new List<ColumnInfo>();
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({tableName})";
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(new ColumnInfo
                    {
                        Name = reader.GetString(1),
                        Type = reader.GetString(2),
                        NotNull = reader.GetInt32(3) == 1,
                        DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                        PrimaryKey = reader.GetInt32(5) == 1
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading columns for {tableName}: {ex.Message}");
            }
            return columns;
        }

        // --- JSON Persistence Methods (Ported) ---

        public async Task SaveResearchAsync(ResearchItem item)
        {
            var list = await LoadResearchAsync();
            
            // Check for existing by ID or DiscordMessageId to prevent duplicates
            var existing = list.FirstOrDefault(x => x.Id == item.Id || 
                (!string.IsNullOrEmpty(item.DiscordMessageId) && x.DiscordMessageId == item.DiscordMessageId));
            
            if (existing != null)
            {
                if (item.Id != existing.Id) item.Id = existing.Id;
                list.Remove(existing);
            }
            
            list.Add(item);
            await SaveFileAsync(_researchFile, list);
        }

        public async Task<List<ResearchItem>> LoadResearchAsync()
        {
            return await LoadFileAsync<List<ResearchItem>>(_researchFile) ?? new List<ResearchItem>();
        }

        public async Task SavePersonaAsync(Persona item)
        {
            var list = await LoadPersonasAsync();
            var existing = list.FirstOrDefault(p => p.Id == item.Id);
            if (existing != null) list.Remove(existing);
            list.Add(item);
            await SaveFileAsync(_personasFile, list);
        }

        public async Task<List<Persona>> LoadPersonasAsync()
        {
            return await LoadFileAsync<List<Persona>>(_personasFile) ?? new List<Persona>();
        }

        public async Task LogChatMessageAsync(string author, string channel, string message, string timestamp, string messageId = "")
        {
            if (!string.IsNullOrEmpty(messageId))
            {
                if (_recentMessageIds.Contains(messageId)) return;
                _recentMessageIds.Add(messageId);
                if (_recentMessageIds.Count > 10000) _recentMessageIds.Clear();
            }

            string dailyLogFile = Path.Combine(_dataFolder, $"chat_log_{DateTime.Now:yyyy-MM-dd}.txt");
            string logEntry = $"[{timestamp}] [ID:{messageId}] [{channel}] {author}: {message}\n";
            
            try
            {
                await File.AppendAllTextAsync(dailyLogFile, logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log chat: {ex.Message}");
            }
        }

        // --- Helpers ---

        private async Task<T?> LoadFileAsync<T>(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(path);
                    return JsonSerializer.Deserialize<T>(json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading {path}: {ex.Message}");
                }
            }

            string backupPath = path + ".bak";
            if (File.Exists(backupPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(backupPath);
                    var data = JsonSerializer.Deserialize<T>(json);
                    await File.WriteAllTextAsync(path, json);
                    return data;
                }
                catch
                {
                    return default;
                }
            }

            return default;
        }

        private async Task SaveFileAsync<T>(string path, T data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            try
            {
                await File.WriteAllTextAsync(tempPath, json);
                if (File.Exists(path))
                {
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                    File.Move(path, backupPath);
                }
                File.Move(tempPath, path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving {path}: {ex.Message}");
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
    }

    // --- Domain Classes ---

    public class TableInfo
    {
        public string Name { get; set; } = "";
        public List<ColumnInfo> Columns { get; set; } = new();
    }

    public class ColumnInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool NotNull { get; set; }
        public string? DefaultValue { get; set; }
        public bool PrimaryKey { get; set; }
    }

    public class KnowledgeItem
    {
        public int Id { get; set; }
        public string Filename { get; set; } = "";
        public string Extension { get; set; } = "";
        public string Type { get; set; } = "";
        public string Workspace { get; set; } = "";
        public string Path { get; set; } = "";
    }

    public class SystemDocItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class ResearchItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        private string? _name;
        public string Name 
        { 
            get => !string.IsNullOrEmpty(_name) ? _name! : Prompt; 
            set => _name = value; 
        }

        public string Type { get; set; } = "Text"; // "Audio", "Video", "Image", "Document", "Text"
        public string Prompt { get; set; } = "";
        public string Result { get; set; } = ""; // File path or content
        public string Status { get; set; } = "";
        
        private string? _icon;
        public string Icon
        {
            get 
            {
                if (!string.IsNullOrEmpty(_icon)) return _icon!;
                return Type switch
                {
                    "Image" => "🎨",
                    "Video" => "🎬",
                    "Audio" => "🎧",
                    "Document" => "📄",
                    "Text" => "📝",
                    _ => "❓"
                };
            }
            set => _icon = value;
        }
        
        // Integration Fields
        public string? DiscordMessageId { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Summary { get; set; }
        public List<string> SuggestedPrompts { get; set; } = new List<string>();

        public bool IsImageOrVideo => Type == "Image" || Type == "Video";
        public bool IsNotImageOrVideo => !IsImageOrVideo;
    }

    public class Persona
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string SystemPrompt { get; set; } = "";
        public string VoiceId { get; set; } = "";
        public string DiscordId { get; set; } = "";
    }
}

