using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public partial class MainMenuWindow : Window
    {
        private const double BackgroundEdgePadding = 40;

        private readonly Level1? pauseOwner;
        private readonly bool isPauseSettingsMode;

        private double backgroundX;
        private double backgroundSpeed = -20;
        private double backgroundWidth;
        private double backgroundHeight;
        private DateTime lastUpdateTime;

        public MainMenuWindow() : this(null)
        {
        }

        public MainMenuWindow(Level1 pauseOwner)
        {
            this.pauseOwner = pauseOwner;
            isPauseSettingsMode = pauseOwner != null;

            InitializeComponent();

            if (isPauseSettingsMode)
            {
                Owner = pauseOwner;
                AllowsTransparency = true;
                Background = new SolidColorBrush(Color.FromArgb(0xAA, 0, 0, 0));
                ShowInTaskbar = false;
            }

            BuildKeyBindingRows();
            Loaded += MainMenuWindow_Loaded;
            Closing += MainMenuWindow_Closing;
        }

        private void MainMenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (isPauseSettingsMode)
            {
                BackgroundCanvas.Visibility = Visibility.Collapsed;
                ShowView(SettingsView);
            }
            else
            {
                if (MainMenuBackground.Source is BitmapSource bitmap)
                {
                    backgroundHeight = ActualHeight + 100;
                    backgroundWidth = backgroundHeight * bitmap.PixelWidth / bitmap.PixelHeight;
                    MainMenuBackground.Width = backgroundWidth;
                    MainMenuBackground.Height = backgroundHeight;
                    Canvas.SetLeft(MainMenuBackground, 0);
                    Canvas.SetTop(MainMenuBackground, -50);
                }

                backgroundX = -BackgroundEdgePadding;
                Canvas.SetLeft(MainMenuBackground, backgroundX);
                lastUpdateTime = DateTime.Now;
                CompositionTarget.Rendering += BackgroundLoop;
            }

            CompositionTarget.Rendering += VolumeSliderDragLoop;

            LoadSettings();
            ApplySettingsToUI();

            savedKeyBindings.Clear();
            foreach (var binding in keyBindings)
                savedKeyBindings[binding.Key] = binding.Value;

            SetSettingsChanged(false);
            SetKeyBindingsChanged(false);
        }

        private void MainMenuWindow_Closing(object? sender, CancelEventArgs e)
        {
            CompositionTarget.Rendering -= BackgroundLoop;
            CompositionTarget.Rendering -= VolumeSliderDragLoop;

            if (isPauseSettingsMode && pauseOwner != null && !pauseOwner.IsDeadWindow)
            {
                pauseOwner.RestorePauseMenuAfterSettings();
            }
        }

        private void BackgroundLoop(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double deltaTime = Math.Min((now - lastUpdateTime).TotalSeconds, 0.1);
            lastUpdateTime = now;

            if (backgroundWidth <= ActualWidth)
                return;

            backgroundX += backgroundSpeed * deltaTime;

            double minX = ActualWidth - backgroundWidth + BackgroundEdgePadding;
            double maxX = -BackgroundEdgePadding;

            if (backgroundX <= minX)
            {
                backgroundX = minX;
                backgroundSpeed = Math.Abs(backgroundSpeed);
            }
            else if (backgroundX >= maxX)
            {
                backgroundX = maxX;
                backgroundSpeed = -Math.Abs(backgroundSpeed);
            }

            Canvas.SetLeft(MainMenuBackground, backgroundX);
        }

        // -------------------- NÉZETEK --------------------

        private void ShowView(Grid view)
        {
            MainMenuView.Visibility = view == MainMenuView ? Visibility.Visible : Visibility.Collapsed;
            GameModeView.Visibility = view == GameModeView ? Visibility.Visible : Visibility.Collapsed;
            SettingsView.Visibility = view == SettingsView ? Visibility.Visible : Visibility.Collapsed;
            KeyBindingsView.Visibility = view == KeyBindingsView ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e) => ShowView(GameModeView);
        private void BackToMainMenu_Click(object sender, RoutedEventArgs e) => ShowView(MainMenuView);

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowView(SettingsView);
            UpdateDisplaySettings();
        }

        private void BackFromSettings_Click(object sender, RoutedEventArgs e)
        {
            if (isPauseSettingsMode)
            {
                Close();
                return;
            }

            ShowView(MainMenuView);
        }

        private void KeyBindings_Click(object sender, RoutedEventArgs e)
        {
            CancelKeyBindingCapture();
            UpdateKeyBindingUI();
            ShowView(KeyBindingsView);
        }

        private void BackFromKeyBindings_Click(object sender, RoutedEventArgs e)
        {
            CancelKeyBindingCapture();
            ShowView(SettingsView);
        }

        private BitmapImage LoadMenuImage(string fileName) =>
            new(new Uri($"pack://application:,,,/Assets/Menu/{fileName}", UriKind.Absolute));

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
