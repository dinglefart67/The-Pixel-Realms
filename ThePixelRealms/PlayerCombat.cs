using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public class PlayerCombat
    {
        private readonly IGameState map;
        private readonly EnemyCombat enemyCombat;
        private readonly GamePhysics physics;

        public PlayerCombat(IGameState map, EnemyCombat enemyCombat, GamePhysics physics)
        {
            this.map = map;
            this.enemyCombat = enemyCombat;
            this.physics = physics;
        }

        public void UpdateDamageCooldown()
        {
            if (map.invulnerableVisualTimer > 0)
            {
                map.invulnerableVisualTimer -= map.deltaTime;
                if (map.invulnerableVisualTimer <= 0)
                {
                    map.invulnerableVisualTimer = 0;
                    map.Player.Opacity = 1.0;
                }
            }
        }

        public void DamagePlayer(double amount, Enemy sourceEnemy = null)
        {
            if (!map.canTakeDamage || map.isDead)
                return;

            if (map.isShielding && map.currentShield > 0)
            {
                map.currentShield -= amount;
                if (map.currentShield < 0) map.currentShield = 0;

                if (map.currentShield == 0)
                {
                    map.isShielding = false;

                    if (map.idleFrames != null && map.idleFrames.Length > 0)
                    {
                        map.idleFrameIndex = 0;
                        map.idleFrameTimer = 0;
                        map.Player.Source = map.idleFrames[0];
                    }
                }

                map.invulnerableVisualTimer = map.invulnerableVisualTime;
                map.Player.Opacity = 0.7;

                map.playerStunTimer = map.playerStunDuration;

                if (sourceEnemy != null)
                {
                    if (sourceEnemy.WorldX < map.playerWorldX)
                        map.playerVelocityX = map.playerKnockbackX;
                    else
                        map.playerVelocityX = -map.playerKnockbackX;

                }

                return;
            }
            map.currentHp -= amount;
            if (map.currentHp < 0)
                map.currentHp = 0;

            map.invulnerableVisualTimer = map.invulnerableVisualTime;
            map.Player.Opacity = 0.5;

            map.playerStunTimer = map.playerStunDuration;

            if (sourceEnemy != null)
            {
                if (sourceEnemy.WorldX < map.playerWorldX)
                    map.playerVelocityX = map.playerKnockbackX;
                else
                    map.playerVelocityX = -map.playerKnockbackX;

                map.playerVelocityY = -map.playerKnockbackY;
            }
        }

        public void UpdatePlayerStun()
        {
            if (map.playerStunTimer > 0)
            {
                map.playerStunTimer -= map.deltaTime;
                if (map.playerStunTimer <= 0)
                {
                    map.playerStunTimer = 0;
                }
            }
        }

        public void HandlePlayerAttack()
        {
            if (!map.attackPressed || !map.canMeleeAttack || map.isMeleeAnimating)
                return;

            map.isMeleeAnimating = true;
            map.meleeFrameIndex = 0;
            map.meleeFrameTimer = 0;
            map.Player.Source = map.meleeFrames[0];

            map.canMeleeAttack = false;
            map.meleeCooldownTimer = map.meleeCooldown;

            foreach (var enemy in map.enemies)
            {
                Rect pr = map.GetPlayerHitboxRect();
                double playerCenterX = pr.X + pr.Width / 2.0;

                double enemyCenterX = enemy.WorldX + enemy.Visual.Width / 2.0;

                double dx = enemyCenterX - playerCenterX;
                double dy = Math.Abs((enemy.WorldY + enemy.Visual.Height / 2.0) - (pr.Y + pr.Height / 2.0));

                bool enemyInFront =
                    (map.isFacingRight && dx > 0 && dx < map.attackRange) ||
                    (!map.isFacingRight && dx < 0 && -dx < map.attackRange);

                if (enemyInFront && dy < 50)
                {
                    enemyCombat.DamageEnemy(enemy);
                    break;
                }
            }
        }

        public void UpdateMeleeCooldown()
        {
            if (!map.canMeleeAttack)
            {
                map.meleeCooldownTimer -= map.deltaTime;
                if (map.meleeCooldownTimer <= 0)
                {
                    map.canMeleeAttack = true;
                    map.meleeCooldownTimer = 0;
                }
            }
        }

        public void LoadMeleeFrames()
        {
            map.meleeFrames = new BitmapImage[3];

            string[] paths =
            {
                "Assets/Sprites/Borien/Borien_Sword1_1.png",
                "Assets/Sprites/Borien/Borien_Sword1_2.png",
                "Assets/Sprites/Borien/Borien_Sword1_3.png"
            };

            for (int i = 0; i < paths.Length; i++)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(paths[i], UriKind.Relative);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                map.meleeFrames[i] = bmp;
            }
        }

        public void UpdateMeleeAnimation()
        {
            if (!map.isMeleeAnimating || map.meleeFrames == null)
                return;

            map.meleeFrameTimer += map.deltaTime;

            if (map.meleeFrameTimer >= map.meleeFrameDuration)
            {
                map.meleeFrameTimer -= map.meleeFrameDuration;
                map.meleeFrameIndex++;

                if (map.meleeFrameIndex >= map.meleeFrames.Length)
                {
                    map.isMeleeAnimating = false;
                    map.meleeFrameIndex = 0;

                    if (map.idleFrames != null && map.idleFrames.Length > 0)
                    {
                        map.idleFrameIndex = 0;
                        map.idleFrameTimer = 0;
                        map.Player.Source = map.idleFrames[0];
                    }

                    return;
                }

                map.Player.Source = map.meleeFrames[map.meleeFrameIndex];
            }
        }

        public void UpdateBowCharge()
        {
            if (!map.canShootBow)
                return;

            if (map.bowKeyPressed && !map.IsPlayerStunned)
            {
                if (!map.isChargingBow)
                {
                    map.isChargingBow = true;
                    map.bowChargeTime = 0;
                    map.currentChargeStep = 0;

                    map.bowChargeBarBg.Visibility = Visibility.Visible;
                    map.bowChargeBarFill.Visibility = Visibility.Visible;
                }

                map.bowChargeTime += map.deltaTime;

                int step = (int)(map.bowChargeTime / map.ChargeStepTime);
                map.currentChargeStep = Math.Min(step, map.MaxChargeSteps);

                map.bowChargeBarFill.Width = 20 * map.currentChargeStep;

                Canvas.SetLeft(map.bowChargeBarBg, Canvas.GetLeft(map.Player) - 20);
                Canvas.SetTop(map.bowChargeBarBg, Canvas.GetTop(map.Player) - 20);

                Canvas.SetLeft(map.bowChargeBarFill, Canvas.GetLeft(map.Player) - 18);
                Canvas.SetTop(map.bowChargeBarFill, Canvas.GetTop(map.Player) - 18);
            }
        }
        
        public void ReleaseArrow()
        {
            if (!map.isChargingBow)
                return;

            map.canShootBow = false;
            map.bowShootCooldownTimer = map.bowShootCooldown;

            map.isChargingBow = false;
            map.bowKeyPressed = false;

            map.bowChargeBarBg.Visibility = Visibility.Hidden;
            map.bowChargeBarFill.Visibility = Visibility.Hidden;

            map.lastBowFrameIndex = -1;

            if (map.idleFrames != null && map.idleFrames.Length > 0)
            {
                map.idleFrameIndex = 0;
                map.idleFrameTimer = 0;
                map.Player.Source = map.idleFrames[0];
            }

            double[] chargeLevels = { 0.2, 0.4, 0.6, 0.8, 1.0 };
            double[] damageLevels = { 8, 16, 32, 48, 64 };

            int index = Math.Clamp(map.currentChargeStep - 1, 0, chargeLevels.Length - 1);

            double power = chargeLevels[index];
            double damage = damageLevels[index];

            double speedX = 600 + 1800 * power;
            double baseSpeedY = -80 * power;
            double aimInfluence = 400 * map.currentAimRatioY;

            double speedY = baseSpeedY + aimInfluence;

            if (!map.isFacingRight)
                speedX = -speedX;

            Arrow arrow = new Arrow(
                map.GameCanvas,
                map.playerWorldX + (map.isFacingRight ? map.Player.ActualWidth : -10),
                map.playerWorldY + map.Player.ActualHeight / 2,
                speedX,
                speedY,
                damage
            );

            Panel.SetZIndex(arrow.Visual, 950);

            map.arrows.Add(arrow);
        }

        public void UpdateArrows()
        {
            for (int i = map.arrows.Count - 1; i >= 0; i--)
            {
                var arrow = map.arrows[i];

                arrow.Update(map.deltaTime);

                Canvas.SetLeft(arrow.Visual, arrow.WorldX - map.cameraX);
                Canvas.SetTop(arrow.Visual, arrow.WorldY);

                Rect arrowRect = arrow.GetRect();

                if (arrow.WorldX < 0 || arrow.WorldX > map.WorldWidth ||
                    arrow.WorldY > map.GameCanvas.ActualHeight)
                {
                    map.GameCanvas.Children.Remove(arrow.Visual);
                    map.arrows.RemoveAt(i);
                    continue;
                }

                bool hitEnemy = false;
                foreach (var enemy in map.enemies)
                {
                    Rect enemyRect = new Rect(
                        enemy.WorldX,
                        enemy.WorldY,
                        enemy.Visual.Width,
                        enemy.Visual.Height
                    );

                    if (arrowRect.IntersectsWith(enemyRect))
                    {
                        enemyCombat.DamageEnemy(enemy, arrow.Damage);
                        hitEnemy = true;
                        break;
                    }
                }

                if (hitEnemy)
                {
                    map.GameCanvas.Children.Remove(arrow.Visual);
                    map.arrows.RemoveAt(i);
                    continue;
                }

                if (arrowRect.IntersectsWith(physics.GetPlatformRect(map.Ground, map.GroundX)))
                {
                    map.GameCanvas.Children.Remove(arrow.Visual);
                    map.arrows.RemoveAt(i);
                }
            }
        }

        public void UpdateAimUI()
        {
            if (!map.isChargingBow)
            {
                map.aimLineLeft.Visibility = Visibility.Hidden;
                map.aimLineRight.Visibility = Visibility.Hidden;
                map.aimDot.Visibility = Visibility.Hidden;
                return;
            }

            double playerScreenX = map.playerWorldX - map.cameraX;
            double playerCenterY = map.playerWorldY + map.Player.ActualHeight / 2;

            double lineTop = playerCenterY - map.aimLineHeight / 2;
            double lineBottom = playerCenterY + map.aimLineHeight / 2;

            map.aimLineLeft.X1 = playerScreenX - 30;
            map.aimLineLeft.X2 = map.aimLineLeft.X1;
            map.aimLineLeft.Y1 = lineTop;
            map.aimLineLeft.Y2 = lineBottom;

            map.aimLineRight.X1 = playerScreenX + map.Player.ActualWidth + 30;
            map.aimLineRight.X2 = map.aimLineRight.X1;
            map.aimLineRight.Y1 = lineTop;
            map.aimLineRight.Y2 = lineBottom;

            Point mouse = Mouse.GetPosition(map.GameCanvas);

            bool mouseOnRight = mouse.X > playerScreenX + map.Player.ActualWidth / 2;

            map.aimLineLeft.Visibility = mouseOnRight ? Visibility.Hidden : Visibility.Visible;
            map.aimLineRight.Visibility = mouseOnRight ? Visibility.Visible : Visibility.Hidden;

            double clampedY = Math.Clamp(mouse.Y, lineTop, lineBottom);

            double dotX = mouseOnRight
                ? map.aimLineRight.X1
                : map.aimLineLeft.X1;

            Canvas.SetLeft(map.aimDot, dotX - map.aimDot.Width / 2);
            Canvas.SetTop(map.aimDot, clampedY - map.aimDot.Height / 2);

            double centerY = (lineTop + lineBottom) / 2;
            double halfHeight = map.aimLineHeight / 2;

            map.currentAimRatioY = (clampedY - centerY) / halfHeight;
            map.currentAimRatioY = Math.Clamp(map.currentAimRatioY, -1.0, 1.0);

            map.aimDot.Visibility = Visibility.Visible;

            if (mouseOnRight && !map.isFacingRight)
            {
                map.PlayerScaleTransform.ScaleX = 1;
                map.isFacingRight = true;
            }
            else if (!mouseOnRight && map.isFacingRight)
            {
                map.PlayerScaleTransform.ScaleX = -1;
                map.isFacingRight = false;
            }
        }

        public void LoadIdleFrames()
        {
            map.idleFrames = new BitmapImage[4];

            for (int i = 0; i < 4; i++)
            {
                string path = $"Assets/Sprites/Borien/Borien_Stand_{i + 1}.png";

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Relative);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                map.idleFrames[i] = bmp;
            }

            map.Player.Source = map.idleFrames[0];
        }

        public void UpdateIdleAnimation()
        {
            if (map.isMeleeAnimating)
                return;

            if (map.idleFrames == null || map.idleFrames.Length == 0)
                return;

            bool isIdle =
                Math.Abs(map.playerVelocityX) < 0.01 &&
                Math.Abs(map.playerVelocityY) < 0.01 &&
                !map.leftPressed && !map.rightPressed &&
                !map.attackPressed &&
                !map.isChargingBow &&
                !map.IsPlayerStunned &&
                map.canMeleeAttack &&
                map.isOnGround;

            if (!isIdle)
            {
                map.idleFrameTimer = 0;
                map.idleFrameIndex = 0;
                return;
            }

            map.idleFrameTimer += map.deltaTime;

            double currentDuration = map.idleFrameDurations[map.idleFrameIndex];

            if (map.idleFrameTimer >= currentDuration)
            {
                map.idleFrameTimer -= currentDuration;

                map.idleFrameIndex++;
                if (map.idleFrameIndex >= map.idleFrames.Length)
                    map.idleFrameIndex = 0;

                map.Player.Source = map.idleFrames[map.idleFrameIndex];
            }
        }

        public void LoadWalkFrames()
        {
            map.walkFrames = new BitmapImage[4];

            for (int i = 0; i < 4; i++)
            {
                string path = $"Assets/Sprites/Borien/Borien_Walk_{i + 1}.png";

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Relative);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                map.walkFrames[i] = bmp;
            }
        }

        public void UpdateJumpAirSprite()
        {
            if (map.jumpAirFrame == null)
                return;

            if (map.isOnGround)
                return;

            if (map.isChargingBow || map.isMeleeAnimating)
                return;

            map.Player.Source = map.jumpAirFrame;
        }

        public void LoadBowFrames()
        {
            map.bowFrames = new BitmapImage[4];

            for (int i = 0; i < 4; i++)
            {
                string path = $"Assets/Sprites/Borien/Borien_Bow_{i + 1}.png";

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Relative);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                map.bowFrames[i] = bmp;
            }
        }

        public void UpdateBowAnimation()
        {
            if (map.bowFrames == null || map.bowFrames.Length == 0)
                return;

            if (!map.isChargingBow)
            {
                map.lastBowFrameIndex = -1;
                return;
            }

            int bowFrameIndex;
            if (map.currentChargeStep <= 0) bowFrameIndex = 0;
            else if (map.currentChargeStep == 1) bowFrameIndex = 1;
            else if (map.currentChargeStep == 2) bowFrameIndex = 2;
            else bowFrameIndex = 3;

            if (bowFrameIndex != map.lastBowFrameIndex)
            {
                map.Player.Source = map.bowFrames[bowFrameIndex];
                map.lastBowFrameIndex = bowFrameIndex;
            }
        }

        public void UpdateBowCooldown()
        {
            if (!map.canShootBow)
            {
                map.bowShootCooldownTimer -= map.deltaTime;
                if (map.bowShootCooldownTimer <= 0)
                {
                    map.canShootBow = true;
                    map.bowShootCooldownTimer = 0;
                }
            }
        }

        public void LoadShieldSprite()
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri("Assets/Sprites/Borien/Borien_Shield.png", UriKind.Relative);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            map.shieldSprite = bmp;
        }
    }
}
