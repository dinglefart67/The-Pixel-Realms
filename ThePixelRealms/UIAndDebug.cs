using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public class UIAndDebug
    {
        private readonly dynamic map;

        public UIAndDebug(dynamic map)
        {
            this.map = map;
        }

        public void InitializeSpeechUi()
        {
            map.npcSpeech = new TextBlock
            {
                Foreground = Brushes.Black,
                FontSize = 16,
                Visibility = Visibility.Hidden
            };

            map.interactionHint = new TextBlock
            {
                Text = "Köszönj a lakosnak [E]",
                Foreground = Brushes.White,
                FontSize = 16,
                Visibility = Visibility.Hidden
            };

            BitmapImage leftImg = new BitmapImage(new Uri("Assets/UI/CommentBubble_Left.png", UriKind.Relative));
            BitmapImage midImg = new BitmapImage(new Uri("Assets/UI/CommentBubble_Middle.png", UriKind.Relative));
            BitmapImage rightImg = new BitmapImage(new Uri("Assets/UI/CommentBubble_Right.png", UriKind.Relative));

            map.npcBubbleLeft = new Image { Source = leftImg, Height = 44, Stretch = Stretch.Fill };
            map.npcBubbleMiddle = new Image { Source = midImg, Height = 44, Stretch = Stretch.Fill };
            map.npcBubbleRight = new Image { Source = rightImg, Height = 44, Stretch = Stretch.Fill };

            Panel.SetZIndex(map.npcBubbleLeft, 1992);
            Panel.SetZIndex(map.npcBubbleMiddle, 1993);
            Panel.SetZIndex(map.npcBubbleRight, 1994);

            Panel.SetZIndex(map.npcSpeech, 1995);
            Panel.SetZIndex(map.interactionHint, 2001);

            map.GameCanvas.Children.Add(map.npcBubbleLeft);
            map.GameCanvas.Children.Add(map.npcBubbleMiddle);
            map.GameCanvas.Children.Add(map.npcBubbleRight);

            map.GameCanvas.Children.Add(map.npcSpeech);
            map.GameCanvas.Children.Add(map.interactionHint);
        }

        public void InitializeBowAndAimUI()
        {
            map.bowChargeBarBg = new Rectangle
            {
                Width = 104,
                Height = 12,
                Fill = Brushes.Black,
                Visibility = Visibility.Hidden
            };

            map.bowChargeBarFill = new Rectangle
            {
                Width = 0,
                Height = 8,
                Fill = Brushes.Gold,
                Visibility = Visibility.Hidden
            };

            map.GameCanvas.Children.Add(map.bowChargeBarBg);
            map.GameCanvas.Children.Add(map.bowChargeBarFill);

            map.aimLineLeft = new Line
            {
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Visibility = Visibility.Hidden
            };

            map.aimLineRight = new Line
            {
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Visibility = Visibility.Hidden
            };

            map.aimDot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red,
                Visibility = Visibility.Hidden
            };

            Panel.SetZIndex(map.bowChargeBarBg, 1200);
            Panel.SetZIndex(map.bowChargeBarFill, 1201);
            Panel.SetZIndex(map.aimLineLeft, 1200);
            Panel.SetZIndex(map.aimLineRight, 1200);
            Panel.SetZIndex(map.aimDot, 1201);

            map.GameCanvas.Children.Add(map.aimLineLeft);
            map.GameCanvas.Children.Add(map.aimLineRight);
            map.GameCanvas.Children.Add(map.aimDot);
        }

        public void TogglePause()
        {
            map.isPaused = !map.isPaused;

            map.PauseOverlay.Visibility = map.isPaused
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (map.isPaused)
            {
                map.leftPressed = false;
                map.rightPressed = false;
                map.spacePressed = false;
            }
        }

        public void CheckDeath()
        {
            if (map.isDead)
                return;

            if (map.currentHp <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            map.isDead = true;
            map.isPaused = true;

            map.Player.Visibility = Visibility.Collapsed;
            map.Dead.Visibility = Visibility.Visible;

            map.leftPressed = false;
            map.rightPressed = false;
            map.spacePressed = false;
        }

        public void DrawBubble(Image left, Image middle, Image right, TextBlock text, double centerX, double worldY)
        {
            const double side = 12, height = 44, padding = 20;

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = Math.Ceiling(text.DesiredSize.Width);
            double totalWidth = textWidth + padding + (side * 2);

            double startX = centerX - map.cameraX - totalWidth / 2;
            double topY = worldY - 72;
            double middleWidth = totalWidth - (side * 2);

            left.Width = right.Width = side;
            middle.Width = middleWidth;

            left.Height = middle.Height = right.Height = height;

            Canvas.SetLeft(left, startX);
            Canvas.SetLeft(middle, startX + side);
            Canvas.SetLeft(right, startX + side + middleWidth);

            Canvas.SetTop(left, topY);
            Canvas.SetTop(middle, topY);
            Canvas.SetTop(right, topY);

            Canvas.SetLeft(text, centerX - map.cameraX - textWidth / 2);
            Canvas.SetTop(text, worldY - 61);

            left.Visibility = middle.Visibility = right.Visibility = text.Visibility = Visibility.Visible;
        }

        public void UpdateSpeech()
        {
            if (map.dialogueSystem != null && map.dialogueSystem.IsActive)
            {
                map.npcSpeech.Visibility = Visibility.Hidden;
                map.npcBubbleLeft.Visibility = Visibility.Hidden;
                map.npcBubbleMiddle.Visibility = Visibility.Hidden;
                map.npcBubbleRight.Visibility = Visibility.Hidden;
                return;
            }

            if (!map.isGreetingActive || map.currentNpc == null)
            {
                map.npcSpeech.Visibility = Visibility.Hidden;
                map.npcBubbleLeft.Visibility = Visibility.Hidden;
                map.npcBubbleMiddle.Visibility = Visibility.Hidden;
                map.npcBubbleRight.Visibility = Visibility.Hidden;
                return;
            }

            map.npcSpeechTimer -= map.deltaTime;

            double centerX = map.currentNpc.WorldX + (map.currentNpc.Visual.Width / 2.0);

            DrawBubble(
                map.npcBubbleLeft,
                map.npcBubbleMiddle,
                map.npcBubbleRight,
                map.npcSpeech,
                centerX,
                map.currentNpc.WorldY
            );

            if (map.npcSpeechTimer <= 0)
            {
                map.isGreetingActive = false;
            }
        }

        public List<string> LoadGreetings()
        {
            var lines = new List<string>();

            try
            {
                using (var reader = new System.IO.StreamReader("Assets/Dialog/greetings.txt", Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                            lines.Add(line);
                    }
                }
            }
            catch
            {
                lines.Add("Hiba");
            }

            return lines;
        }

        public void StartGreeting(Npc npc)
        {
            var lines = LoadGreetings();
            if (lines.Count < 12) return;

            map.npcSpeech.Text = lines[new Random().Next(6, 12)];

            map.currentNpc = npc;
            map.npcSpeechTimer = 2;
            map.isGreetingActive = true;

            map.npcSpeech.Visibility = Visibility.Visible;
        }

        public void HandleNpcInteraction()
        {
            Npc nearest = null;
            double minDist = double.MaxValue;

            foreach (var npc in map.npcs)
            {
                double dist = Math.Abs(map.playerWorldX - npc.WorldX);

                if (dist < 120 && dist < minDist)
                {
                    nearest = npc;
                    minDist = dist;
                }
            }

            if (nearest == null)
            {
                map.interactionHint.Visibility = Visibility.Hidden;
                return;
            }

            bool hasDialogue =
                map.talkSequence.ContainsKey(map.story.CurrentStep) &&
                map.talkSequence[map.story.CurrentStep] == nearest.Type;
            bool canGreet = map.greetingAllowedTypes.Contains(nearest.Type);

            if (hasDialogue)
                map.interactionHint.Text = map.dialogueSystem.GetHintText(nearest.Type);
            else if (canGreet)
                map.interactionHint.Text = "Köszönj a lakosnak [E]";
            else
                map.interactionHint.Text = "";

            map.interactionHint.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double npcCenterX = nearest.WorldX + (nearest.Visual.Width / 2.0);

            Canvas.SetLeft(map.interactionHint,
                npcCenterX - map.cameraX - (map.interactionHint.DesiredSize.Width / 2.0));

            Canvas.SetTop(map.interactionHint, nearest.WorldY - 28);

            map.interactionHint.Visibility =
                (nearest.InteractCooldown <= 0 && (hasDialogue || canGreet) && !map.dialogueSystem.IsActive)
                ? Visibility.Visible
                : Visibility.Hidden;

            if (!map.input.EPressed)
                return;

            map.input.EPressed = false;

            if (map.dialogueSystem.IsActive)
            {
                map.dialogueSystem.Advance();
                return;
            }

            if (nearest.InteractCooldown > 0)
                return;

            if (hasDialogue)
            {
                map.dialogueSystem.Start(nearest);
                map.interactionHint.Visibility = Visibility.Hidden;
                nearest.InteractCooldown = 0.3;
                return;
            }

            if (canGreet)
            {
                StartGreeting(nearest);
                map.interactionHint.Visibility = Visibility.Hidden;
                nearest.InteractCooldown = 10.0;
            }
        }

        public void LoadPlayerImage()
        {
            try
            {
                string imagePath = "Assets/Sprites/Borien/Borien_Stand_1.png";

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                map.Player.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kép betöltése sikertelen: {ex.Message}");
                map.Player.Source = null;
            }
        }

        public void UpdateHpBar()
        {
            double hpPercent = map.currentHp / map.maxHp;
            hpPercent = Math.Max(0, Math.Min(1, hpPercent));

            double maxWidth = 320;
            map.HpBarFill.Width = maxWidth * hpPercent;
        }

        public void UpdateShieldBar()
        {
            double percent = map.currentShield / Level1.maxShield;
            if (percent < 0) percent = 0;
            if (percent > 1) percent = 1;

            map.ShieldBarFill.Width = 320 * percent;
        }

        public void UpdateFPS()
        {
            map.frameCount++;

            if ((DateTime.Now - map.lastFpsUpdate).TotalSeconds >= 0.5)
            {
                map.fps = map.frameCount / (DateTime.Now - map.lastFpsUpdate).TotalSeconds;
                map.frameCount = 0;
                map.lastFpsUpdate = DateTime.Now;
            }
        }

        public void UpdateDebugInfo()
        {
            List<string> debugInfo = map.debugInfo;

            //debugInfo.Add($"FPS: {map.fps:F1}");
            //debugInfo.Add($"Position: ({System.Windows.Controls.Canvas.GetLeft(map.Player):F0}, {System.Windows.Controls.Canvas.GetTop(map.Player):F0})");
            //debugInfo.Add($"On Ground: {map.isOnGround}");
            //debugInfo.Add($"Jumping: {map.isJumping}");
            //debugInfo.Add($"Facing: {(map.isFacingRight ? "Right" : "Left")}");
            //debugInfo.Add($"Keys: A/D={map.leftPressed}/{map.rightPressed}, Space={map.spacePressed}");

            //map.DebugText.Text = string.Join("\n", debugInfo);
        }

        public void ToggleDebugVisibility()
        {
            map.DebugText.Visibility = map.DebugText.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }
}