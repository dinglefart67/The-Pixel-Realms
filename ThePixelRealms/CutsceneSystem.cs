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
    public class CutsceneSystem
    {
        private readonly IGameState map;

        private Image fullImage;
        private TextBlock text;
        private TextBlock continueText;

        private Image dlgLeft, dlgMid, dlgRight;

        private readonly List<(BitmapImage image, string text)> steps = new();

        private int currentStepIndex = 0;

        public bool IsActive { get; private set; } = false;

        public CutsceneSystem(IGameState map)
        {
            this.map = map;

            fullImage = new Image
            {
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            text = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 26,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Hidden
            };

            continueText = new TextBlock
            {
                Text = "Tovább [E]",
                Foreground = Brushes.White,
                FontSize = 20,
                Visibility = Visibility.Hidden
            };

            dlgLeft = new Image
            {
                Source = new BitmapImage(new Uri("Assets/UI/DialogBubble_Left.png", UriKind.Relative)),
                Height = 88,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            dlgMid = new Image
            {
                Source = new BitmapImage(new Uri("Assets/UI/DialogBubble_Middle.png", UriKind.Relative)),
                Height = 88,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            dlgRight = new Image
            {
                Source = new BitmapImage(new Uri("Assets/UI/DialogBubble_Right.png", UriKind.Relative)),
                Height = 88,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            Panel.SetZIndex(fullImage, 5000);
            Panel.SetZIndex(text, 5001);
            Panel.SetZIndex(continueText, 5002);
            Panel.SetZIndex(dlgLeft, 5001);
            Panel.SetZIndex(dlgMid, 5001);
            Panel.SetZIndex(dlgRight, 5001);
            Panel.SetZIndex(text, 5002);
            Panel.SetZIndex(continueText, 5003);

            map.RootGrid.Children.Add(fullImage);
            map.RootGrid.Children.Add(text);
            map.RootGrid.Children.Add(continueText);
            map.RootGrid.Children.Add(dlgLeft);
            map.RootGrid.Children.Add(dlgMid);
            map.RootGrid.Children.Add(dlgRight);
        }

        public void Start(string folderPath)
        {
            steps.Clear();
            currentStepIndex = 0;

            string cutsceneName = System.IO.Path.GetFileName(folderPath);

            List<BitmapImage> loadedImages = new();
            for (int i = 1; i <= 20; i++)
            {
                string path = $"{folderPath}/{cutsceneName}_{i}.png";
                if (!File.Exists(path))
                    break;

                loadedImages.Add(new BitmapImage(new Uri(path, UriKind.Relative)));
            }

            List<string> loadedLines = new();
            string txtPath = $"{folderPath}/{cutsceneName}.txt";

            if (File.Exists(txtPath))
            {
                using var reader = new StreamReader(txtPath, Encoding.UTF8);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        loadedLines.Add(line);
                }
            }

            for (int i = 0; i < loadedImages.Count; i++)
            {
                string line = i < loadedLines.Count ? loadedLines[i] : null;
                steps.Add((loadedImages[i], line));
            }

            IsActive = true;
            map.isPaused = true;

            ShowUI();
            UpdateStep();
        }

        public void Update()
        {
            if (!IsActive) return;

            Layout();
        }

        private void UpdateStep()
        {
            if (steps.Count == 0)
                return;

            if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
                return;

            var step = steps[currentStepIndex];

            fullImage.Source = step.image;

            if (!string.IsNullOrEmpty(step.text))
            {
                text.Text = step.text;
                text.Visibility = Visibility.Visible;
            }
            else
            {
                text.Visibility = Visibility.Hidden;
            }
        }

        private void Layout()
        {
            double w = map.ActualWidth;
            double h = map.ActualHeight;

            fullImage.Width = w;
            fullImage.Height = h;

            double margin = 100;
            double totalWidth = w - (margin * 2);

            double sideWidth = 36;
            double middleWidth = totalWidth - (sideWidth * 2);

            dlgLeft.Width = sideWidth;
            dlgLeft.HorizontalAlignment = HorizontalAlignment.Left;
            dlgLeft.VerticalAlignment = VerticalAlignment.Bottom;
            dlgLeft.Margin = new Thickness(margin, 0, 0, 120);

            dlgMid.Width = middleWidth;
            dlgMid.HorizontalAlignment = HorizontalAlignment.Left;
            dlgMid.VerticalAlignment = VerticalAlignment.Bottom;
            dlgMid.Margin = new Thickness(margin + sideWidth, 0, 0, 120);

            dlgRight.Width = sideWidth;
            dlgRight.HorizontalAlignment = HorizontalAlignment.Left;
            dlgRight.VerticalAlignment = VerticalAlignment.Bottom;
            dlgRight.Margin = new Thickness(margin + sideWidth + middleWidth, 0, 0, 120);

            text.MaxWidth = middleWidth - 40;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            text.VerticalAlignment = VerticalAlignment.Bottom;
            text.Margin = new Thickness(margin + sideWidth + 20, 0, 0, 150);

            continueText.HorizontalAlignment = HorizontalAlignment.Right;
            continueText.VerticalAlignment = VerticalAlignment.Bottom;
            continueText.Margin = new Thickness(0, 0, margin + 20, 90);
        }

        private void ShowUI()
        {
            fullImage.Visibility =
            dlgLeft.Visibility =
            dlgMid.Visibility =
            dlgRight.Visibility =
            text.Visibility =
            continueText.Visibility = Visibility.Visible;
        }

        private void HideUI()
        {
            fullImage.Visibility =
            dlgLeft.Visibility =
            dlgMid.Visibility =
            dlgRight.Visibility =
            text.Visibility =
            continueText.Visibility = Visibility.Hidden;
        }

        private void End()
        {
            IsActive = false;
            map.isPaused = false;

            HideUI();

            map.story.AdvanceStep();
        }

        public void Advance()
        {
            if (!IsActive) return;

            currentStepIndex++;

            if (currentStepIndex >= steps.Count)
            {
                End();
                return;
            }

            UpdateStep();
        }
    }
}