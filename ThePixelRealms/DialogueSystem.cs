using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThePixelRealms;

namespace ThePixelRealms
{
    public class DialogueSystem
    {
        public Npc CurrentNpc => currentNpc;

        private readonly IGameState map;

        private readonly TextBlock dialogueText;
        private readonly TextBlock nameText;

        private readonly Image dlgLeft, dlgMid, dlgRight, dlgName;

        private readonly List<string> currentLines = new List<string>();
        private int currentLineIndex = 0;
        private Npc currentNpc = null;
        private readonly TextBlock continueText;

        public bool IsActive { get; private set; } = false;

        public DialogueSystem(IGameState map)
        {
            this.map = map;

            dialogueText = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 24,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Hidden
            };

            nameText = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 24,
                Visibility = Visibility.Hidden
            };

            continueText = new TextBlock
            {
                Foreground = Brushes.Black,
                FontSize = 20,
                Text = "Tovább [E]",
                Visibility = Visibility.Hidden
            };

            Panel.SetZIndex(continueText, 3001);
            map.GameCanvas.Children.Add(continueText);

            dlgLeft = new Image
            {
                Source = Load("Assets/UI/DialogBubble_Left.png"),
                Height = 88,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            dlgMid = new Image
            {
                Source = Load("Assets/UI/DialogBubble_Middle.png"),
                Height = 88,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            dlgRight = new Image
            {
                Source = Load("Assets/UI/DialogBubble_Right.png"),
                Height = 88,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            dlgName = new Image
            {
                Source = Load("Assets/UI/DialogBubble_Name.png"),
                Height = 48,
                Width = 352,
                Stretch = Stretch.Fill,
                Visibility = Visibility.Hidden
            };

            Panel.SetZIndex(dlgLeft, 2900);
            Panel.SetZIndex(dlgMid, 2901);
            Panel.SetZIndex(dlgRight, 2902);
            Panel.SetZIndex(dlgName, 2903);
            Panel.SetZIndex(nameText, 2904);
            Panel.SetZIndex(dialogueText, 3000);

            map.GameCanvas.Children.Add(dlgLeft);
            map.GameCanvas.Children.Add(dlgMid);
            map.GameCanvas.Children.Add(dlgRight);
            map.GameCanvas.Children.Add(dlgName);
            map.GameCanvas.Children.Add(nameText);
            map.GameCanvas.Children.Add(dialogueText);
        }

        private BitmapImage Load(string path)
        {
            return new BitmapImage(new Uri(path, UriKind.Relative));
        }

        public bool SupportsNpc(NpcType type)
        {
            return type == NpcType.Eldon || type == NpcType.Mira || type == NpcType.Dorin || type == NpcType.Noril || type == NpcType.Guard3;
        }

        public string GetHintText(NpcType type)
        {
            switch (type)
            {
                case NpcType.Eldon: return "Beszélj Eldon-nal [E]";
                case NpcType.Mira: return "Beszélj Mira-val [E]";
                case NpcType.Dorin: return "Beszélj Dorin-nal [E]";
                case NpcType.Noril: return "Beszélj Noril-nal [E]";
                case NpcType.Guard3: return "Beszélj a városi őrrel [E]";
                default: return "";
            }
        }

        public void Start(Npc npc)
        {
            string filePath = GetDialoguePath(npc.Type);
            if (string.IsNullOrEmpty(filePath)) return;

            currentLines.Clear();

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                            currentLines.Add(line);
                    }
                }
            }
            catch
            {
                currentLines.Add("Hiba a fájl betöltésekor.");
            }

            if (currentLines.Count == 0) return;

            currentNpc = npc;
            currentLineIndex = 0;
            IsActive = true;

            ShowUI();
            UpdateDialogueText();
        }

        public void Advance()
        {
            if (!IsActive) return;

            currentLineIndex++;

            if (currentLineIndex >= currentLines.Count)
            {
                End();
                return;
            }

            UpdateDialogueText();
        }

        public void Update()
        {
            if (!IsActive) return;

            Layout();
        }

        public void End()
        {
            var finishedNpc = currentNpc;

            IsActive = false;
            currentNpc = null;
            currentLineIndex = 0;
            currentLines.Clear();

            HideUI();

            if (finishedNpc != null)
            {
                map.story.AdvanceStep();
            }
        }

        private void Layout()
        {
            double margin = 200;
            double totalWidth = map.ActualWidth - (margin * 2);

            double sideWidth = 36;
            double middleWidth = totalWidth - (sideWidth * 2);

            double x = margin;
            double y = 100;

            dlgLeft.Width = sideWidth;
            Canvas.SetLeft(dlgLeft, x);
            Canvas.SetTop(dlgLeft, y);

            dlgMid.Width = middleWidth;
            Canvas.SetLeft(dlgMid, x + sideWidth);
            Canvas.SetTop(dlgMid, y);

            dlgRight.Width = sideWidth;
            Canvas.SetLeft(dlgRight, x + sideWidth + middleWidth);
            Canvas.SetTop(dlgRight, y);

            Canvas.SetLeft(dlgName, x + 10);
            Canvas.SetTop(dlgName, y - 30);

            nameText.Text = GetSpeakerName(currentNpc.Type);

            nameText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = nameText.DesiredSize.Width;

            double nameCenterX = x + 10 + (352 / 2.0);

            Canvas.SetLeft(nameText, nameCenterX - textWidth / 2.0);
            Canvas.SetTop(nameText, y - 25);

            Canvas.SetLeft(dialogueText, x + sideWidth + 30);
            Canvas.SetTop(dialogueText, y + 25);
            dialogueText.MaxWidth = middleWidth - 40;

            continueText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double contWidth = continueText.DesiredSize.Width;

            Canvas.SetLeft(
                continueText,
                x + sideWidth + middleWidth - contWidth - 20
            );

            Canvas.SetTop(
                continueText,
                y + 88
            );

        }

        private void ShowUI()
        {
            dlgLeft.Visibility =
            dlgMid.Visibility =
            dlgRight.Visibility =
            dlgName.Visibility =
            nameText.Visibility =
            dialogueText.Visibility =
            continueText.Visibility = Visibility.Visible;
        }

        private void HideUI()
        {
            dlgLeft.Visibility =
            dlgMid.Visibility =
            dlgRight.Visibility =
            dlgName.Visibility =
            nameText.Visibility =
            dialogueText.Visibility =
            continueText.Visibility = Visibility.Hidden;
        }

        private void UpdateDialogueText()
        {
            if (currentNpc == null) return;

            dialogueText.Text =
                $"{currentLines[currentLineIndex]}";
        }

        private string GetDialoguePath(NpcType type)
        {
            int step = map.story.CurrentStep;

            switch (type)
            {
                case NpcType.Eldon:
                    if (step < 5)
                        return "Assets/Dialog/Eldon1.txt";
                    else if (step < 9)
                        return "Assets/Dialog/Eldon2.txt";
                    else
                        return "Assets/Dialog/Eldon3.txt";

                case NpcType.Mira:
                    return "Assets/Dialog/Mira1.txt";

                case NpcType.Dorin:
                    return "Assets/Dialog/Dorin1.txt";

                case NpcType.Noril:
                    return "Assets/Dialog/Noril1.txt";

                default:
                    return null;
            }
        }

        private string GetSpeakerName(NpcType type)
        {
            switch (type)
            {
                case NpcType.Eldon: return "Eldon";
                case NpcType.Mira: return "Mira";
                case NpcType.Dorin: return "Dorin";
                case NpcType.Noril: return "Noril";
                case NpcType.Guard3: return "Városi őr";
                default: return type.ToString();
            }
        }
    }
}