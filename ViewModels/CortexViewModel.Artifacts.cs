using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Serenity.Diagnostics;
using Serenity.Cortex.Core.Config;
using Serenity.Cortex.Core.Models;
using Serenity.Cortex.Core.Services;
using Serenity.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Serenity.ViewModels;

public sealed partial class CortexViewModel
{
    // ---- Graph Visualization ----
    
    public ObservableCollection<GraphNode> ArtifactGraphNodes { get; } = new();
    public ObservableCollection<GraphEdge> ArtifactGraphEdges { get; } = new();
    
    [ObservableProperty]
    private double _artifactGraphZoom = 1.0;
    
    [ObservableProperty]
    private double _artifactGraphOffsetX = 0;
    
    [ObservableProperty]
    private double _artifactGraphOffsetY = 0;
    
    // ---- Slides ----
    
    public ObservableCollection<SlideItem> SelectedArtifactSlides { get; } = new();
    
    [ObservableProperty]
    private int currentSlideIndex = -1;
    
    public bool CanGoToPreviousSlide => CurrentSlideIndex > 0;
    public bool CanGoToNextSlide => CurrentSlideIndex >= 0 && CurrentSlideIndex < SelectedArtifactSlides.Count - 1;
    
    public SlideItem? CurrentSlide => CurrentSlideIndex >= 0 && CurrentSlideIndex < SelectedArtifactSlides.Count 
        ? SelectedArtifactSlides[CurrentSlideIndex] 
        : null;

