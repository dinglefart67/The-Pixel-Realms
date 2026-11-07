using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public class PlayerMovement
    {
        private readonly IGameState map;

        public PlayerMovement(IGameState map)
        {
            this.map = map;
        }

        public void HandleMovement()
        {
            if (map.IsPlayerStunned || map.isChargingBow || map.isMeleeAnimating || map.isShielding)
            {
                map.playerVelocityX = 0;
                return;
            }

            map.playerVelocityX = 0;

            double speed = map.shiftPressed
                ? map.playerRunSpeed
                : map.playerWalkSpeed;

            if (map.leftPressed)
                map.playerVelocityX = -speed;

            if (map.rightPressed)
                map.playerVelocityX = speed;

            if (map.spacePressed && map.isOnGround)
            {
                map.playerVelocityY = -map.jumpForce;
                map.isJumping = true;
                map.isOnGround = false;
            }
        }

        public void UpdatePlayerPosition()
        {
            map.playerWorldX += map.playerVelocityX * map.deltaTime;
            map.playerWorldY += map.playerVelocityY * map.deltaTime;

            if (map.playerWorldX < 0) map.playerWorldX = 0;
            if (map.playerWorldX + map.Player.ActualWidth > map.WorldWidth)
                map.playerWorldX = map.WorldWidth - map.Player.ActualWidth;

            Canvas.SetLeft(map.Player, map.playerWorldX - map.cameraX);
            Canvas.SetTop(map.Player, map.playerWorldY);
        }

        public void UpdatePlayerFacingDirection()
        {
            if (map.playerVelocityX > 0.1 && !map.isFacingRight)
            {
                map.PlayerScaleTransform.ScaleX = 1;
                map.isFacingRight = true;
            }
            else if (map.playerVelocityX < -0.1 && map.isFacingRight)
            {
                map.PlayerScaleTransform.ScaleX = -1;
                map.isFacingRight = false;
            }
        }

        public void UpdateWalkAnimation()
        {
            if (map.isMeleeAnimating)
                return;

            if (map.walkFrames == null || map.walkFrames.Length == 0)
                return;

            bool walkAllowed = map.isOnGround && !map.isChargingBow && !map.isMeleeAnimating;

            bool isWalkingNow = walkAllowed &&
                                Math.Abs(map.playerVelocityX) > 0.1 &&
                                (map.leftPressed || map.rightPressed);

            if (map.wasWalking && !isWalkingNow)
            {
                map.walkFrameTimer = 0;
                map.walkFrameIndex = 0;

                if (map.idleFrames != null && map.idleFrames.Length > 0)
                {
                    map.idleFrameIndex = 0;
                    map.idleFrameTimer = 0;
                    map.Player.Source = map.idleFrames[0];
                }
            }

            if (!isWalkingNow)
            {
                map.wasWalking = false;
                return;
            }

            map.wasWalking = true;

            map.walkFrameTimer += map.deltaTime;

            if (map.walkFrameTimer >= map.walkFrameDuration)
            {
                map.walkFrameTimer -= map.walkFrameDuration;

                map.walkFrameIndex++;
                if (map.walkFrameIndex >= map.walkFrames.Length)
                    map.walkFrameIndex = 0;
            }

            map.Player.Source = map.walkFrames[map.walkFrameIndex];
        }

        public void LoadJumpAirFrame()
        {
            string path = "Assets/Sprites/Borien/Borien_Jump.png";

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Relative);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            map.jumpAirFrame = bmp;
        }
    }
}
