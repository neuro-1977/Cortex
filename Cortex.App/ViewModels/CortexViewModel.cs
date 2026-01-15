using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cortex.Core.Config;
using Cortex.Core.Models;
using Cortex.Core.Persistence;
using Cortex.Core.Services;
using Cortex.Core.LLM;
using Cortex.App.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cortex.App.ViewModels;

/// <summary>
/// ViewModel for the Cortex tab - handles sources, artifacts, and chat.
/// </summary>
public sealed partial class CortexViewModel : ObservableObject
{
    private readonly CortexStorageService _store = new();
    private readonly CortexIngestionService _ingestion = new();
    private readonly CortexChatService _chat = new();
    private readonly CortexStudioService _studio = new();
    private readonly CortexImageGenerationService _imageService = new();
    private readonly LlmClient _llm = new();
    private readonly DatabaseService _databaseService;
    internal readonly ElevenLabsTtsService _tts = new();

    [ObservableProperty] private CortexProject? currentProject;
    [ObservableProperty] private SourceDocument? selectedSource;
    [ObservableProperty] private CortexArtifact? selectedArtifact;

    public ObservableCollection<SourceDocument> Sources { get; } = new();
    public ObservableCollection<CortexArtifact> Artifacts { get; } = new();
    public ObservableCollection<CortexChatMessage> ChatHistory { get; } = new();

    [ObservableProperty] private string? chatInput;
    [ObservableProperty] private bool showChatOverlay;
    [ObservableProperty] private string? status;
    [ObservableProperty] private bool isGeneratingMedia;
    [ObservableProperty] private bool isGeneratingArtifact;
    [ObservableProperty] private string? generatingArtifactId;
    
    // Voice/TTS properties for ChatPill
    [ObservableProperty] private bool isVoiceMuted = false;
    [ObservableProperty] private bool isTtsMuted = false;
    [ObservableProperty] private bool isVoiceListening = false;
    [ObservableProperty] private bool isTtsSpeaking = false;
    [ObservableProperty] private string voiceStatusText = "Idle";
    
    // Graph visualization properties
    public bool IsGraphArtifact => SelectedArtifact?.Type == ArtifactType.MindMap;
    [ObservableProperty] private double artifactGraphZoom = 1.0;
    [ObservableProperty] private double artifactGraphOffsetX = 0;
    [ObservableProperty] private double artifactGraphOffsetY = 0;
    public ObservableCollection<Cortex.Core.Models.GraphNode> ArtifactGraphNodes { get; } = new();
    public ObservableCollection<Cortex.Core.Models.GraphEdge> ArtifactGraphEdges { get; } = new();

    [ObservableProperty] private string videoFormat = "Detailed";
    public string[] AllVideoFormats { get; } = { "Brief", "Detailed", "Tutorial", "Documentary" };
    
    [ObservableProperty] private ObservableCollection<string> selectedVideoHosts = new() { "Jax", "Zara" };
    [ObservableProperty] private bool useLipSync = true;

    public ObservableCollection<string> AvailableModels { get; } = new();
    [ObservableProperty] private string? selectedModel;

    // Artifact generation settings
    [ObservableProperty] private bool showQuizModal;
    [ObservableProperty] private bool showFlashCardModal;
    [ObservableProperty] private bool showDataTableModal;
    [ObservableProperty] private bool showSlideDeckModal;
    [ObservableProperty] private bool showBriefingDocModal;
    [ObservableProperty] private bool showInfographicModal;
    
    [ObservableProperty] private string quizLanguage = "English";
    [ObservableProperty] private string? quizDescription;
    [ObservableProperty] private string quizNumberOfQuestions = "Standard";
    [ObservableProperty] private string quizDifficulty = "Medium";
    
    [ObservableProperty] private int flashCardNumberOfCards = 10;
    [ObservableProperty] private string? flashCardDescription;
    [ObservableProperty] private string flashCardLanguage = "English";
    
    [ObservableProperty] private string? dataTableDescription;
    [ObservableProperty] private string dataTableLanguage = "English";
    
