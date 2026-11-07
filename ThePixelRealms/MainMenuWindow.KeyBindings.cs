using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public partial class MainMenuWindow
    {
        private readonly Dictionary<string, Key> keyBindings = GameKeyBindings.Bindings;

        private readonly Dictionary<string, Button> keyBindingButtons = new();
        private readonly Dictionary<string, List<Button>> dependentKeyBindingButtons = new();

        private readonly Dictionary<string, Key> defaultKeyBindings = new();
        private readonly Dictionary<string, Key> savedKeyBindings = new();

        private Button? activeKeyBindingButton;
        private string? activeKeyBindingId;

        private void KeyBindingInput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id || !keyBindings.ContainsKey(id))
                return;

            if (activeKeyBindingButton != null && activeKeyBindingButton != button)
                SetKeyBindingButtonVisual(activeKeyBindingButton, false);

            activeKeyBindingButton = button;
            activeKeyBindingId = id;
            SetKeyBindingButtonVisual(button, true);
            SetKeyBindingButtonText(button, "...");
            button.Focus();
        }

        private void KeyBindingInput_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button != activeKeyBindingButton)
                SetKeyBindingButtonVisual(button, true);
        }

        private void KeyBindingInput_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button != activeKeyBindingButton)
                SetKeyBindingButtonVisual(button, false);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (activeKeyBindingButton == null || activeKeyBindingId == null)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                CancelKeyBindingCapture();
                return;
            }

            Key pressedKey = e.Key == Key.System ? e.SystemKey : e.Key;

            // Ugyanaz a billentyű több, egymástól független bindhoz nem tartozhat.
            // A Primary kombóelemek nem külön bindok, ezért azok természetesen
            // ugyanazt a billentyűt mutatják, mint a Primary forrásuk.
            foreach (var binding in keyBindings)
            {
                if (binding.Key != activeKeyBindingId && binding.Value == pressedKey)
                    return;
            }

            keyBindings[activeKeyBindingId] = pressedKey;
            SetKeyBindingButtonText(activeKeyBindingButton, GetKeyDisplayName(pressedKey));

            if (dependentKeyBindingButtons.TryGetValue(activeKeyBindingId, out List<Button>? dependents)) 
                foreach (Button dependent in dependents)
                    SetKeyBindingButtonText(dependent, GetKeyDisplayName(pressedKey));

            SetKeyBindingButtonVisual(activeKeyBindingButton, false);
            activeKeyBindingButton = null;
            activeKeyBindingId = null;

            UpdateKeyBindingsChanged();
        }

        private void CancelKeyBindingCapture()
        {
            if (activeKeyBindingButton != null && activeKeyBindingId != null)
            {
                SetKeyBindingButtonText(activeKeyBindingButton, GetKeyDisplayName(keyBindings[activeKeyBindingId]));
                SetKeyBindingButtonVisual(activeKeyBindingButton, false);
            }

            activeKeyBindingButton = null;
            activeKeyBindingId = null;
        }

        private void BuildKeyBindingRows()
        {
            var rows = new[]
            {
                // id / cím / ikon / leírás / kategória / kombó elemek / elválasztók
                ("MoveLeft", "Mozgás balra", "WalkLeft.png", "A karakter mozgatása balra.", "MOZGÁS",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),
                ("MoveRight", "Mozgás jobbra", "WalkRight.png", "A karakter mozgatása jobbra.", "",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),
                ("Run", "Futás", "Run.png", "Gyorsabb mozgás futás közben.", "",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),
                ("Jump", "Ugrás", "Jump.png", "Ugrás a karakterrel, akár mozgás közben.", "",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),

                ("Sword", "Ütés", "Sword.png", "Egyszerű támadás a kiválasztott közelharci fegyverrel.", "HARC (Alap)",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),
                ("Bow", "Lövés", "Bow.png", "Távolsági fegyver használata a távolabb lévő ellenfelek ellen.", "",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),
                ("Shield", "Pajzs", "Shield.png", "Védekezés a felszerelt pajzzsal.", "",
                    Array.Empty<(string BindingId, int Count, bool Primary)>(), Array.Empty<string>()),

                ("", "Dupla sújtás", "", "Két egyszerű támadás karddal egymás után.", "HARC (Haladó)",
                    new[] { ("Sword", 2, true) },
                    Array.Empty<string>()),

                ("", "Dupla sújtás + döfés", "", "Két egyszerű támadás karddal egymás után, majd egy nagy döfés.", "",
                    new[] { ("Sword", 3, true) },
                    Array.Empty<string>()),

                ("", "Nehéz sújtás", "", "Egy hátulról induló, nagy erőteljes csapás karddal.", "",
                    new[] { ("Sword", 1, true), ("Bow", 1, true) },
                    new[] { "+" }),

                ("", "Forgó sújtás", "", "Egy teljes fordulat karddal kisebb tömegellenfelek ellen.", "",
                    new[] { ("Sword", 1, true), ("Shield", 1, true) },
                    new[] { "+" }),

                ("", "Tripla lövés", "", "Egyszerre három nyílvesszőt lő ki az íjjal.", "",
                    new[] { ("Bow", 3, true) },
                    Array.Empty<string>()),

                ("", "Lökés pajzzsal", "", "Hátrébblök egyszerre több ellenfelet is.", "",
                    new[] { ("Shield", 2, true) },
                    Array.Empty<string>())
            };

            foreach (var row in rows)
            {
                if (!string.IsNullOrEmpty(row.Item5))
                    KeyBindingRows.Children.Add(CreateCategoryTitle(row.Item5));

                if (row.Item6.Length == 0)
                    KeyBindingRows.Children.Add(CreateKeyBindingRow(row.Item1, row.Item2, row.Item3, row.Item4));
                else
                    KeyBindingRows.Children.Add(CreateComboRow(row.Item2, row.Item4, row.Item6, row.Item7));
            }
        }

        private Grid CreateCategoryTitle(string text)
        {
            Grid section = new()
            {
                Height = 110,
                Margin = new Thickness(0, 30, 0, 25)
            };

            section.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            section.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1) });
            section.RowDefinitions.Add(new RowDefinition { Height = new GridLength(49) });

            TextBlock title = new()
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            Grid.SetRow(title, 0);
            section.Children.Add(title);

            return section;
        }

        private Grid CreateKeyBindingRow(string id, string title, string icon, string description)
        {
            Grid row = CreateKeyBindingRowBase(title, description);
            row.Children.Add(new Image { Source = LoadKeyBindingImage(icon), Width = 78, Height = 90, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
            Grid.SetColumn(row.Children[^1], 2);

            Button button = new()
            {
                Tag = id,
                Style = (Style)FindResource("KeyBindingInputButtonStyle"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = CreateInputVisual(GetKeyDisplayName(keyBindings[id]))
            };
            button.Click += KeyBindingInput_Click;
            button.MouseEnter += KeyBindingInput_MouseEnter;
            button.MouseLeave += KeyBindingInput_MouseLeave;
            keyBindingButtons[id] = button;

            Grid input = new() { Width = 410, HorizontalAlignment = HorizontalAlignment.Center };
            input.Children.Add(button);
            Grid.SetColumn(input, 4);
            row.Children.Add(input);
            return row;
        }

        private Grid CreateComboRow(
            string title,
            string description,
            (string BindingId, int Count, bool Primary)[] inputs,
            string[] separators)
        {
            Grid row = CreateKeyBindingRowBase(title, description);
            StackPanel combo = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            for (int i = 0; i < inputs.Length; i++)
            {
                if (i > 0)
                {
                    string separator = i - 1 < separators.Length ? separators[i - 1] : "→";
                    combo.Children.Add(new TextBlock
                    {
                        Text = separator,
                        Foreground = Brushes.White,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 0, 5, 0)
                    });
                }

                var input = inputs[i];

                if (input.Count > 1)
                {
                    combo.Children.Add(new TextBlock
                    {
                        Text = $"{input.Count}x",
                        Foreground = Brushes.White,
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    });
                }

                Button button = new()
                {
                    Tag = input.BindingId,
                    Style = (Style)FindResource("KeyBindingInputButtonStyle"),
                    Content = CreateInputVisual(GetKeyDisplayName(keyBindings[input.BindingId]))
                };

                if (input.Primary)
                {
                    button.IsHitTestVisible = false;
                    button.Focusable = false;
                    button.Cursor = Cursors.Arrow;
                    button.Opacity = 0.55;

                    if (!dependentKeyBindingButtons.TryGetValue(input.BindingId, out List<Button>? dependents))
                    {
                        dependents = new List<Button>();
                        dependentKeyBindingButtons[input.BindingId] = dependents;
                    }

                    dependents.Add(button);
                }
                else
                {
                    button.Click += KeyBindingInput_Click;
                    button.MouseEnter += KeyBindingInput_MouseEnter;
                    button.MouseLeave += KeyBindingInput_MouseLeave;
                    keyBindingButtons[input.BindingId] = button;
                }

                combo.Children.Add(button);
            }

            Grid.SetColumn(combo, 4);
            row.Children.Add(combo);
            return row;
        }

        private Grid CreateKeyBindingRowBase(string title, string description)
        {
            Grid row = new() { Height = 96 };
            foreach (GridLength width in new[] { new GridLength(210), new GridLength(1), new GridLength(125), new GridLength(1), new GridLength(430), new GridLength(1), new GridLength(1, GridUnitType.Star) })
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            row.Children.Add(CreateRowText(title, 0, 22, TextAlignment.Left));
            foreach (int column in new[] { 1, 3, 5 })
            {
                Border separator = new() { Width = 1, Margin = new Thickness(0, 10, 0, 10), Background = new SolidColorBrush(Color.FromArgb(0xA0, 0, 0, 0)) };
                Grid.SetColumn(separator, column);
                row.Children.Add(separator);
            }
            row.Children.Add(CreateRowText(description, 6, 18, TextAlignment.Center));
            return row;
        }

        private TextBlock CreateRowText(string text, int column, double size, TextAlignment alignment)
        {
            TextBlock block = new() { Text = text, Foreground = Brushes.White, FontSize = size, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = alignment == TextAlignment.Center ? HorizontalAlignment.Center : HorizontalAlignment.Left, TextAlignment = alignment, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(12, 0, 12, 0) };
            Grid.SetColumn(block, column);
            return block;
        }

        private Grid CreateInputVisual(string text)
        {
            Grid grid = new() { Width = 106, Height = 72 };
            grid.Children.Add(new Image { Source = LoadKeyBindingImage("Input.png"), Width = 106, Height = 72, Stretch = Stretch.Fill });
            grid.Children.Add(new TextBlock { Text = text, Foreground = Brushes.White, FontSize = 18, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false });
            return grid;
        }

        private BitmapImage LoadKeyBindingImage(string fileName) =>
            new(new Uri($"pack://application:,,,/Assets/Menu/KeyBindings/{fileName}", UriKind.Absolute));

        private void UpdateKeyBindingUI()
        {
            foreach (var binding in keyBindings)
            {
                string text = GetKeyDisplayName(binding.Value);

                if (keyBindingButtons.TryGetValue(binding.Key, out Button? button))
                    SetKeyBindingButtonText(button, text);

                if (dependentKeyBindingButtons.TryGetValue(binding.Key, out List<Button>? dependents))
                    foreach (Button dependent in dependents)
                        SetKeyBindingButtonText(dependent, text);
            }
        }

        private void SetKeyBindingButtonVisual(Button button, bool active)
        {
            if (button.Content is not Grid grid || grid.Children.Count < 2 || grid.Children[0] is not Image image)
                return;

            image.Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Menu/KeyBindings/Input{(active ? "_Active" : "")}.png", UriKind.Absolute));
            button.RenderTransform = active ? new ScaleTransform(1.05, 1.05) : new ScaleTransform(1, 1);
        }

        private void SetKeyBindingButtonText(Button button, string text)
        {
            if (button.Content is Grid grid && grid.Children.Count >= 2 && grid.Children[1] is TextBlock textBlock)
                textBlock.Text = text;
        }

        private static string GetKeyDisplayName(Key key) => key switch
        {
            Key.LeftShift => "LSHIFT",
            Key.RightShift => "RSHIFT",
            Key.LeftCtrl => "LCTRL",
            Key.RightCtrl => "RCTRL",
            Key.LeftAlt => "LALT",
            Key.RightAlt => "RALT",
            Key.Space => "SPACE",
            Key.Return => "ENTER",
            Key.Back => "BACKSPACE",
            _ => key.ToString().ToUpperInvariant()
        };

        // -------------------- HANGERŐ SLIDEREK --------------------
        // A Canvas Tag-je az azonosító: Master / Music / Environment / Entity.
        // Emiatt a drag-kód mind a négy sliderhez ugyanaz.


        private void ResetKeyBindings_Click(object sender, RoutedEventArgs e)
        {
            CancelKeyBindingCapture();

            foreach (var binding in defaultKeyBindings)
                keyBindings[binding.Key] = binding.Value;

            UpdateKeyBindingUI();
            UpdateKeyBindingsChanged();
        }

        private void SaveKeyBindings_Click(object sender, RoutedEventArgs e)
        {
            CancelKeyBindingCapture();
            SaveConfigFile();

            savedKeyBindings.Clear();
            foreach (var binding in keyBindings)
                savedKeyBindings[binding.Key] = binding.Value;

            SetKeyBindingsChanged(false);
        }

        private void UpdateKeyBindingsChanged()
        {
            bool changed = keyBindings.Count != savedKeyBindings.Count;

            if (!changed)
            {
                foreach (var binding in keyBindings)
                {
                    if (!savedKeyBindings.TryGetValue(binding.Key, out Key savedKey) ||
                        savedKey != binding.Value)
                    {
                        changed = true;
                        break;
                    }
                }
            }

            SetKeyBindingsChanged(changed);
        }

        private void SetKeyBindingsChanged(bool changed)
        {
            keyBindingsChanged = changed;

            SaveKeyBindingsButton.IsEnabled = changed;
            SaveKeyBindingsButton.Opacity = changed ? 1.0 : 0.25;

            bool isDefault = true;

            foreach (var binding in keyBindings)
            {
                if (!defaultKeyBindings.TryGetValue(binding.Key, out Key defaultKey) ||
                    defaultKey != binding.Value)
                {
                    isDefault = false;
                    break;
                }
            }

            ResetKeyBindingsButton.IsEnabled = !isDefault;
            ResetKeyBindingsButton.Opacity = isDefault ? 0.25 : 1.0;
        }
    }


    public static class GameKeyBindings
    {
        // Ezek az induló kiosztások csak az első config.txt létrehozásához kellenek.
        // A Reset alapértékeit a config.txt-ben az adott beállítás alatti Default= sor adja.
        public static readonly Dictionary<string, Key> Bindings = new()
        {
            ["MoveLeft"] = Key.A,
            ["MoveRight"] = Key.D,
            ["Run"] = Key.LeftShift,
            ["Jump"] = Key.Space,
            ["Sword"] = Key.F,
            ["Bow"] = Key.G,
            ["Shield"] = Key.V
        };

        public static string ConfigPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "The Pixel Realms",
                "config.txt");

        static GameKeyBindings() => Load();

        public static Key Get(string id) => Bindings[id];

        public static void Load()
        {
            if (!File.Exists(ConfigPath))
                return;

            foreach (string line in File.ReadLines(ConfigPath))
            {
                string[] parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();

                if (!key.StartsWith("KeyBinding.", StringComparison.Ordinal))
                    continue;

                string id = key["KeyBinding.".Length..];

                if (Bindings.ContainsKey(id) &&
                    Enum.TryParse(parts[1].Trim(), true, out Key parsedKey))
                {
                    Bindings[id] = parsedKey;
                }
            }
        }
    }

}
