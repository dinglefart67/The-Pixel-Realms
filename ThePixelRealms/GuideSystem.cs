using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public class GuideSystem
    {
        private readonly IGameState map;

        private readonly TextBlock text;
        private readonly Image left;
        private readonly Image middle;
        private readonly Image right;

        private readonly List<string> lines = new();

        private bool visibleByGame = false;

        public bool IsActive { get; private set; } = false;

        public GuideSystem(IGameState map)
        {
            this.map = map;

            text = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Hidden
            };

            left = new Image
            {
                Source = Load("Assets/UI/GuideBubble_Left.png"),
                Height = 64,
                Width = 32,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            middle = new Image
            {
                Source = Load("Assets/UI/GuideBubble_Middle.png"),
                Height = 64,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            right = new Image
            {
                Source = Load("Assets/UI/GuideBubble_Right.png"),
                Height = 64,
                Width = 32,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            Panel.SetZIndex(left, 4000);
            Panel.SetZIndex(middle, 4001);
            Panel.SetZIndex(right, 4002);
            Panel.SetZIndex(text, 4003);

            map.GameCanvas.Children.Add(left);
            map.GameCanvas.Children.Add(middle);
            map.GameCanvas.Children.Add(right);
            map.GameCanvas.Children.Add(text);

            LoadLines();
        }

        private BitmapImage Load(string path)
        {
            return new BitmapImage(new Uri(path, UriKind.Relative));
        }

        private void LoadLines()
        {
            try
            {
                using var reader = new StreamReader("Assets/Guide/Level1.txt", Encoding.UTF8);

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
            catch
            {
                lines.Add("Guide load error.");
            }
        }

        public void ShowLine(int index)
        {
            if (index < 0 || index >= lines.Count)
                return;

            text.Text = lines[index];
            visibleByGame = true;
            IsActive = true;

            if (!map.dialogueSystem.IsActive && !map.cutscene.IsActive)
            {
                left.Visibility =
                middle.Visibility =
                right.Visibility =
                text.Visibility = Visibility.Visible;
            }

            Layout();
        }

        public void HideGuide()
        {
            visibleByGame = false;
            IsActive = false;

            left.Visibility =
            middle.Visibility =
            right.Visibility =
            text.Visibility = Visibility.Hidden;
        }

        public void Update()
        {
            if (!visibleByGame)
            {
                left.Visibility =
                middle.Visibility =
                right.Visibility =
                text.Visibility = Visibility.Hidden;

                return;
            }

            if (map.dialogueSystem.IsActive || map.cutscene.IsActive)
            {
                left.Visibility =
                middle.Visibility =
                right.Visibility =
                text.Visibility = Visibility.Hidden;

                return;
            }

            left.Visibility =
            middle.Visibility =
            right.Visibility =
            text.Visibility = Visibility.Visible;

            Layout();
        }

        private void Layout()
        {
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = Math.Ceiling(text.DesiredSize.Width);

            double padding = 40;
            double totalWidth = textWidth + padding + 64;

            double startX = 40;
            double y = 40;

            double middleWidth = totalWidth - 64;

            left.Width = 32;
            right.Width = 32;
            middle.Width = middleWidth;

            Canvas.SetLeft(left, startX);
            Canvas.SetLeft(middle, startX + 30);
            Canvas.SetLeft(right, startX + 28 + middleWidth);

            Canvas.SetTop(left, y);
            Canvas.SetTop(middle, y);
            Canvas.SetTop(right, y);

            Canvas.SetLeft(text, startX + 32 + (middleWidth - textWidth) / 2.0);
            Canvas.SetTop(text, y + 18);
        }
    }
}