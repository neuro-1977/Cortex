using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Cortex.App.ViewModels;
using Cortex.App.Helpers;
using System;

namespace Cortex.App.Controls.Common;

public partial class ChatPill : UserControl
{
    private Border? _chatPillBorder;
    private Canvas? _waveformCanvas;
    private TextBox? _txtInput;
    private bool _isTtsSpeaking = false;
    private bool _isVoiceListening = false;
    private double _listeningPulseOpacity = 0.95;
    private bool _listeningPulseDirection = true;
    private Avalonia.Threading.DispatcherTimer? _borderAnimationTimer;
    private Avalonia.Threading.DispatcherTimer? _waveformTimer;
    
    // Waveform performance optimization
    private DateTime _lastWaveformUpdate = DateTime.MinValue;
    private const double AudioLevelChangeThreshold = 0.05;
    private const int MinUpdateIntervalMs = 33;

    public ChatPill()
    {
        InitializeComponent();
        Loaded += ChatPill_Loaded;
        Unloaded += ChatPill_Unloaded;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ChatPill_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _chatPillBorder = this.FindControl<Border>("ChatPillBorder");
        _waveformCanvas = this.FindControl<Canvas>("WaveformCanvas");
        _txtInput = this.FindControl<TextBox>("TxtInput");
        
        // Start waveform update timer
        if (_waveformCanvas != null)
        {
            _waveformTimer = new Avalonia.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(33), 
                Avalonia.Threading.DispatcherPriority.Render, 
                (s, e) => UpdateWaveform());
            _waveformTimer.Start();
        }
        
