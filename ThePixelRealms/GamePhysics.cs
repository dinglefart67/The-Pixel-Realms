using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public class GamePhysics
    {
        private readonly IGameState map;

        public GamePhysics(IGameState map)
        {
            this.map = map;
        }

        public void ApplyPhysics()
        {
            if (!map.isOnGround)
            {
                map.playerVelocityY += map.gravity * map.deltaTime;
            }

            if (map.playerVelocityY > 800) map.playerVelocityY = 800;
        }

        public void CheckCollisions()
        {
            map.isOnGround = false;

            Rect playerRect = map.GetPlayerHitboxRect();
            double playerWidth = playerRect.Width;
            double playerHeight = playerRect.Height;

            if (playerRect.Bottom >= map.GameCanvas.ActualHeight)
            {
                map.playerWorldY = map.GameCanvas.ActualHeight - playerHeight;
                map.playerVelocityY = 0;
                map.isOnGround = true;
                return;
            }

            ResolvePlatformCollision(map.Ground, map.GroundX);
            foreach (var platform in map.platforms)
            {
                double worldX = (double)platform.Tag;
                ResolvePlatformCollision(platform, worldX);
            }
        }

        public void ResolvePlatformCollision(Rectangle platform, double platformWorldX)
        {
            Rect curr = map.GetPlayerHitboxRect();

            Rect plat = new Rect(
                platformWorldX,
                Canvas.GetTop(platform),
                platform.Width,
                platform.Height
            );

            double dx = map.playerVelocityX * map.deltaTime;
            double dy = map.playerVelocityY * map.deltaTime;

            Rect next = new Rect(
                curr.X + dx,
                curr.Y + dy,
                curr.Width,
                curr.Height
            );

            bool horizOverlapCurr = curr.Right > plat.Left && curr.Left < plat.Right;
            bool horizOverlapNext = next.Right > plat.Left && next.Left < plat.Right;

            bool vertOverlapNext = next.Bottom > plat.Top && next.Top < plat.Bottom;

            if (dx > 0 && vertOverlapNext && curr.Right <= plat.Left && next.Right >= plat.Left)
            {
                double hitboxLeftTarget = plat.Left - curr.Width;
                map.playerWorldX = hitboxLeftTarget - Level1.PlayerHitboxOffsetX;
                map.playerVelocityX = 0;
                return;
            }

            if (dx < 0 && vertOverlapNext && curr.Left >= plat.Right && next.Left <= plat.Right)
            {
                double hitboxLeftTarget = plat.Right;
                map.playerWorldX = hitboxLeftTarget - Level1.PlayerHitboxOffsetX;
                map.playerVelocityX = 0;
                return;
            }

            if (dy > 0 && horizOverlapNext && curr.Bottom <= plat.Top && next.Bottom >= plat.Top)
            {
                map.playerWorldY = plat.Top - curr.Height;
                map.playerVelocityY = 0;
                map.isOnGround = true;
                return;
            }

            if (dy < 0 && horizOverlapNext && curr.Top >= plat.Bottom && next.Top <= plat.Bottom)
            {
                map.playerWorldY = plat.Bottom;
                map.playerVelocityY = 0;
                return;
            }

            const double groundEpsilon = 6.0;

            if (dy == 0 && horizOverlapCurr &&
                curr.Bottom >= plat.Top && curr.Bottom <= plat.Top + groundEpsilon)
            {
                map.playerWorldY = plat.Top - curr.Height;
                map.playerVelocityY = 0;
                map.isOnGround = true;
                return;
            }
        }

        public void UpdateCamera()
        {
            double screenX = map.playerWorldX - map.cameraX;

            if (screenX > map.screenRightLimit)
                map.cameraX = map.playerWorldX - map.screenRightLimit;
            else if (screenX < map.screenLeftLimit)
                map.cameraX = map.playerWorldX - map.screenLeftLimit;

            if (map.cameraX < 0)
                map.cameraX = 0;

            double maxCameraX = map.WorldWidth - map.GameCanvas.ActualWidth;
            if (maxCameraX < 0)
                maxCameraX = 0;

            if (map.cameraX > maxCameraX)
                map.cameraX = maxCameraX;

            foreach (UIElement element in map.GameCanvas.Children)
            {
                if (element == map.Player)
                    continue;

                if (element is FrameworkElement fe && fe.Tag is double worldX)
                {
                    ParallaxSystem.UpdateElement(fe, worldX, map.cameraX);
                }
            }
        }

        public Rect GetPlatformRect(Rectangle platform, double worldX)
        {
            return new Rect(
                worldX,
                Canvas.GetTop(platform),
                platform.Width,
                platform.Height
            );
        }
    }
}