    [ObservableProperty] private string? slideDeckDescription;
    [ObservableProperty] private string slideDeckLanguage = "English";
    [ObservableProperty] private string slideDeckFormat = "Detailed Deck";
    [ObservableProperty] private string slideDeckLength = "Default";
    
    [ObservableProperty] private string? briefingDocDescription;
    [ObservableProperty] private string briefingDocLanguage = "English";
    [ObservableProperty] private string briefingDocFormat = "Comprehensive";
    [ObservableProperty] private string briefingDocLength = "Default";
    [ObservableProperty] private string briefingDocTone = "Formal";
    
    [ObservableProperty] private string? mindMapFocus;
    [ObservableProperty] private string mindMapFormat = "Deep Dive";
    [ObservableProperty] private string mindMapLength = "Default";
    [ObservableProperty] private bool mindMapIsEditable = false;
    
    [ObservableProperty] private string? audioOverviewDescription;
    [ObservableProperty] private string audioOverviewLanguage = "English";
    [ObservableProperty] private string audioOverviewFormat = "Deep Dive";
    [ObservableProperty] private string audioOverviewLength = "Default";

    [ObservableProperty] private string infographicOrientation = "Portrait";
    [ObservableProperty] private string infographicDetailLevel = "Standard";
    [ObservableProperty] private string? infographicDescription;
    [ObservableProperty] private string infographicLanguage = "English";

    public string[] InfographicOrientations { get; } = { "Landscape", "Portrait", "Square" };
    public string[] InfographicDetailLevels { get; } = { "Concise", "Standard", "Detailed" };
    public string[] Languages { get; } = { "English", "Spanish", "French", "German", "Italian", "Portuguese", "Chinese", "Japanese", "Korean", "Russian" };
    public string[] BriefingDocFormats { get; } = { "Comprehensive", "Executive Summary", "Tactical", "Timeline" };
    public string[] BriefingDocLengths { get; } = { "Short", "Default", "Long" };
    public string[] BriefingDocTones { get; } = { "Formal", "Casual", "Analytical" };
    public string[] SlideDeckFormats { get; } = { "Detailed Deck", "Presenter Slides" };
    public string[] SlideDeckLengths { get; } = { "Short", "Default", "Long" };
    public string[] QuizNumberOfQuestionsOptions { get; } = { "Fewer", "Standard", "More" };
    public string[] QuizDifficulties { get; } = { "Easy", "Medium", "Hard" };
    public string[] MindMapFormats { get; } = { "Deep Dive", "Brief", "Critique", "Debate" };
    public string[] MindMapLengths { get; } = { "Short", "Default", "Long" };
    public string[] AudioOverviewFormats { get; } = { "Deep Dive", "Brief", "Critique", "Debate" };
    public string[] AudioOverviewLengths { get; } = { "Short", "Default", "Long" };
    
    [ObservableProperty] private ArtifactType selectedArtifactType = ArtifactType.BriefingDoc;
    
    public bool ShowAudioOptions => SelectedArtifactType == ArtifactType.AudioOverview;
    public bool ShowVisualOptions => SelectedArtifactType is ArtifactType.MindMap or ArtifactType.Infographic or ArtifactType.SlideDeck or ArtifactType.VideoOverview;
    public bool ShowVideoOptions => SelectedArtifactType == ArtifactType.VideoOverview;
    public bool ShowQuizOptions => SelectedArtifactType == ArtifactType.Quiz;
    public bool ShowInfographicOptions => SelectedArtifactType == ArtifactType.Infographic;
    public bool ShowMindMapOptions => SelectedArtifactType == ArtifactType.MindMap;
    
