using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    internal sealed class PropManager
    {
        private readonly IGameState map;

        private readonly List<(Image img, double x, double y)> apples = new();
        private readonly List<(Image img, double x, double y)> shields = new();

        private readonly BitmapImage appleBmp;
        private readonly BitmapImage shieldBmp;

        public PropManager(IGameState map)
        {
            this.map = map;

            appleBmp = Load("Assets/Props/Apple.png");
            shieldBmp = Load("Assets/Props/Shield.png");
        }

        private BitmapImage Load(string path)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Relative);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        public void SpawnApple(double x, double y)
        {
            var img = new Image
            {
                Width = 38,
                Height = 38,
                Source = appleBmp
            };

            Panel.SetZIndex(img, 1200);
            map.GameCanvas.Children.Add(img);

            apples.Add((img, x, y));
        }

        public void UpdateApples()
        {
            for (int i = apples.Count - 1; i >= 0; i--)
            {
                var (img, x, y) = apples[i];

                Canvas.SetLeft(img, x - map.cameraX);
                Canvas.SetTop(img, y);

                Rect playerRect = map.GetPlayerHitboxRect();
                Rect itemRect = new Rect(x, y, img.Width, img.Height);

                if (playerRect.IntersectsWith(itemRect))
                {
                    map.currentHp += map.appleHealAmount;
                    if (map.currentHp > map.maxHp)
                        map.currentHp = map.maxHp;

                    map.uiAndDebug.UpdateHpBar();

                    map.GameCanvas.Children.Remove(img);
                    apples.RemoveAt(i);
                }
            }
        }

        public void SpawnShield(double x, double y)
        {
            var img = new Image
            {
                Width = 46,
                Height = 52,
                Source = shieldBmp
            };

            Panel.SetZIndex(img, 1200);
            map.GameCanvas.Children.Add(img);

            shields.Add((img, x, y));
        }

        public void UpdateShields()
        {
            map.uiAndDebug.UpdateShieldBar();

            for (int i = shields.Count - 1; i >= 0; i--)
            {
                var (img, x, y) = shields[i];

                Canvas.SetLeft(img, x - map.cameraX);
                Canvas.SetTop(img, y);

                Rect playerRect = map.GetPlayerHitboxRect();
                Rect itemRect = new Rect(x, y, img.Width, img.Height);

                if (playerRect.IntersectsWith(itemRect))
                {
                    map.currentShield = map.maxShield;
                    map.ShieldBarFill.Width = 320;

                    map.GameCanvas.Children.Remove(img);
                    shields.RemoveAt(i);
                }
            }
        }
    }
}