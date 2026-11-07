using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public partial class MainMenuWindow
    {
        private readonly Dictionary<string, int> volumes = new()
        {
            ["Master"] = 50,
            ["Music"] = 50,
            ["Environment"] = 50,
            ["Entity"] = 50
        };

        private string ConfigPath => GameKeyBindings.ConfigPath;

        private Canvas? activeVolumeSlider;
        private Button? activeVolumeButton;
        private bool isDraggingVolume;
        private bool settingsChanged;
        private bool keyBindingsChanged;
        private double volumeDragOffsetX;

        private readonly string[] resolutions =
        {
            "Auto", "1280 × 720", "1366 × 768", "1600 × 900",
            "1920 × 1080", "2560 × 1440", "2560 × 1600", "3840 × 2160"
        };

        private readonly string[] screenModes = { "Teljes képernyő", "Keret nélküli", "Ablakos" };
        private readonly string[] uiScales = { "Auto", "Kicsi", "Normál", "Nagy" };

        private int resolutionIndex;
        private int screenModeIndex;
        private int uiScaleIndex;

        private int defaultResolutionIndex = 0;
        private int defaultScreenModeIndex = 0;
        private int defaultUiScaleIndex = 0;

        private readonly Dictionary<string, int> defaultVolumes = new()
        {
            ["Master"] = 50,
            ["Music"] = 50,
            ["Environment"] = 50,
            ["Entity"] = 50
        };


        private void ResolutionLeft_Click(object sender, RoutedEventArgs e) =>
            ChangeIndex(ref resolutionIndex, -1, resolutions.Length);

        private void ResolutionRight_Click(object sender, RoutedEventArgs e) =>
            ChangeIndex(ref resolutionIndex, 1, resolutions.Length);

        private void ScreenModeLeft_Click(object sender, RoutedEventArgs e) =>
            ChangeIndex(ref screenModeIndex, -1, screenModes.Length);

        private void ScreenModeRight_Click(object sender, RoutedEventArgs e) =>
            ChangeIndex(ref screenModeIndex, 1, screenModes.Length);

        private void UiScaleLeft_Click(object sender, RoutedEventArgs e) =>
            ChangeIndex(ref uiScaleIndex, -1, uiScales.Length);

        private void UiScaleRight_Click(object sender, RoutedEventArgs e) =>
            ChangeIndex(ref uiScaleIndex, 1, uiScales.Length);

        private void ChangeIndex(ref int index, int direction, int count)
        {
            int oldIndex = index;

            index = Math.Clamp(index + direction, 0, count - 1);
            UpdateDisplaySettings();

            if (index != oldIndex)
                SetSettingsChanged(true);
        }

        private void UpdateDisplaySettings()
        {
            ResolutionText.Text = resolutions[resolutionIndex];
            ScreenModeText.Text = screenModes[screenModeIndex];
            UiScaleText.Text = uiScales[uiScaleIndex];

            SetArrowState(ResolutionLeftButton, ResolutionLeftImage, resolutionIndex > 0, false);
            SetArrowState(ResolutionRightButton, ResolutionRightImage, resolutionIndex < resolutions.Length - 1, true);
            SetArrowState(ScreenModeLeftButton, ScreenModeLeftImage, screenModeIndex > 0, false);
            SetArrowState(ScreenModeRightButton, ScreenModeRightImage, screenModeIndex < screenModes.Length - 1, true);
            SetArrowState(UiScaleLeftButton, UiScaleLeftImage, uiScaleIndex > 0, false);
            SetArrowState(UiScaleRightButton, UiScaleRightImage, uiScaleIndex < uiScales.Length - 1, true);
        }

        private void SetArrowState(Button button, Image image, bool enabled, bool right)
        {
            button.IsEnabled = enabled;
            if (!enabled)
                image.Source = LoadMenuImage(right ? "DisplayBorderButtonRight.png" : "DisplayBorderButtonLeft.png");
        }

        private void DisplayArrow_MouseEnter(object sender, MouseEventArgs e) => SetArrowHover(sender, true);
        private void DisplayArrow_MouseLeave(object sender, MouseEventArgs e) => SetArrowHover(sender, false);

        private void SetArrowHover(object sender, bool active)
        {
            if (sender is not Button button || !button.IsEnabled || button.Content is not Image image)
                return;

            bool right = button == ResolutionRightButton || button == ScreenModeRightButton || button == UiScaleRightButton;
            image.Source = LoadMenuImage($"DisplayBorderButton{(right ? "Right" : "Left")}{(active ? "_Active" : "")}.png");
        }

        // -------------------- BILLENTYŰK --------------------


        private void VoiceButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Button button || button.Parent is not Canvas slider || slider.Tag is not string id || !volumes.ContainsKey(id))
                return;

            activeVolumeButton = button;
            activeVolumeSlider = slider;
            isDraggingVolume = true;
            volumeDragOffsetX = e.GetPosition(button).X;
            e.Handled = true;
        }

        private void VolumeSliderDragLoop(object? sender, EventArgs e)
        {
            if (!isDraggingVolume || activeVolumeSlider == null || activeVolumeButton == null)
                return;

            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                StopVolumeDragging();
                return;
            }

            const double buttonWidth = 24;
            double usableWidth = activeVolumeSlider.ActualWidth - buttonWidth;
            if (usableWidth <= 0)
                return;

            double left = Math.Clamp(Mouse.GetPosition(activeVolumeSlider).X - volumeDragOffsetX, 0, usableWidth);
            Canvas.SetLeft(activeVolumeButton, left);

            if (activeVolumeSlider.Tag is string id)
            {
                int newValue = (int)Math.Round(left / usableWidth * 100);

                if (volumes[id] != newValue)
                {
                    volumes[id] = newValue;
                    SetSettingsChanged(true);
                }
            }
        }

        private void StopVolumeDragging()
        {
            isDraggingVolume = false;
            activeVolumeSlider = null;
            activeVolumeButton = null;
            volumeDragOffsetX = 0;
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            bool changed =
                resolutionIndex != defaultResolutionIndex ||
                screenModeIndex != defaultScreenModeIndex ||
                uiScaleIndex != defaultUiScaleIndex ||
                volumes["Master"] != defaultVolumes["Master"] ||
                volumes["Music"] != defaultVolumes["Music"] ||
                volumes["Environment"] != defaultVolumes["Environment"] ||
                volumes["Entity"] != defaultVolumes["Entity"];

            resolutionIndex = defaultResolutionIndex;
            screenModeIndex = defaultScreenModeIndex;
            uiScaleIndex = defaultUiScaleIndex;

            foreach (var volume in defaultVolumes)
                volumes[volume.Key] = volume.Value;

            StopVolumeDragging();
            ApplySettingsToUI();

            if (changed)
                SetSettingsChanged(true);
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigFile();
            SetSettingsChanged(false);
        }
        private void LoadSettings()
        {
            defaultKeyBindings.Clear();
            foreach (var binding in keyBindings)
                defaultKeyBindings[binding.Key] = binding.Value;

            EnsureConfigFile();

            string? previousSetting = null;

            foreach (string line in File.ReadLines(ConfigPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (key == "Default")
                {
                    if (previousSetting != null)
                        ApplyConfigValue(previousSetting, value, true);

                    previousSetting = null;
                    continue;
                }

                ApplyConfigValue(key, value, false);
                previousSetting = key;
            }
        }

        private void ApplyConfigValue(string key, string value, bool isDefault)
        {
            const string keyBindingPrefix = "KeyBinding.";

            if (key.StartsWith(keyBindingPrefix, StringComparison.Ordinal))
            {
                string bindingId = key[keyBindingPrefix.Length..];

                if (Enum.TryParse(value, true, out Key parsedKey))
                {
                    if (isDefault)
                    {
                        if (defaultKeyBindings.ContainsKey(bindingId))
                            defaultKeyBindings[bindingId] = parsedKey;
                    }
                    else if (keyBindings.ContainsKey(bindingId))
                    {
                        keyBindings[bindingId] = parsedKey;
                    }
                }

                return;
            }

            if (!int.TryParse(value, out int number))
                return;

            if (isDefault)
            {
                switch (key)
                {
                    case "Resolution":
                        defaultResolutionIndex = Math.Clamp(number, 0, resolutions.Length - 1);
                        break;
                    case "ScreenMode":
                        defaultScreenModeIndex = Math.Clamp(number, 0, screenModes.Length - 1);
                        break;
                    case "UiScale":
                        defaultUiScaleIndex = Math.Clamp(number, 0, uiScales.Length - 1);
                        break;
                    case "MasterVolume":
                        defaultVolumes["Master"] = Math.Clamp(number, 0, 100);
                        break;
                    case "MusicVolume":
                        defaultVolumes["Music"] = Math.Clamp(number, 0, 100);
                        break;
                    case "EnvironmentVolume":
                        defaultVolumes["Environment"] = Math.Clamp(number, 0, 100);
                        break;
                    case "EntityVolume":
                        defaultVolumes["Entity"] = Math.Clamp(number, 0, 100);
                        break;
                }

                return;
            }

            switch (key)
            {
                case "Resolution":
                    resolutionIndex = Math.Clamp(number, 0, resolutions.Length - 1);
                    break;
                case "ScreenMode":
                    screenModeIndex = Math.Clamp(number, 0, screenModes.Length - 1);
                    break;
                case "UiScale":
                    uiScaleIndex = Math.Clamp(number, 0, uiScales.Length - 1);
                    break;
                case "MasterVolume":
                    volumes["Master"] = Math.Clamp(number, 0, 100);
                    break;
                case "MusicVolume":
                    volumes["Music"] = Math.Clamp(number, 0, 100);
                    break;
                case "EnvironmentVolume":
                    volumes["Environment"] = Math.Clamp(number, 0, 100);
                    break;
                case "EntityVolume":
                    volumes["Entity"] = Math.Clamp(number, 0, 100);
                    break;
            }
        }

        private void EnsureConfigFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

            if (File.Exists(ConfigPath))
            {
                string[] lines = File.ReadAllLines(ConfigPath);

                // Régi config migrációja: ha nincs Default= sor, a jelenlegi értékeket
                // megtartjuk, és melléjük írjuk a valódi gyári alapértékeket.
                if (Array.Exists(lines, line => line.TrimStart().StartsWith("Default=", StringComparison.Ordinal)))
                    return;

                Dictionary<string, string> current = new(StringComparer.Ordinal);

                foreach (string line in lines)
                {
                    string[] parts = line.Split('=', 2);
                    if (parts.Length == 2)
                        current[parts[0].Trim()] = parts[1].Trim();
                }

                WriteConfigFile(current);
                return;
            }

            WriteConfigFile(new Dictionary<string, string>(StringComparer.Ordinal));
        }

        private void WriteConfigFile(Dictionary<string, string> existing)
        {
            using StreamWriter writer = new(ConfigPath);

            WriteConfigPair(writer, "Resolution",
                existing.TryGetValue("Resolution", out string? resolution) ? resolution : defaultResolutionIndex.ToString(),
                defaultResolutionIndex.ToString());

            WriteConfigPair(writer, "ScreenMode",
                existing.TryGetValue("ScreenMode", out string? screenMode) ? screenMode : defaultScreenModeIndex.ToString(),
                defaultScreenModeIndex.ToString());

            WriteConfigPair(writer, "UiScale",
                existing.TryGetValue("UiScale", out string? uiScale) ? uiScale : defaultUiScaleIndex.ToString(),
                defaultUiScaleIndex.ToString());

            foreach (string id in new[] { "Master", "Music", "Environment", "Entity" })
            {
                string key = $"{id}Volume";
                WriteConfigPair(writer, key,
                    existing.TryGetValue(key, out string? volume) ? volume : defaultVolumes[id].ToString(),
                    defaultVolumes[id].ToString());
            }

            foreach (var binding in GameKeyBindings.Bindings)
            {
                string key = $"KeyBinding.{binding.Key}";
                WriteConfigPair(writer, key,
                    existing.TryGetValue(key, out string? currentKey) ? currentKey : binding.Value.ToString(),
                    binding.Value.ToString());
            }
        }

        private static void WriteConfigPair(StreamWriter writer, string key, string value, string defaultValue)
        {
            writer.WriteLine($"{key}={value}");
            writer.WriteLine($"Default={defaultValue}");
            writer.WriteLine();
        }

        private void SaveConfigFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

            using StreamWriter writer = new(ConfigPath);

            WriteConfigPair(writer, "Resolution", resolutionIndex.ToString(), defaultResolutionIndex.ToString());
            WriteConfigPair(writer, "ScreenMode", screenModeIndex.ToString(), defaultScreenModeIndex.ToString());
            WriteConfigPair(writer, "UiScale", uiScaleIndex.ToString(), defaultUiScaleIndex.ToString());

            foreach (var volume in volumes)
                WriteConfigPair(writer, $"{volume.Key}Volume", volume.Value.ToString(), defaultVolumes[volume.Key].ToString());

            foreach (var binding in keyBindings)
            {
                Key defaultKey = defaultKeyBindings.TryGetValue(binding.Key, out Key configuredDefault)
                    ? configuredDefault
                    : binding.Value;

                WriteConfigPair(writer, $"KeyBinding.{binding.Key}", binding.Value.ToString(), defaultKey.ToString());
            }
        }


        private void ApplySettingsToUI()
        {
            UpdateDisplaySettings();

            SetVolumeButtonPosition(MasterVolumeButton, volumes["Master"]);
            SetVolumeButtonPosition(MusicVolumeButton, volumes["Music"]);
            SetVolumeButtonPosition(EnvironmentVolumeButton, volumes["Environment"]);
            SetVolumeButtonPosition(EntityVolumeButton, volumes["Entity"]);
        }

        private void SetVolumeButtonPosition(Button button, int value)
        {
            const double usableWidth = 216;
            Canvas.SetLeft(button, usableWidth * value / 100.0);
        }

        private void SetSettingsChanged(bool changed)
        {
            settingsChanged = changed;

            SaveSettingsButton.IsEnabled = changed;
            SaveSettingsButton.Opacity = changed ? 1.0 : 0.25;

            UpdateResetButton();
        }

        private void UpdateResetButton()
        {
            bool isDefault =
                resolutionIndex == defaultResolutionIndex &&
                screenModeIndex == defaultScreenModeIndex &&
                uiScaleIndex == defaultUiScaleIndex &&
                volumes["Master"] == defaultVolumes["Master"] &&
                volumes["Music"] == defaultVolumes["Music"] &&
                volumes["Environment"] == defaultVolumes["Environment"] &&
                volumes["Entity"] == defaultVolumes["Entity"];

            ResetSettingsButton.IsEnabled = !isDefault;
            ResetSettingsButton.Opacity = isDefault ? 0.25 : 1.0;
        }
    }
}