    // Artifact content properties
    public Cortex.Core.Models.Slide? CurrentSlide => SelectedArtifactSlides.Count > CurrentSlideIndex && CurrentSlideIndex >= 0 ? SelectedArtifactSlides[CurrentSlideIndex] : null;
    [ObservableProperty] private int currentSlideIndex = 0;
    public ObservableCollection<Cortex.Core.Models.Slide> SelectedArtifactSlides { get; } = new();
    public ObservableCollection<Cortex.Core.Models.AudioTurn> SelectedArtifactAudioTurns { get; } = new();
    public ObservableCollection<Cortex.Core.Models.Slide> SelectedArtifactVideoSlides { get; } = new();
    public ObservableCollection<Cortex.Core.Models.QuizQuestion> SelectedArtifactQuizQuestions { get; } = new();
    public ObservableCollection<Cortex.Core.Models.FlashCard> SelectedArtifactFlashCards { get; } = new();
    public ObservableCollection<object> SelectedArtifactDataTableRows { get; } = new();
    public ObservableCollection<string> SelectedArtifactDataTableColumns { get; } = new();
    public Cortex.Core.Models.Slide? CurrentInfographicVideoSlide => SelectedArtifactVideoSlides.Count > CurrentSlideIndex && CurrentSlideIndex >= 0 ? SelectedArtifactVideoSlides[CurrentSlideIndex] : null;
    public int CurrentInfographicSlideIndexPlusOne => CurrentSlideIndex + 1;
    public bool IsMarkdownArtifact => SelectedArtifact?.Type == ArtifactType.BriefingDoc;

