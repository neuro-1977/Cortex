using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Serenity.ViewModels;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Serenity.Views;

public partial class CortexView : UserControl
{
    private DispatcherTimer? _waveformTimer;
    private int _micAudioLevel = 0;
    private int _ttsAudioLevel = 0;
    private bool _isListening = false;
    private bool _isSpeaking = false;
    private bool _isMuted = false;

    public CortexView()
    {
        InitializeComponent();
        _waveformTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Render, (s, e) => UpdateWaveform());
        _waveformTimer.Start();

        var graphTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Render, (s, e) => UpdateArtifactGraphEdges());
        graphTimer.Start();

        // Setup mouse wheel zoom and drag for Artifact Graph
        var artifactCanvas = this.FindControl<Canvas>("ArtifactGraphCanvas");
        if (artifactCanvas != null)
        {
            var parentPanel = artifactCanvas.Parent as Panel;
            if (parentPanel != null)
            {
                parentPanel.PointerWheelChanged += (s, e) =>
                {
                    if (DataContext is CortexViewModel vm && vm.IsGraphArtifact)
                    {
                        // Note: Zoom commands are internal or relay commands, check if exposed
                        // Since they were RelayCommands in partial class, they should be available if public
                        // Assuming ZoomInArtifactGraphCommand and ZoomOutArtifactGraphCommand exist in CortexViewModel
                        // If not, we might need to expose them or use the properties directly
                        
                        // For now, let's assume they are available or we can modify properties directly
                        if (e.Delta.Y > 0) vm.ArtifactGraphZoom *= 1.1; // Simple fallback if command not found
                        else vm.ArtifactGraphZoom /= 1.1;
                        e.Handled = true;
                    }
                };

                bool isDragging = false;
                Point lastPos = new Point();

                parentPanel.PointerPressed += (s, e) =>
                {
                    if (DataContext is CortexViewModel vm && vm.IsGraphArtifact && e.GetCurrentPoint(parentPanel).Properties.IsLeftButtonPressed)
                    {
                        isDragging = true;
                        lastPos = e.GetPosition(parentPanel);
                        e.Pointer.Capture(parentPanel);
                        e.Handled = true;
                    }
                };

                parentPanel.PointerMoved += (s, e) =>
                {
                    if (isDragging && DataContext is CortexViewModel vm)
                    {
                        var currentPos = e.GetPosition(parentPanel);
                        var delta = currentPos - lastPos;
                        vm.ArtifactGraphOffsetX += delta.X;
                        vm.ArtifactGraphOffsetY += delta.Y;
                        lastPos = currentPos;
                        e.Handled = true;
                    }
                };

                parentPanel.PointerReleased += (s, e) =>
                {
                    if (isDragging)
                    {
                        isDragging = false;
                        e.Pointer.Capture(null);
                        e.Handled = true;
                    }
                };
            }
        }
    }

    private void UpdateArtifactGraphEdges()
    {
        var canvas = this.FindControl<Canvas>("ArtifactGraphCanvas");
        if (canvas == null || DataContext is not CortexViewModel vm) return;

        // Remove existing lines
        var pathsToRemove = canvas.Children.OfType<Avalonia.Controls.Shapes.Path>().ToList(); var linesToRemove = canvas.Children.OfType<Line>().ToList();
        foreach (var path in pathsToRemove) canvas.Children.Remove(path); foreach (var line in linesToRemove) canvas.Children.Remove(line);

        foreach (var edge in vm.ArtifactGraphEdges)
        {
            var fromNode = vm.ArtifactGraphNodes.FirstOrDefault(n => n.Id == edge.FromId);
            var toNode = vm.ArtifactGraphNodes.FirstOrDefault(n => n.Id == edge.ToId);

            if (fromNode != null && toNode != null)
            {
                                // Create smooth Bezier curve for organic flow (like NotebookLM)
                var startX = fromNode.X + 60;
                var startY = fromNode.Y + 15;
                var endX = toNode.X + 60;
                var endY = toNode.Y + 15;
                
                var dx = endX - startX;
                var dy = endY - startY;
                
                var cp1X = startX + dx * 0.4;
                var cp1Y = startY;
                var cp2X = startX + dx * 0.6;
                var cp2Y = endY;

                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure
                {
                    StartPoint = new Avalonia.Point(startX, startY),
                    IsClosed = false
                };

                pathFigure.Segments?.Add(new BezierSegment
                {
                    Point1 = new Avalonia.Point(cp1X, cp1Y),
                    Point2 = new Avalonia.Point(cp2X, cp2Y),
                    Point3 = new Avalonia.Point(endX, endY)
                });

                pathGeometry.Figures?.Add(pathFigure);

                var path = new Avalonia.Controls.Shapes.Path {
                    Data = pathGeometry,
                    Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 127)) { Opacity = 0.4 },
                    StrokeThickness = 2,
                    StrokeLineCap = PenLineCap.Round
                };
                canvas.Children.Insert(0, path);
                
                // Add edge label if present
                if (!string.IsNullOrWhiteSpace(edge.Label))
                {
                    var midX = (startX + endX) / 2;
                    var midY = (startY + endY) / 2;
                    
                    var labelText = new TextBlock
                    {
                        Text = edge.Label,
                        Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)) { Opacity = 0.8 },
                        FontSize = 9,
                        FontFamily = "Consolas, monospace",
                        Background = new SolidColorBrush(Color.FromRgb(10, 10, 10)) { Opacity = 0.9 },
                        Padding = new Thickness(4, 2),
                        [Canvas.LeftProperty] = midX - 30,
                        [Canvas.TopProperty] = midY - 10
                    };
                    canvas.Children.Add(labelText);
                }
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

        private void UpdateWaveform()
    {
        var leftCanvas = FindCanvasByGridColumn(0);
        UpdateWaveformCanvas(leftCanvas);
    }
    
    
    private Canvas? FindCanvasByGridColumn(int column)
    {
        var mainGrid = this.Content as Grid;
        if (mainGrid == null) return null;
        if (mainGrid == null) return null; var chatPillGrid = mainGrid.Children.OfType<Grid>().Where(g => Grid.GetRow(g) == 1).FirstOrDefault();
        if (chatPillGrid == null) return null;
        return chatPillGrid.Children.OfType<Canvas>().Where(c => Grid.GetColumn(c) == column).FirstOrDefault();
    }

    private void UpdateWaveformCanvas(Canvas? canvas)
    {
        if (canvas == null) return;

        canvas.Children.Clear();
        double canvasHeight = canvas.Bounds.Height;
        if (canvasHeight <= 0) canvasHeight = 40;
        double canvasWidth = canvas.Bounds.Width;
        if (canvasWidth <= 0) canvasWidth = 60;
        
        // Draw 3 vertical bars (short-long-short) like Lordicon waveform
        double barWidth = 3;
        double spacing = 4;
        double startX = (canvasWidth - (barWidth * 3 + spacing * 2)) / 2;
        double baseY = canvasHeight;
        
        IBrush barColor = Brushes.Gray; // Default gray
        if (_isListening || (_isSpeaking && !_isMuted))
        {
            barColor = Brushes.SpringGreen; // Green when listening/speaking
        }
        
        // Calculate bar heights based on audio level
        double level = _isListening ? (_micAudioLevel / 100.0) : (_isSpeaking ? (_ttsAudioLevel / 100.0) : 0.3);
        if (level < 0.3) level = 0.3; // Minimum height for visibility
        
        // Bar 1: 30% height (short)
        double bar1Height = canvasHeight * 0.3 * level;
        canvas.Children.Add(new Rectangle
        {
            Fill = barColor,
            Width = barWidth,
            Height = bar1Height,
            [Canvas.LeftProperty] = startX,
            [Canvas.TopProperty] = baseY - bar1Height
        });
        
        // Bar 2: 60% height (center, tallest)
        double bar2Height = canvasHeight * 0.6 * level;
        canvas.Children.Add(new Rectangle
        {
            Fill = barColor,
            Width = barWidth,
            Height = bar2Height,
            [Canvas.LeftProperty] = startX + barWidth + spacing,
            [Canvas.TopProperty] = baseY - bar2Height
        });
        
        // Bar 3: 30% height (short)
        double bar3Height = canvasHeight * 0.3 * level;
        canvas.Children.Add(new Rectangle
        {
            Fill = barColor,
            Width = barWidth,
            Height = bar3Height,
            [Canvas.LeftProperty] = startX + (barWidth + spacing) * 2,
            [Canvas.TopProperty] = baseY - bar3Height
        });
    }

    private async void AddSource_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CortexViewModel vm) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Add sources",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new Avalonia.Platform.Storage.FilePickerFileType("Documents") { Patterns = new[] { "*.pdf", "*.txt", "*.md", "*.markdown" } },
                Avalonia.Platform.Storage.FilePickerFileTypes.All
            }
        });

        foreach (var f in files)
        {
            string? path = null;
            var pathProperty = f.GetType().GetProperty("Path");
            if (pathProperty != null)
            {
                var uri = pathProperty.GetValue(f) as Uri;
                if (uri != null)
                {
                    path = uri.LocalPath;
                }
            }
            
            // Explicitly block serenity.db and all .db files
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (path.Contains("serenity.db", StringComparison.OrdinalIgnoreCase) || 
                    path.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                {
                    vm.Status = "serenity.db is only viewable in Engine Room, not available as a Cortex source.";
                    continue;
                }
                
                await vm.AddSourceFromExternalAsync(path);
            }
        }
    }

    private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            if (DataContext is CortexViewModel vm)
            {
                await vm.SendChatMessageCommand.ExecuteAsync(null);
            }
        }
    }
    
    private void CloseSlideDeckModal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CortexViewModel vm)
        {
            vm.ShowSlideDeckModal = false;
        }
    }
    
    private void CloseBriefingDocModal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CortexViewModel vm)
        {
            vm.ShowBriefingDocModal = false;
        }
    }
    
    private void CloseQuizModal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CortexViewModel vm)
        {
            vm.ShowQuizModal = false;
        }
    }
    
    private void CloseFlashCardModal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CortexViewModel vm)
        {
            vm.ShowFlashCardModal = false;
        }
    }
    
    private void CloseInfographicModal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CortexViewModel vm)
        {
            vm.ShowInfographicModal = false;
        }
    }
    
    private void CloseDataTableModal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CortexViewModel vm)
        {
            vm.ShowDataTableModal = false;
        }
    }
    
    private void ChatOverlayBackground_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Close overlay when clicking background
        if (DataContext is CortexViewModel vm)
        {
            vm.ToggleChatOverlayCommand.Execute(null);
        }
        e.Handled = true;
    }
    
    private void ChatOverlayContent_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Prevent closing when clicking inside the overlay
        e.Handled = true;
    }
    
    private void CitationButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Serenity.Cortex.Core.Models.CortexCitation citation && DataContext is CortexViewModel vm)
        {
            // Use reflection to call the private RelayCommand method
            var method = typeof(CortexViewModel).GetMethod("OnCitationClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(vm, new object[] { citation });
        }
    }
}