        // Monitor voice state changes
        if (DataContext is CortexViewModel vm)
        {
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CortexViewModel.IsTtsSpeaking))
                {
                    _isTtsSpeaking = vm.IsTtsSpeaking;
                    UpdateChatPillBorderAnimation();
                }
                else if (e.PropertyName == nameof(CortexViewModel.IsVoiceListening))
                {
                    _isVoiceListening = vm.IsVoiceListening && !vm.IsVoiceMuted;
                    UpdateChatPillBorderAnimation();
                }
                else if (e.PropertyName == nameof(CortexViewModel.IsVoiceMuted))
                {
                    _isVoiceListening = vm.IsVoiceListening && !vm.IsVoiceMuted;
                    UpdateChatPillBorderAnimation();
                }
            };
            
            // Start border animation timer
            _borderAnimationTimer = new Avalonia.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(33), 
                Avalonia.Threading.DispatcherPriority.Render, 
                (s, e) => UpdateBorderPulseAnimation());
            _borderAnimationTimer.Start();
        }
        
        // Wire up mic mute button
        var btnMicMute = this.FindControl<ToggleButton>("BtnMicMute");
        if (btnMicMute != null && DataContext is CortexViewModel vm2)
        {
            btnMicMute.Click += (s, e) =>
            {
                if (btnMicMute.IsChecked == true)
                    vm2.IsVoiceMuted = true;
                else
                    vm2.IsVoiceMuted = false;
            };
        }
    }

    private void ChatPill_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Stop timers
        _borderAnimationTimer?.Stop();
        _borderAnimationTimer = null;
        _waveformTimer?.Stop();
        _waveformTimer = null;
    }

    private void UpdateBorderPulseAnimation()
    {
        if (!_isVoiceListening || _isTtsSpeaking) return;
        
        // Pulse opacity between 0.95 and 1.0 when listening
        if (_listeningPulseDirection)
        {
            _listeningPulseOpacity += 0.02;
            if (_listeningPulseOpacity >= 1.0)
            {
                _listeningPulseOpacity = 1.0;
                _listeningPulseDirection = false;
            }
        }
        else
        {
            _listeningPulseOpacity -= 0.02;
            if (_listeningPulseOpacity <= 0.95)
            {
                _listeningPulseOpacity = 0.95;
                _listeningPulseDirection = true;
            }
        }
        
        UpdateChatPillBorderAnimation();
    }

    private void UpdateChatPillBorderAnimation()
    {
        if (_chatPillBorder == null) return;
        
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_isTtsSpeaking)
            {
                // Enhanced glow when speaking
                _chatPillBorder.Opacity = 1.0;
                _chatPillBorder.BoxShadow = new BoxShadows(
                    new BoxShadow { OffsetX = 0, OffsetY = 0, Blur = 50, Spread = 8, Color = Color.Parse("#00ffff60") },
                    new[] {
                        new BoxShadow { OffsetX = 0, OffsetY = 0, Blur = 100, Spread = 15, Color = Color.Parse("#ff00ff40") },
                        new BoxShadow { OffsetX = 0, OffsetY = 4, Blur = 20, Spread = 0, Color = Color.Parse("#00000080") }
                    }
                );
            }
            else if (_isVoiceListening)
            {
                // Pulse effect when listening
                _chatPillBorder.Opacity = _listeningPulseOpacity;
                _chatPillBorder.BoxShadow = new BoxShadows(
                    new BoxShadow { OffsetX = 0, OffsetY = 0, Blur = 35, Spread = 5, Color = Color.Parse("#00ffff50") },
                    new[] {
                        new BoxShadow { OffsetX = 0, OffsetY = 0, Blur = 70, Spread = 10, Color = Color.Parse("#ff00ff30") },
                        new BoxShadow { OffsetX = 0, OffsetY = 4, Blur = 20, Spread = 0, Color = Color.Parse("#00000080") }
                    }
                );
            }
            else
            {
                // Default state
                _chatPillBorder.Opacity = 0.95;
                _chatPillBorder.BoxShadow = new BoxShadows(
                    new BoxShadow { OffsetX = 0, OffsetY = 0, Blur = 30, Spread = 5, Color = Color.Parse("#00ffff40") },
                    new[] {
                        new BoxShadow { OffsetX = 0, OffsetY = 0, Blur = 60, Spread = 10, Color = Color.Parse("#ff00ff20") },
                        new BoxShadow { OffsetX = 0, OffsetY = 4, Blur = 20, Spread = 0, Color = Color.Parse("#00000080") }
                    }
                );
            }
        }, Avalonia.Threading.DispatcherPriority.Render);
    }

    private void UpdateWaveform()
    {
        if (_waveformCanvas == null) return;
        
        bool isListening = false;
        bool isTtsActive = false;
        if (DataContext is CortexViewModel vm)
        {
            isListening = vm.IsVoiceListening && !vm.IsVoiceMuted;
            isTtsActive = vm.IsTtsSpeaking;
        }
        
        // Performance optimization: Only redraw if needed
        bool shouldRedraw = isTtsActive != _isTtsSpeaking ||
                           (DateTime.Now - _lastWaveformUpdate).TotalMilliseconds >= MinUpdateIntervalMs;
        
        if (!shouldRedraw) return;
        
        _lastWaveformUpdate = DateTime.Now;
        
        _waveformCanvas.Children.Clear();
        double canvasHeight = _waveformCanvas.Bounds.Height;
        if (canvasHeight <= 0) canvasHeight = 50;
        double canvasWidth = _waveformCanvas.Bounds.Width;
        if (canvasWidth <= 0) canvasWidth = 50;
        
        double centerX = canvasWidth / 2;
        double centerY = canvasHeight / 2;
        
        // Draw waveform
        if (isListening)
        {
            DrawListeningWaveform(canvasWidth, canvasHeight, centerX, centerY);
        }
        else if (isTtsActive)
        {
            DrawTtsPulseAnimation(canvasWidth, canvasHeight, centerX, centerY);
        }
        else
        {
            DrawIdleWaveform(canvasWidth, canvasHeight, centerX, centerY);
        }
    }

    private void DrawListeningWaveform(double canvasWidth, double canvasHeight, double centerX, double centerY)
    {
        // Simple waveform visualization when listening
        var barColor = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00ff88"));
        for (int i = 0; i < 3; i++)
        {
            double barWidth = 3;
            double spacing = 4;
            double startX = centerX - (barWidth + spacing);
            double barHeight = canvasHeight * (0.3 + (i * 0.15));
            
            var bar = new Avalonia.Controls.Shapes.Rectangle
            {
                Fill = barColor,
                Width = barWidth,
                Height = barHeight,
                [Avalonia.Controls.Canvas.LeftProperty] = startX + (i * (barWidth + spacing)),
                [Avalonia.Controls.Canvas.TopProperty] = centerY - (barHeight / 2)
            };
            _waveformCanvas?.Children.Add(bar);
        }
    }

    private void DrawTtsPulseAnimation(double canvasWidth, double canvasHeight, double centerX, double centerY)
    {
        // Draw pulse rings for TTS
        for (int ring = 0; ring < 3; ring++)
        {
            double ringRadius = (DateTime.Now.Millisecond / 20.0) + (ring * 15);
            if (ringRadius > 60) ringRadius = ringRadius % 60;
            
            var pulseOpacity = 1.0 - (ringRadius / 60.0);
            if (pulseOpacity > 0)
            {
                var pulseGradient = new Avalonia.Media.RadialGradientBrush
                {
                    GradientStops = new Avalonia.Media.GradientStops
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 0, 0), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 127, 0), 0.16),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(255, 255, 0), 0.33),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(0, 255, 0), 0.5),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(0, 255, 255), 0.66),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(0, 127, 255), 0.83),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.FromRgb(127, 0, 255), 1)
                    },
                    Center = new Avalonia.RelativePoint(0.5, 0.5, Avalonia.RelativeUnit.Relative),
                    Radius = 1.0,
                    Opacity = pulseOpacity * 0.6
                };
                
                var pulseCircle = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = ringRadius * 2,
                    Height = ringRadius * 2,
                    Fill = pulseGradient,
                    [Avalonia.Controls.Canvas.LeftProperty] = centerX - ringRadius,
                    [Avalonia.Controls.Canvas.TopProperty] = centerY - ringRadius
                };
                _waveformCanvas?.Children.Add(pulseCircle);
            }
        }
    }

    private void DrawIdleWaveform(double canvasWidth, double canvasHeight, double centerX, double centerY)
    {
        // Minimal visualization when idle
        var idleCircle = new Avalonia.Controls.Shapes.Ellipse
        {
            Width = 20,
            Height = 20,
            Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00ffff40")),
            Opacity = 0.3,
            [Avalonia.Controls.Canvas.LeftProperty] = centerX - 10,
            [Avalonia.Controls.Canvas.TopProperty] = centerY - 10
        };
        _waveformCanvas?.Children.Add(idleCircle);
    }

    private void TxtInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter && DataContext is CortexViewModel vm)
        {
            // Handle Enter key - send message
            if (!string.IsNullOrWhiteSpace(vm.ChatInput))
            {
                SendMessage();
                e.Handled = true;
            }
        }
    }
    
    private void BtnSend_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SendMessage();
    }
    
    private void SendMessage()
    {
        if (DataContext is not CortexViewModel vm) return;
        
        if (string.IsNullOrWhiteSpace(vm.ChatInput)) return;
        
        // Send chat message
        _ = vm.SendChatMessageCommand.ExecuteAsync(null);
    }
    
    private void WaveformCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Stop TTS playback when waveform is clicked
        if (DataContext is CortexViewModel vm)
        {
            // Cancel TTS if available
            // vm.CancelTtsQueue(); // Will be implemented when TTS is added
        }
    }
}