    public CortexViewModel(DatabaseService? databaseService = null)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Cortex",
            "cortex.db"
        );
        _databaseService = databaseService ?? new DatabaseService(dbPath);
        InitializeAvailableModels();
        _ = LoadProjectAsync();
    }

    private void InitializeAvailableModels()
    {
        AvailableModels.Clear();
        AvailableModels.Add("gpt-4o");
        AvailableModels.Add("gemini-1.5-pro");
        AvailableModels.Add("gemini-1.5-flash");
        AvailableModels.Add("grok-2");
        AvailableModels.Add("phi3:latest");
        SelectedModel = "gemini-1.5-flash";
    }

    [RelayCommand]
    private void ToggleChatOverlay()
    {
        ShowChatOverlay = !ShowChatOverlay;
    }

    [RelayCommand]
    private async Task AddSourceAsync()
    {
        try
        {
            await PerformanceProfiler.MeasureAsync("AddSource", async () =>
            {
                Status = "Select files to add as sources";
            });
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.AddSource");
            Status = $"Error adding source: {ex.Message}";
        }
    }

    public async Task AddSourceFromExternalAsync(string pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
        {
            Status = "Error: Path or URL cannot be empty.";
            return;
        }
        pathOrUrl = pathOrUrl.Trim();
        if (pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                Status = $"Error: Invalid URL format: {pathOrUrl}";
                return;
            }
        }
        else if (Path.IsPathRooted(pathOrUrl) || pathOrUrl.Contains(Path.DirectorySeparatorChar) || pathOrUrl.Contains(Path.AltDirectorySeparatorChar))
        {
            if (!File.Exists(pathOrUrl))
            {
                Status = $"Error: File not found: {pathOrUrl}";
                return;
            }
            var fileInfo = new FileInfo(pathOrUrl);
            if (fileInfo.Length > 100 * 1024 * 1024)
            {
                Status = $"Error: File too large (max 100MB): {fileInfo.Length / (1024 * 1024)}MB";
                return;
            }
        }
        await AddSourceFromExternalAsyncInternal(pathOrUrl);
    }
    
    private async Task AddSourceFromExternalAsyncInternal(string pathOrUrl)
    {
        if (CurrentProject == null) return;
        var input = (pathOrUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(input)) return;
        if (input.Contains("serenity.db", StringComparison.OrdinalIgnoreCase) || input.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
        {
            Status = "serenity.db is only viewable in Engine Room, not available as a Cortex source.";
            return;
        }
        Status = "Ingesting…";
        var src = await _ingestion.IngestAsync(input, CancellationToken.None);
        if (src == null) return;
        if ((!string.IsNullOrEmpty(src.Title) && src.Title.Contains("serenity.db", StringComparison.OrdinalIgnoreCase)) || (src.ExtractedText?.Contains("serenity.db", StringComparison.OrdinalIgnoreCase) == true) || (src.FilePath?.Contains("serenity.db", StringComparison.OrdinalIgnoreCase) == true))
        {
            Status = "serenity.db is only viewable in Engine Room, not available as a Cortex source.";
            return;
        }
        CurrentProject.Sources.Add(src);
        Sources.Add(src);
        SelectedSource = src;
        Status = src.IsProcessed ? $"Added source: {src.Title} (groundable)" : $"Added source: {src.Title} (extraction too small / failed)";
        await _store.SaveProjectAsync(CurrentProject);
    }

    [RelayCommand] private async Task GenerateSlideDeckAsync() => await GenerateArtifactInternalAsync(ArtifactType.SlideDeck, $"Format: {SlideDeckFormat}. Length: {SlideDeckLength}. Language: {SlideDeckLanguage}. {(string.IsNullOrWhiteSpace(SlideDeckDescription) ? "" : $"Description: {SlideDeckDescription}. ")}");
    [RelayCommand] private async Task GenerateMindMapAsync() => await GenerateArtifactInternalAsync(ArtifactType.MindMap, $"Format: {MindMapFormat}. Length: {MindMapLength}. {(string.IsNullOrWhiteSpace(MindMapFocus) ? "" : $"Focus: {MindMapFocus}. ")}", mindMapFormat: MindMapFormat, mindMapLength: MindMapLength, mindMapFocus: MindMapFocus);
    [RelayCommand] private async Task GenerateAudioOverviewAsync() => await GenerateArtifactInternalAsync(ArtifactType.AudioOverview, $"Format: {AudioOverviewFormat}. Length: {AudioOverviewLength}. Language: {AudioOverviewLanguage}. {(string.IsNullOrWhiteSpace(AudioOverviewDescription) ? "" : $"Description: {AudioOverviewDescription}. ")}", audioFormat: AudioOverviewFormat, audioLength: AudioOverviewLength, audioFocus: AudioOverviewDescription);
    [RelayCommand] private async Task GenerateVideoOverviewAsync() => await GenerateArtifactInternalAsync(ArtifactType.VideoOverview, "Video Format: Detailed. ");
    [RelayCommand] private async Task GenerateQuizAsync() => await GenerateArtifactInternalAsync(ArtifactType.Quiz, $"Number of Questions: {QuizNumberOfQuestions}. Difficulty: {QuizDifficulty}. Language: {QuizLanguage}. {(string.IsNullOrWhiteSpace(QuizDescription) ? "" : $"Description: {QuizDescription}. ")}");
    [RelayCommand] private async Task GenerateFlashCardsAsync() => await GenerateArtifactInternalAsync(ArtifactType.Flashcards, $"Number: {FlashCardNumberOfCards}. Language: {FlashCardLanguage}. {(string.IsNullOrWhiteSpace(FlashCardDescription) ? "" : $"Description: {FlashCardDescription}. ")}");
    [RelayCommand] private async Task GenerateDataTableAsync() => await GenerateArtifactInternalAsync(ArtifactType.DataTable, $"Language: {DataTableLanguage}. {(string.IsNullOrWhiteSpace(DataTableDescription) ? "" : $"Description: {DataTableDescription}. ")}");
    [RelayCommand] private async Task GenerateBriefingDocAsync() => await GenerateArtifactInternalAsync(ArtifactType.BriefingDoc, $"Format: {BriefingDocFormat}. Length: {BriefingDocLength}. Tone: {BriefingDocTone}. Language: {BriefingDocLanguage}. {(string.IsNullOrWhiteSpace(BriefingDocDescription) ? "" : $"Description: {BriefingDocDescription}. ")}");

    private async Task GenerateArtifactInternalAsync(ArtifactType type, string instructions, string? audioFormat = null, string? audioLength = null, string? audioFocus = null, string? mindMapFormat = null, string? mindMapLength = null, string? mindMapFocus = null)
    {
        if (CurrentProject == null || Sources.Count == 0) { Status = "Please add sources first"; return; }
        try
        {
            IsGeneratingMedia = true;
            Status = $"Generating {type}...";
            await PerformanceProfiler.MeasureAsync($"Generate{type}", async () =>
            {
                var artifact = await _studio.GenerateArtifactAsync(type, Sources.Where(s => s.IncludeInContext).ToList(), instructions, videoHosts: null, useLipSync: false, audioFormat, audioLength, audioFocus, mindMapFormat, mindMapLength, mindMapFocus, CancellationToken.None);
                if (artifact != null)
                {
                    CurrentProject.Artifacts.Insert(0, artifact);
                    Artifacts.Insert(0, artifact);
                    SelectedArtifact = artifact;
                    Status = $"{type} generated successfully";
                    if (RequiresMediaGeneration(type)) StartMediaGenerationMonitoring(artifact);
                }
            });
        }
        catch (Exception ex) { ErrorHandlingService.HandleException(ex, $"CortexViewModel.Generate{type}"); Status = $"Error generating {type}: {ex.Message}"; }
        finally { IsGeneratingMedia = false; GeneratingArtifactId = null; }
    }

    [RelayCommand]
    private async Task SendChatMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ChatInput) || CurrentProject == null) return;
        var userMessage = ChatInput;
        ChatInput = string.Empty;
        var userMsg = new CortexChatMessage { Sender = "Me", Kind = "user", Message = userMessage, Timestamp = DateTimeOffset.Now };
        ChatHistory.Add(userMsg);
        try
        {
            Status = "Thinking...";
            await PerformanceProfiler.MeasureAsync("SendChatMessage", async () =>
            {
                var result = await _chat.ChatWithCitationsAsync(userMessage, Sources.Where(s => s.IncludeInContext).ToList(), CancellationToken.None);
                if (result != null)
                {
                    var assistantMsg = new CortexChatMessage { Sender = "Cortex", Kind = "assistant", Message = result.ResponseText, Citations = result.Citations, Timestamp = DateTimeOffset.Now };
                    ChatHistory.Add(assistantMsg);
                    Status = "Response received";
                }
            });
        }
        catch (Exception ex) { ErrorHandlingService.HandleException(ex, "CortexViewModel.SendChatMessage"); Status = $"Error sending message: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task DeleteSourceAsync(SourceDocument? source)
    {
        if (source == null || CurrentProject == null) return;
        try { CurrentProject.Sources.Remove(source); Sources.Remove(source); await _store.SaveProjectAsync(CurrentProject); Status = "Source removed"; }
        catch (Exception ex) { ErrorHandlingService.HandleException(ex, "CortexViewModel.DeleteSource"); Status = $"Error removing source: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task DeleteArtifactAsync(CortexArtifact? artifact)
    {
        if (artifact == null || CurrentProject == null) return;
        try { CurrentProject.Artifacts.Remove(artifact); Artifacts.Remove(artifact); if (SelectedArtifact == artifact) SelectedArtifact = null; await _store.SaveProjectAsync(CurrentProject); Status = "Artifact removed"; }
        catch (Exception ex) { ErrorHandlingService.HandleException(ex, "CortexViewModel.DeleteArtifact"); Status = $"Error removing artifact: {ex.Message}"; }
    }

    [RelayCommand]
    private void GenerateArtifactDirectly(ArtifactType? type)
    {
        if (type.HasValue)
        {
            SelectedArtifactType = type.Value;
            if (type.Value == ArtifactType.Quiz) ShowQuizModal = true;
            else if (type.Value == ArtifactType.Flashcards) ShowFlashCardModal = true;
            else if (type.Value == ArtifactType.Infographic) { InfographicOrientation = "Portrait"; InfographicDetailLevel = "Standard"; PopulateInfographicDescriptionSuggestion(); ShowInfographicModal = true; }
            else if (type.Value == ArtifactType.DataTable) ShowDataTableModal = true;
            else if (type.Value == ArtifactType.SlideDeck) ShowSlideDeckModal = true;
            else if (type.Value == ArtifactType.BriefingDoc) ShowBriefingDocModal = true;
            else GenerateSelectedArtifactCommand.Execute(null);
        }
    }
    
    [RelayCommand]
    private async Task GenerateSelectedArtifact()
    {
        ShowQuizModal = false; ShowFlashCardModal = false; ShowInfographicModal = false; ShowDataTableModal = false; ShowSlideDeckModal = false; ShowBriefingDocModal = false;
        switch (SelectedArtifactType)
        {
            case ArtifactType.Quiz: await GenerateQuizAsync(); break;
            case ArtifactType.Flashcards: await GenerateFlashCardsAsync(); break;
            case ArtifactType.Infographic: Status = "Infographic generation not implemented yet"; break;
            case ArtifactType.DataTable: await GenerateDataTableAsync(); break;
            case ArtifactType.SlideDeck: await GenerateSlideDeckAsync(); break;
            case ArtifactType.BriefingDoc: await GenerateBriefingDocAsync(); break;
            case ArtifactType.MindMap: await GenerateMindMapAsync(); break;
            case ArtifactType.AudioOverview: await GenerateAudioOverviewAsync(); break;
            case ArtifactType.VideoOverview: await GenerateVideoOverviewAsync(); break;
        }
    }
    
    [RelayCommand]
    private void ShowOptionsForType(ArtifactType? type)
    {
        if (!type.HasValue) return;
        SelectedArtifactType = type.Value;
        switch (type.Value)
        {
            case ArtifactType.Quiz: ShowQuizModal = true; break;
            case ArtifactType.Flashcards: ShowFlashCardModal = true; break;
            case ArtifactType.DataTable: ShowDataTableModal = true; break;
            case ArtifactType.SlideDeck: ShowSlideDeckModal = true; break;
            case ArtifactType.BriefingDoc: ShowBriefingDocModal = true; break;
            case ArtifactType.Infographic: PopulateInfographicDescriptionSuggestion(); ShowInfographicModal = true; break;
            case ArtifactType.AudioOverview: GenerateArtifactDirectlyCommand.Execute(ArtifactType.AudioOverview); break;
            case ArtifactType.VideoOverview: GenerateArtifactDirectlyCommand.Execute(ArtifactType.VideoOverview); break;
            case ArtifactType.MindMap: GenerateArtifactDirectlyCommand.Execute(ArtifactType.MindMap); break;
        }
    }
    
    private void PopulateInfographicDescriptionSuggestion()
    {
        try {
            var contextText = string.Join("\n", Sources.Where(s => s.IncludeInContext).Select(s => s.ExtractedText).Take(3));
            if (string.IsNullOrWhiteSpace(contextText)) { InfographicDescription = "Create an infographic summarizing the key information from the selected sources."; return; }
            var preview = contextText.Length > 500 ? contextText.Substring(0, 500) + "..." : contextText;
            var lines = preview.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).Take(5);
            var summary = string.Join(" ", lines);
            InfographicDescription = $"Create an infographic about: {summary}";
        } catch { InfographicDescription = "Create an infographic summarizing the key information from the selected sources."; }
    }
    
    [RelayCommand] private void SetSelectedArtifact(CortexArtifact? artifact) => SelectedArtifact = artifact;

    [RelayCommand]
    private void ToggleVideoHost(string? hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName)) return;
        if (SelectedVideoHosts.Contains(hostName)) SelectedVideoHosts.Remove(hostName);
        else if (SelectedVideoHosts.Count < 5) SelectedVideoHosts.Add(hostName);
    }

    private bool RequiresMediaGeneration(ArtifactType type)
    {
        return type is ArtifactType.VideoOverview or ArtifactType.AudioOverview or ArtifactType.SlideDeck;
    }

    private void StartMediaGenerationMonitoring(CortexArtifact artifact)
    {
        GeneratingArtifactId = artifact.Id;
        // Media generation monitoring will be implemented when needed
    }

    [RelayCommand]
    private async Task LoadProjectAsync()
    {
        try
        {
            var projects = await _store.LoadAsync();
            if (projects.Count > 0)
            {
                CurrentProject = projects[0];
                Sources.Clear();
                Artifacts.Clear();
                ChatHistory.Clear();
                foreach (var src in CurrentProject.Sources) Sources.Add(src);
                foreach (var art in CurrentProject.Artifacts) Artifacts.Add(art);
                foreach (var msg in CurrentProject.ChatHistory) ChatHistory.Add(msg);
                Status = $"Loaded project: {CurrentProject.Name}";
            }
            else
            {
                CurrentProject = new CortexProject { Name = "New Project" };
                await _store.SaveProjectAsync(CurrentProject);
                Status = "Created new project";
            }
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.LoadProject");
            Status = $"Error loading project: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SaveProjectAsync()
    {
        if (CurrentProject == null) return;
        try
        {
            await _store.SaveProjectAsync(CurrentProject);
            Status = "Project saved";
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.SaveProject");
            Status = $"Error saving project: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ImportNotebooksAsync()
    {
        try
        {
            // This will be called from code-behind with file picker
            Status = "Import notebooks - use file picker";
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.ImportNotebooks");
            Status = $"Error importing notebooks: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportNotebooksAsync()
    {
        try
        {
            // This will be called from code-behind with file picker
            Status = "Export notebooks - use file picker";
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.ExportNotebooks");
            Status = $"Error exporting notebooks: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NewProjectAsync()
    {
        try
        {
            // Create new project
            var newProject = new CortexProject 
            { 
                Name = "New Project",
                Id = Guid.NewGuid().ToString()
            };
            
            // Clear current collections
            Sources.Clear();
            Artifacts.Clear();
            ChatHistory.Clear();
            
            // Set new project
            CurrentProject = newProject;
            
            // Save it
            await _store.SaveProjectAsync(newProject);
            Status = "Created new project";
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.NewProject");
            Status = $"Error creating new project: {ex.Message}";
        }
    }

    public async Task ImportNotebooksFromFileAsync(string filePath)
    {
        try
        {
            var importedProjects = await _store.ImportAsync(filePath);
            if (importedProjects.Count > 0)
            {
                // Merge imported projects with existing ones
                var allProjects = await _store.LoadAsync();
                foreach (var project in importedProjects)
                {
                    // Check if project with same ID exists
                    var existing = allProjects.FirstOrDefault(p => p.Id == project.Id);
                    if (existing != null)
                    {
                        // Update existing project
                        var index = allProjects.IndexOf(existing);
                        allProjects[index] = project;
                    }
                    else
                    {
                        // Add new project
                        allProjects.Add(project);
                    }
                }
                
                // Save all projects
                await _store.SaveAsync(allProjects);
                
                // Load the first imported project
                if (importedProjects.Count > 0)
                {
                    CurrentProject = importedProjects[0];
                    Sources.Clear();
                    Artifacts.Clear();
                    ChatHistory.Clear();
                    foreach (var src in CurrentProject.Sources) Sources.Add(src);
                    foreach (var art in CurrentProject.Artifacts) Artifacts.Add(art);
                    foreach (var msg in CurrentProject.ChatHistory) ChatHistory.Add(msg);
                }
                
                Status = $"Imported {importedProjects.Count} project(s)";
            }
            else
            {
                Status = "No projects found in import file";
            }
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.ImportNotebooksFromFile");
            Status = $"Error importing notebooks: {ex.Message}";
        }
    }

    public async Task ExportNotebooksToFileAsync(string filePath)
    {
        try
        {
            var allProjects = await _store.LoadAsync();
            await _store.ExportAsync(allProjects, filePath);
            Status = $"Exported {allProjects.Count} project(s) to {System.IO.Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            ErrorHandlingService.HandleException(ex, "CortexViewModel.ExportNotebooksToFile");
            Status = $"Error exporting notebooks: {ex.Message}";
        }
    }
}