    partial void OnCurrentSlideIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoToPreviousSlide));
        OnPropertyChanged(nameof(CanGoToNextSlide));
        OnPropertyChanged(nameof(CurrentSlide));
    }
    
    [RelayCommand]
    private void PreviousSlide()
    {
        if (CurrentSlideIndex > 0)
        {
            CurrentSlideIndex--;
        }
    }
    
    [RelayCommand]
    private void NextSlide()
    {
        if (CurrentSlideIndex < SelectedArtifactSlides.Count - 1)
        {
            CurrentSlideIndex++;
        }
    }

    // ---- Video/Infographic Slides ----
    
    public ObservableCollection<VideoSlide> SelectedArtifactVideoSlides { get; } = new();
    
    [ObservableProperty]
    private int currentInfographicSlideIndex = -1;
    
    public VideoSlide? CurrentInfographicVideoSlide => CurrentInfographicSlideIndex >= 0 && CurrentInfographicSlideIndex < SelectedArtifactVideoSlides.Count ? SelectedArtifactVideoSlides[CurrentInfographicSlideIndex] : null;

    public bool CanGoToPreviousInfographicSlide => CurrentInfographicSlideIndex > 0;
    public bool CanGoToNextInfographicSlide => CurrentInfographicSlideIndex >= 0 && CurrentInfographicSlideIndex < SelectedArtifactVideoSlides.Count - 1;
    
    public int CurrentInfographicSlideIndexPlusOne => CurrentInfographicSlideIndex + 1;
    
    partial void OnCurrentInfographicSlideIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoToPreviousInfographicSlide));
        OnPropertyChanged(nameof(CanGoToNextInfographicSlide));
        OnPropertyChanged(nameof(CurrentInfographicSlideIndexPlusOne));
        OnPropertyChanged(nameof(CurrentInfographicVideoSlide));
    }
    
    [RelayCommand]
    private void PreviousInfographicSlide()
    {
        if (CurrentInfographicSlideIndex > 0)
        {
            CurrentInfographicSlideIndex--;
        }
    }
    
    [RelayCommand]
    private void NextInfographicSlide()
    {
        if (CurrentInfographicSlideIndex < SelectedArtifactVideoSlides.Count - 1)
        {
            CurrentInfographicSlideIndex++;
        }
    }

    // ---- Audio ----
    
    public ObservableCollection<AudioTurn> SelectedArtifactAudioTurns { get; } = new();
    
    private LibVLC? _audioLibVlc;
    private MediaPlayer? _audioMediaPlayer;
    private Media? _currentAudioMedia;
    private int _currentAudioTurnIndex = -1;
    private bool _isPlayingAudioSequence = false;
    private CancellationTokenSource? _audioPlaybackCts;
    
    [ObservableProperty]
    private string? audioOverviewStatus;

    // ---- Quiz/FlashCards/DataTable ----
    
    public ObservableCollection<QuizQuestion> SelectedArtifactQuizQuestions { get; } = new();
    public ObservableCollection<FlashCard> SelectedArtifactFlashCards { get; } = new();
    public ObservableCollection<DataTableRow> SelectedArtifactDataTableRows { get; } = new();
    public List<string> SelectedArtifactDataTableColumns { get; } = new();

    // ---- Media Player (General) ----
    
    private LibVLC? _libVlc;
    [ObservableProperty] private MediaPlayer? _mediaPlayer;

    // ---- Convenience Properties ----
    
    public bool HasSelectedArtifact => SelectedArtifact != null;
    public bool HasNoSelectedArtifact => SelectedArtifact == null;
    public string? SelectedArtifactVisualPath => SelectedArtifact?.VisualPath;

    public bool IsMarkdownArtifact => SelectedArtifact?.Type is ArtifactType.BriefingDoc or ArtifactType.DataTable;
    public bool IsAudioArtifact => SelectedArtifact?.Type == ArtifactType.AudioOverview;
    public bool IsSlideArtifact => SelectedArtifact?.Type == ArtifactType.SlideDeck;
    public bool IsGraphArtifact => SelectedArtifact?.Type is ArtifactType.MindMap or ArtifactType.Infographic;
    public bool IsVideoArtifact => SelectedArtifact?.Type == ArtifactType.VideoOverview;
    
    // ---- Logic ----

    [RelayCommand]
    private void ZoomInArtifactGraph() => ArtifactGraphZoom *= 1.2;

    [RelayCommand]
    private void ZoomOutArtifactGraph() => ArtifactGraphZoom /= 1.2;

    [RelayCommand]
    private void ResetArtifactGraphView()
    {
        ArtifactGraphZoom = 1.0;
        ArtifactGraphOffsetX = 0;
        ArtifactGraphOffsetY = 0;
    }

    [RelayCommand]
    private void OpenSourceInTerminal(SourceDocument? source)
    {
        if (source == null || string.IsNullOrWhiteSpace(source.FilePath)) return;
        try
        {
            var directory = Path.GetDirectoryName(source.FilePath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "bash",
                    Arguments = OperatingSystem.IsWindows() ? $"/k cd /d \"{directory}\"" : $"-c \"cd '{directory}'; bash\"",
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task ExportArtifactMindMap()
    {
        if (SelectedArtifact == null || ArtifactGraphNodes.Count == 0) return;
        try
        {
            var outputDir = Serenity.Cortex.Core.Helpers.PathHelper.GetOutputDir("exports");
            Directory.CreateDirectory(outputDir);
            var filePath = Path.Combine(outputDir, $"{SelectedArtifact.Id}_mindmap_{DateTime.Now:yyyyMMdd_HHmmss}.svg");
            var svg = GenerateMindMapSvg();
            await File.WriteAllTextAsync(filePath, svg);
            try { Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true }); } catch { }
        }
        catch { }
    }

    private string GenerateMindMapSvg()
    {
        if (ArtifactGraphNodes.Count == 0) return "";
        var minX = ArtifactGraphNodes.Min(n => n.X) - 50;
        var maxX = ArtifactGraphNodes.Max(n => n.X) + 50;
        var minY = ArtifactGraphNodes.Min(n => n.Y) - 50;
        var maxY = ArtifactGraphNodes.Max(n => n.Y) + 50;
        var width = maxX - minX;
        var height = maxY - minY;
        var svg = new System.Text.StringBuilder();
        svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"{minX} {minY} {width} {height}\">");
        svg.AppendLine("<defs><style>.node { fill: #222; stroke: #00FF7F; stroke-width: 2; } .edge { stroke: #00FF7F; stroke-width: 1; fill: none; } .label { fill: #00FF7F; font-family: Consolas, monospace; font-size: 10px; } .edge-label { fill: #ccc; font-family: Consolas, monospace; font-size: 9px; }</style></defs>");
        foreach (var edge in ArtifactGraphEdges)
        {
            var fromNode = ArtifactGraphNodes.FirstOrDefault(n => n.Id == edge.FromId);
            var toNode = ArtifactGraphNodes.FirstOrDefault(n => n.Id == edge.ToId);
            if (fromNode == null || toNode == null) continue;
            svg.AppendLine($"<line class=\"edge\" x1=\"{fromNode.X}\" y1=\"{fromNode.Y}\" x2=\"{toNode.X}\" y2=\"{toNode.Y}\"/>");
        }
        foreach (var node in ArtifactGraphNodes)
        {
            svg.AppendLine($"<circle class=\"node\" cx=\"{node.X}\" cy=\"{node.Y}\" r=\"10\"/>");
            svg.AppendLine($"<text class=\"label\" x=\"{node.X}\" y=\"{node.Y + 4}\" text-anchor=\"middle\">{System.Security.SecurityElement.Escape(node.Label)}</text>");
        }
        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    partial void OnSelectedArtifactChanged(CortexArtifact? value)
    {
        OnPropertyChanged(nameof(HasSelectedArtifact));
        OnPropertyChanged(nameof(HasNoSelectedArtifact));
        OnPropertyChanged(nameof(SelectedArtifactVisualPath));
        OnPropertyChanged(nameof(IsGraphArtifact));
        OnPropertyChanged(nameof(IsMarkdownArtifact));
        OnPropertyChanged(nameof(IsAudioArtifact));
        OnPropertyChanged(nameof(IsSlideArtifact));
        OnPropertyChanged(nameof(IsVideoArtifact));
        
        RefreshArtifactGraph();
        _ = RefreshArtifactSlidesAsync();
        RefreshArtifactAudioTurns();
        RefreshArtifactVideoSlides();
        RefreshArtifactQuiz();
        RefreshArtifactFlashCards();
        RefreshArtifactDataTable();

        // Video Player Logic
        if (value?.Type == ArtifactType.VideoOverview && !string.IsNullOrWhiteSpace(value.FilePath) && File.Exists(value.FilePath))
        {
            if (_libVlc == null)
            {
                LibVLCSharp.Shared.Core.Initialize();
                _libVlc = new LibVLC();
            }
            if (_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer(_libVlc);
            }

            using var media = new Media(_libVlc!, value.FilePath, FromType.FromPath);
            MediaPlayer!.Play(media);
        }
        else
        {
            MediaPlayer?.Stop();
        }
        
        // Audio Player Logic
        if (value?.Type == ArtifactType.AudioOverview && !string.IsNullOrWhiteSpace(value.FilePath) && File.Exists(value.FilePath))
        {
            LoadAudioOverviewAudio(value.FilePath);
        }
        else
        {
            StopAudioOverviewAudio();
        }
    }

    [RelayCommand]
    private void RefreshArtifactGraph()
    {
        ArtifactGraphNodes.Clear();
        ArtifactGraphEdges.Clear();

        if (SelectedArtifact == null || string.IsNullOrWhiteSpace(SelectedArtifact.Content)) return;
        if (SelectedArtifact.Type != ArtifactType.MindMap && SelectedArtifact.Type != ArtifactType.Infographic) return;

        var content = SelectedArtifact.Content;
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var nodes = new Dictionary<string, GraphNode>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("graph ", StringComparison.OrdinalIgnoreCase)) continue;
            if (trimmed.StartsWith("style ", StringComparison.OrdinalIgnoreCase)) continue;

            var nodeMatches = Regex.Matches(trimmed, @"(\w+)\s*[\[\({\]\)}]+([^\\\]\)\}]+)[\\\]\)\}");
            foreach (Match m in nodeMatches)
            {
                var id = m.Groups[1].Value;
                var label = m.Groups[2].Value;
                if (!nodes.ContainsKey(id))
                {
                    var node = new GraphNode
                    {
                        Id = id,
                        Label = label,
                        Type = "Node",
                        X = 400 + Random.Shared.Next(-200, 200),
                        Y = 300 + Random.Shared.Next(-150, 150)
                    };
                    nodes[id] = node;
                    ArtifactGraphNodes.Add(node);
                }
            }

            var edgePatterns = new[]
            {
                (@"(\w+)\s*--[->]+\|([^\|]+)\|\s*(\w+)", true),
                (@"(\w+)\s*--\s*([^-]+?)\s*-->\s*(\w+)", true),
                (@"(\w+)\s*-->\s*(\w+)", false),
                (@"(\w+)\s*---\s*(\w+)", false),
                (@"(\w+)\s*--[->]+\s*(\w+)", false),
                (@"(\w+)\s*-\\.->\s*(\w+)", false),
                (@"(\w+)\s*==>\s*(\w+)", false),
                (@"(\w+)\s*==\s*(\w+)", false),
            };
            
            Match? edgeMatch = null;
            string? fromId = null;
            string? toId = null;
            string? edgeLabel = null;
            bool hasLabel = false;
            
            foreach (var (pattern, hasLabelPattern) in edgePatterns)
            {
                edgeMatch = Regex.Match(trimmed, pattern);
                if (edgeMatch.Success)
                {
                    fromId = edgeMatch.Groups[1].Value;
                    if (hasLabelPattern && edgeMatch.Groups.Count > 3)
                    {
                        toId = edgeMatch.Groups[3].Value;
                        edgeLabel = edgeMatch.Groups[2].Value.Trim();
                        hasLabel = true;
                    }
                    else
                    {
                        toId = edgeMatch.Groups[2].Value;
                        hasLabel = false;
                    }
                    break;
                }
            }
            
            if (edgeMatch != null && !string.IsNullOrWhiteSpace(fromId) && !string.IsNullOrWhiteSpace(toId))
            {
                if (!nodes.ContainsKey(fromId))
                {
                    var node = new GraphNode { Id = fromId, Label = fromId, Type = "Node", X = 400 + Random.Shared.Next(-200, 200), Y = 300 + Random.Shared.Next(-150, 150) };
                    nodes[fromId] = node;
                    ArtifactGraphNodes.Add(node);
                }
                if (!nodes.ContainsKey(toId))
                {
                    var node = new GraphNode { Id = toId, Label = toId, Type = "Node", X = 400 + Random.Shared.Next(-200, 200), Y = 300 + Random.Shared.Next(-150, 150) };
                    nodes[toId] = node;
                    ArtifactGraphNodes.Add(node);
                }

                ArtifactGraphEdges.Add(new GraphEdge 
                { 
                    FromId = fromId, 
                    ToId = toId,
                    Label = hasLabel ? edgeLabel : null
                });
            }
        }
        
        ApplyForceLayout();
    }

    private void ApplyForceLayout()
    {
        if (ArtifactGraphNodes.Count == 0) return;

        var centerX = 500.0;
        var centerY = 300.0;
        var radius = Math.Min(300.0, ArtifactGraphNodes.Count * 20.0);
        var angleStep = 2.0 * Math.PI / ArtifactGraphNodes.Count;
        var index = 0;
        foreach (var node in ArtifactGraphNodes)
        {
            if (node.X == 0 && node.Y == 0)
            {
                var angle = index * angleStep;
                node.X = centerX + radius * Math.Cos(angle);
                node.Y = centerY + radius * Math.Sin(angle);
            }
            index++;
        }

        const int iterations = 150;
        const double initialRepulse = 8000.0;
        const double initialAttract = 0.08;
        const double idealLength = 150.0;
        const double coolingFactor = 0.95;

        double repulse = initialRepulse;
        double attract = initialAttract;

        for (int i = 0; i < iterations; i++)
        {
            repulse *= coolingFactor;
            attract *= coolingFactor;

            foreach (var n1 in ArtifactGraphNodes)
            {
                double fx = 0, fy = 0;
                foreach (var n2 in ArtifactGraphNodes)
                {
                    if (n1 == n2) continue;
                    double dx = n1.X - n2.X;
                    double dy = n1.Y - n2.Y;
                    double d2 = dx * dx + dy * dy;
                    if (d2 < 0.01) d2 = 0.01;
                    double d = Math.Sqrt(d2);
                    double force = repulse / d2;
                    fx += (dx / d) * force;
                    fy += (dy / d) * force;
                }
                n1.X += fx * 0.1;
                n1.Y += fy * 0.1;
            }

            foreach (var edge in ArtifactGraphEdges)
            {
                var n1 = ArtifactGraphNodes.FirstOrDefault(n => n.Id == edge.FromId);
                var n2 = ArtifactGraphNodes.FirstOrDefault(n => n.Id == edge.ToId);
                if (n1 == null || n2 == null) continue;

                double dx = n2.X - n1.X;
                double dy = n2.Y - n1.Y;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d < 0.01) d = 0.01;
                
                double force = attract * (d - idealLength);
                double fx = (dx / d) * force;
                double fy = (dy / d) * force;

                n1.X += fx * 0.1;
                n1.Y += fy * 0.1;
                n2.X -= fx * 0.1;
                n2.Y -= fy * 0.1;
            }

            foreach (var node in ArtifactGraphNodes)
            {
                node.X = Math.Clamp(node.X, 50, 950);
                node.Y = Math.Clamp(node.Y, 50, 550);
            }
        }
        
        if (ArtifactGraphNodes.Count > 0)
        {
            double avgX = ArtifactGraphNodes.Average(n => n.X);
            double avgY = ArtifactGraphNodes.Average(n => n.Y);
            double offsetX = centerX - avgX;
            double offsetY = centerY - avgY;
            
            foreach (var node in ArtifactGraphNodes)
            {
                node.X += offsetX;
                node.Y += offsetY;
            }
        }
    }

    private async Task RefreshArtifactSlidesAsync()
    {
        SelectedArtifactSlides.Clear();
        if (SelectedArtifact?.Type == ArtifactType.SlideDeck && !string.IsNullOrWhiteSpace(SelectedArtifact.Content))
        {
            try
            {
                using var doc = JsonDocument.Parse(SelectedArtifact.Content);
                List<SlideItem> slides;
                
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    slides = JsonSerializer.Deserialize<List<SlideItem>>(SelectedArtifact.Content) ?? new List<SlideItem>();
                }
                else if (doc.RootElement.TryGetProperty("slides", out var slidesEl) && slidesEl.ValueKind == JsonValueKind.Array)
                {
                    slides = JsonSerializer.Deserialize<List<SlideItem>>(slidesEl.GetRawText()) ?? new List<SlideItem>();
                }
                else
                {
                    slides = new List<SlideItem>();
                }
                
                if (slides != null && slides.Count > 0)
                {
                    var artifactDir = !string.IsNullOrWhiteSpace(SelectedArtifact.FilePath) 
                        ? Path.GetDirectoryName(SelectedArtifact.FilePath)
                        : Path.Combine(AppContext.BaseDirectory, "data", "artifacts", SelectedArtifact.Id);
                    
                    if (!string.IsNullOrEmpty(artifactDir))
                    {
                        Directory.CreateDirectory(artifactDir);
                    }
                    
                    // We already have _imageService in CortexViewModel
                    bool hasImageService = _imageService.IsConfigured();
                    
                    for (int i = 0; i < slides.Count; i++)
                    {
                        var slide = slides[i];
                        
                        if (string.IsNullOrWhiteSpace(slide.visual_path))
                        {
                            var imageFileName = $"slide_{i:000}.png";
                            var imagePath = !string.IsNullOrEmpty(artifactDir) 
                                ? Path.Combine(artifactDir, imageFileName)
                                : null;
                            
                            if (imagePath != null && File.Exists(imagePath))
                            {
                                slide.visual_path = imagePath;
                            }
                            else if (hasImageService && imagePath != null)
                            {
                                try
                                {
                                    var prompt = $"Create a professional slide image for a presentation. Title: {slide.title}. Content: {TruncateText(slide.content, 200)}. Use modern design with clear typography and visual elements.";
                                    var generatedPath = await _imageService.GenerateImageAsync(
                                        $"slide_{SelectedArtifact.Id}_{i}",
                                        prompt,
                                        CancellationToken.None
                                    );
                                    
                                    if (!string.IsNullOrWhiteSpace(generatedPath) && File.Exists(generatedPath))
                                    {
                                        File.Copy(generatedPath, imagePath, true);
                                        slide.visual_path = imagePath;
                                    }
                                }
                                catch {{ }}
                            }
                        }
                        
                        SelectedArtifactSlides.Add(slide);
                    }
                    
                    if (CurrentSlideIndex < 0 && SelectedArtifactSlides.Count > 0)
                    {
                        CurrentSlideIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CortexViewModel] Failed to parse slides: {ex.Message}");
                CrashLog.Write("CortexViewModel.RefreshArtifactSlides", ex);
            }
        }
    }
    
    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }

    private void RefreshArtifactVideoSlides()
    {
        SelectedArtifactVideoSlides.Clear();
        if ((SelectedArtifact?.Type == ArtifactType.VideoOverview || SelectedArtifact?.Type == ArtifactType.Infographic) && !string.IsNullOrWhiteSpace(SelectedArtifact.Content))
        {
            try
            {
                using var doc = JsonDocument.Parse(SelectedArtifact.Content);
                if (doc.RootElement.TryGetProperty("slides", out var slidesEl) && slidesEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var s in slidesEl.EnumerateArray())
                    {
                        SelectedArtifactVideoSlides.Add(new VideoSlide
                        {
                            title = s.GetProperty("title").GetString() ?? "",
                            content = s.GetProperty("content").GetString() ?? "",
                            visual_path = s.TryGetProperty("visual_path", out var v) ? v.GetString() : null
                        });
                    }
                }
                
                if (SelectedArtifact.Type == ArtifactType.Infographic && CurrentInfographicSlideIndex < 0 && SelectedArtifactVideoSlides.Count > 0)
                {
                    CurrentInfographicSlideIndex = 0;
                }
            }
            catch {{ }}
        }
        else if (SelectedArtifact?.Type != ArtifactType.VideoOverview && SelectedArtifact?.Type != ArtifactType.Infographic)
        {
            CurrentInfographicSlideIndex = -1;
        }
    }

    private void RefreshArtifactAudioTurns()
    {
        SelectedArtifactAudioTurns.Clear();
        if (SelectedArtifact?.Type == ArtifactType.AudioOverview && !string.IsNullOrWhiteSpace(SelectedArtifact.Content))
        {
            try
            {
                var turns = JsonSerializer.Deserialize<List<AudioTurn>>(SelectedArtifact.Content);
                if (turns != null)
                {
                    var artifactDir = !string.IsNullOrWhiteSpace(SelectedArtifact.FilePath) 
                        ? Path.GetDirectoryName(SelectedArtifact.FilePath)
                        : Path.Combine(AppContext.BaseDirectory, "data", "artifacts", SelectedArtifact.Id);
                    
                    for (int i = 0; i < turns.Count; i++)
                    {
                        var turn = turns[i];
                        var audioFileName = $"{turn.speaker}_{i:000}.mp3";
                        var audioPath = Path.Combine(artifactDir ?? "", audioFileName);
                        
                        if (File.Exists(audioPath))
                        {
                            turn.audioPath = audioPath;
                            try
                            {
                                turn.duration = GetAudioDuration(audioPath);
                            }
                            catch {{ turn.duration = 0; }}
                        }
                        SelectedArtifactAudioTurns.Add(turn);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CortexViewModel] Failed to parse audio turns: {ex.Message}");
            }
        }
    }
    
    private double GetAudioDuration(string audioPath)
    {
        try
        {
            var ffprobePath = Path.Combine(AppContext.BaseDirectory, "tools", "ffprobe.exe");
            if (!File.Exists(ffprobePath))
            {
                ffprobePath = "ffprobe";
            }
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{audioPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            if (double.TryParse(output.Trim(), out var duration))
            {
                return duration;
            }
        }
        catch {{ }}
        return 0;
    }

    private void LoadAudioOverviewAudio(string audioPath)
    {
        if (SelectedArtifactAudioTurns.Any(t => !string.IsNullOrWhiteSpace(t.audioPath)))
        {
            return;
        }
        
        try
        {
            if (_audioLibVlc == null)
            {
                LibVLCSharp.Shared.Core.Initialize();
                _audioLibVlc = new LibVLC();
            }
            if (_audioMediaPlayer == null)
            {
                _audioMediaPlayer = new MediaPlayer(_audioLibVlc);
                _audioMediaPlayer.EndReached += (s, e) => 
                {
                    AudioOverviewStatus = "Playback finished";
                    _currentAudioMedia?.Dispose();
                    _currentAudioMedia = null;
                };
            }
            _currentAudioMedia?.Dispose();
            _currentAudioMedia = new Media(_audioLibVlc, audioPath, FromType.FromPath);
            AudioOverviewStatus = "Ready to play";
        }
        catch (Exception ex)
        { 
            CrashLog.Write("LoadAudioOverviewAudio", ex);
            AudioOverviewStatus = "Audio playback not available";
        }
    }
    
    private void StopAudioOverviewAudio()
    {
        _audioPlaybackCts?.Cancel();
        _isPlayingAudioSequence = false;
        _currentAudioTurnIndex = -1;
        
        foreach (var turn in SelectedArtifactAudioTurns)
        {
            turn.IsPlaying = false;
        }
        
        _audioMediaPlayer?.Stop();
        _currentAudioMedia?.Dispose();
        _currentAudioMedia = null;
        AudioOverviewStatus = null;
    }
    
    [RelayCommand]
    private async Task PlayAudioOverview()
    {
        if (SelectedArtifactAudioTurns.Count == 0)
        {
            AudioOverviewStatus = "No audio turns available";
            return;
        }
        
        var turnsWithAudio = SelectedArtifactAudioTurns.Where(t => !string.IsNullOrWhiteSpace(t.audioPath) && File.Exists(t.audioPath)).ToList();
        if (turnsWithAudio.Count == 0)
        {
            AudioOverviewStatus = "No audio files found for turns";
            return;
        }
        
        if (_isPlayingAudioSequence)
        {
            return;
        }
        
        _audioPlaybackCts?.Cancel();
        _audioPlaybackCts = new CancellationTokenSource();
        _isPlayingAudioSequence = true;
        
        try
        {
            if (_audioLibVlc == null)
            {
                LibVLCSharp.Shared.Core.Initialize();
                _audioLibVlc = new LibVLC();
            }
            if (_audioMediaPlayer == null)
            {
                _audioMediaPlayer = new MediaPlayer(_audioLibVlc);
            }
            
            for (int i = 0; i < SelectedArtifactAudioTurns.Count && !_audioPlaybackCts.Token.IsCancellationRequested; i++)
            {
                var turn = SelectedArtifactAudioTurns[i];
                if (string.IsNullOrWhiteSpace(turn.audioPath) || !File.Exists(turn.audioPath))
                    continue;
                
                _currentAudioTurnIndex = i;
                turn.IsPlaying = true;
                AudioOverviewStatus = $"Playing {turn.speaker}...";
                OnPropertyChanged(nameof(SelectedArtifactAudioTurns));
                
                await PlayAudioTurnAsync(turn.audioPath, turn.duration);
                
                turn.IsPlaying = false;
                
                if (_audioPlaybackCts.Token.IsCancellationRequested)
                    break;
            }
            
            _currentAudioTurnIndex = -1;
            _isPlayingAudioSequence = false;
            AudioOverviewStatus = "Playback finished";
        }
        catch (OperationCanceledException)
        {
            AudioOverviewStatus = "Playback stopped";
        }
        catch (Exception ex)
        {
            AudioOverviewStatus = $"Playback error: {ex.Message}";
        }
        finally
        {
            foreach (var turn in SelectedArtifactAudioTurns)
            {
                turn.IsPlaying = false;
            }
            _isPlayingAudioSequence = false;
        }
    }
    
    [RelayCommand]
    private async Task PlayAudioTurnCommand(AudioTurn? turn)
    {
        if (turn != null && !string.IsNullOrWhiteSpace(turn.audioPath) && File.Exists(turn.audioPath))
        {
            await PlayAudioTurnAsync(turn.audioPath, turn.duration);
        }
    }
    
    [RelayCommand]
    private async Task PreviewAudioTurnAsync(AudioTurn? turn)
    {
        if (turn == null || string.IsNullOrWhiteSpace(turn.text)) return;
        
        try
        {
            Status = "Generating audio preview...";
            
            var previewText = turn.text.Length > 200 
                ? turn.text.Substring(0, 200) + "..." 
                : turn.text;
            
            var previewId = $"preview_{Guid.NewGuid()}";
            var previewPath = Path.Combine(Serenity.Cortex.Core.Helpers.PathHelper.GetOutputDir("audio"), $"{previewId}.mp3");
            
            var ttsProvider = CortexConfig.Get("TTS_PROVIDER", "Edge") ?? "Edge";
            if (ttsProvider.Equals("Edge", StringComparison.OrdinalIgnoreCase))
            {
                // Edge TTS implementation stub or assume it's available?
                // MainViewModel used GenerateTtsWithAriaAsync which is likely in MainViewModel
                // I need to port GenerateTtsWithAriaAsync or use a service
                
                // For now, let's use a simpler approach or skip Edge TTS if method missing
                // Or better, assume we can access a shared helper
                Status = "Edge TTS not fully ported yet."; 
            }
            else if (ttsProvider.Equals("ElevenLabs", StringComparison.OrdinalIgnoreCase) && _tts.IsConfigured())
            {
                string? voiceId = turn.speaker?.Equals("Elias", StringComparison.OrdinalIgnoreCase) == true
                    ? CortexConfig.Get("VOICE_ID_ELIAS") ?? "8Ly6fkjN3w7ltsU7MwB5"
                    : CortexConfig.Get("VOICE_ID_INARA") ?? "1Ct4QEnFTKYIueJJOBtL";
                
                await _tts.SynthesizeToMp3Async(previewId, previewText, voiceId, CancellationToken.None);
            }
            
            if (File.Exists(previewPath))
            {
                await PlayAudioTurnAsync(previewPath, 0);
                Status = "Audio preview played";
                
                _ = Task.Delay(5000).ContinueWith(_ => 
                {
                    try { if (File.Exists(previewPath)) File.Delete(previewPath); } 
                    catch {{ }}
                });
            }
        }
        catch (Exception ex)
        {
            Status = $"Preview failed: {ex.Message}";
            CrashLog.Write("PreviewAudioTurnAsync", ex);
        }
    }

    private async Task PlayAudioTurnAsync(string audioPath, double duration)
    {
        if (_audioMediaPlayer == null || _audioLibVlc == null) return;
        
        var tcs = new TaskCompletionSource<bool>();
        
        void OnEndReached(object? sender, EventArgs e)
        {
            _audioMediaPlayer!.EndReached -= OnEndReached;
            tcs.TrySetResult(true);
        }
        
        _audioMediaPlayer.EndReached += OnEndReached;
        
        try
        {
            _currentAudioMedia?.Dispose();
            _currentAudioMedia = new Media(_audioLibVlc, audioPath, FromType.FromPath);
            _audioMediaPlayer.Play(_currentAudioMedia);
            
            var timeout = duration > 0 ? TimeSpan.FromSeconds(duration + 1) : TimeSpan.FromSeconds(30);
            await Task.WhenAny(tcs.Task, Task.Delay(timeout, _audioPlaybackCts!.Token));
        }
        finally
        {
            _audioMediaPlayer.EndReached -= OnEndReached;
        }
    }
    
    [RelayCommand]
    private void PauseAudioOverview()
    {
        _audioMediaPlayer?.Pause();
        AudioOverviewStatus = "Paused";
    }
    
    [RelayCommand]
    private void StopAudioOverview()
    {
        StopAudioOverviewAudio();
    }
    
    [RelayCommand]
    private async Task ReplayAudioOverview()
    {
        StopAudioOverviewAudio();
        await Task.Delay(100);
        await PlayAudioOverview();
    }
    
    [RelayCommand]
    private void ToggleFlashCard(FlashCard? card)
    {
        if (card != null)
        {
            card.IsFlipped = !card.IsFlipped;
        }
    }

    private void RefreshArtifactQuiz()
    {
        SelectedArtifactQuizQuestions.Clear();
        if (SelectedArtifact?.Type == ArtifactType.Quiz && !string.IsNullOrWhiteSpace(SelectedArtifact.Content))
        {
            try
            {
                var questions = JsonSerializer.Deserialize<List<QuizQuestion>>(SelectedArtifact.Content);
                if (questions != null)
                {
                    foreach (var q in questions) SelectedArtifactQuizQuestions.Add(q);
                }
            }
            catch {{ }}
        }
    }

    private void RefreshArtifactFlashCards()
    {
        SelectedArtifactFlashCards.Clear();
        if (SelectedArtifact?.Type == ArtifactType.Flashcards && !string.IsNullOrWhiteSpace(SelectedArtifact.Content))
        {
            try
            {
                var cards = JsonSerializer.Deserialize<List<FlashCard>>(SelectedArtifact.Content);
                if (cards != null)
                {
                    foreach (var c in cards) SelectedArtifactFlashCards.Add(c);
                }
            }
            catch {{ }}
        }
    }

    private void RefreshArtifactDataTable()
    {
        SelectedArtifactDataTableRows.Clear();
        SelectedArtifactDataTableColumns.Clear();
        if (SelectedArtifact?.Type == ArtifactType.DataTable && !string.IsNullOrWhiteSpace(SelectedArtifact.Content))
        {
            try
            {
                using var doc = JsonDocument.Parse(SelectedArtifact.Content);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("columns", out var cols) && cols.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var col in cols.EnumerateArray())
                        {
                            SelectedArtifactDataTableColumns.Add(col.GetString() ?? "");
                        }
                    }
                    if (doc.RootElement.TryGetProperty("rows", out var rows) && rows.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var row in rows.EnumerateArray())
                        {
                            if (row.ValueKind == JsonValueKind.Array)
                            {
                                var dataRow = new DataTableRow();
                                foreach (var cell in row.EnumerateArray())
                                {
                                    dataRow.Cells.Add(cell.GetString() ?? "");
                                }
                                SelectedArtifactDataTableRows.Add(dataRow);
                            }
                        }
                    }
                }
            }
            catch {{ }}
        }
    }

    // ---- Media Generation Monitoring ----

    private System.Timers.Timer? _mediaCheckTimer;
    private Dictionary<string, bool> _artifactsGeneratingMedia = new();
    private System.Timers.Timer? _scribeBridgeTimer;

    public bool IsArtifactGeneratingMedia(string? artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId)) return false;
        return _artifactsGeneratingMedia.TryGetValue(artifactId, out var generating) && generating;
    }

    private bool RequiresMediaGeneration(ArtifactType type)
    {
        return type == ArtifactType.VideoOverview 
            || type == ArtifactType.Infographic 
            || type == ArtifactType.AudioOverview
            || type == ArtifactType.MindMap;
    }
    
    private bool IsPlayableMedia(ArtifactType type)
    {
        return type == ArtifactType.VideoOverview 
            || type == ArtifactType.Infographic 
            || type == ArtifactType.AudioOverview;
    }

    private void StartMediaGenerationMonitoring(CortexArtifact artifact)
    {
        _mediaCheckTimer?.Stop();
        _mediaCheckTimer?.Dispose();
        _mediaCheckTimer = null;
        
        _scribeBridgeTimer?.Stop();
        _scribeBridgeTimer?.Dispose();
        _scribeBridgeTimer = null;
        
        _mediaCheckTimer = new System.Timers.Timer(500); // Check every 500ms
        _mediaCheckTimer.Elapsed += (s, e) =>
        {
            if (artifact == null) return;
            
            bool hasMedia = false;
            if (IsPlayableMedia(artifact.Type))
            {
                hasMedia = !string.IsNullOrWhiteSpace(artifact.FilePath) && File.Exists(artifact.FilePath);
            }
            else if (artifact.Type == ArtifactType.MindMap)
            {
                hasMedia = !string.IsNullOrWhiteSpace(artifact.VisualPath) && File.Exists(artifact.VisualPath);
            }
            
            if (hasMedia)
            {
                _mediaCheckTimer?.Stop();
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    IsGeneratingMedia = false;
                    _artifactsGeneratingMedia.Remove(artifact.Id);
                    OnPropertyChanged(nameof(Artifacts)); // Notify list to update
                    OnPropertyChanged(nameof(SelectedArtifact));
                });
            }
        };
        _mediaCheckTimer.Start();
        
        // Stop after 2 minutes to avoid infinite checking
        var stopTimer = new System.Timers.Timer(120000);
        stopTimer.Elapsed += (s, e) =>
        {
            stopTimer.Stop();
            _mediaCheckTimer?.Stop();
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsGeneratingMedia = false;
                _artifactsGeneratingMedia.Remove(artifact.Id);
                OnPropertyChanged(nameof(Artifacts)); // Notify list to update
            });
        };
        stopTimer.Start();
    }
}
