using System;
using System.Windows;
using System.Windows.Media;

namespace ThePixelRealms
{
    public class EnemyCombat
    {
        private readonly IGameState map;

        public EnemyCombat(IGameState map)
        {
            this.map = map;
        }

        public void CheckEnemyCollisions()
        {
            if (map.isDead)
                return;

            foreach (var enemy in map.enemies)
            {
                Rect pr = map.GetPlayerHitboxRect();

                double playerCenterX = pr.X + pr.Width / 2.0;
                double playerCenterY = pr.Y + pr.Height / 2.0;

                double enemyCenterX = enemy.WorldX + enemy.Visual.Width / 2.0;
                double enemyCenterY = enemy.WorldY + enemy.Visual.Height / 2.0;

                double dx = Math.Abs(playerCenterX - enemyCenterX);
                double dy = Math.Abs(playerCenterY - enemyCenterY);

                bool inVerticalBand = dy < 50;
                bool inContactBand = dx <= enemy.AttackRange;

                if (inVerticalBand && inContactBand)
                {
                    if (enemy.DamageCooldownTimer <= 0)
                    {
                        enemy.IsAttacking = true;
                        enemy.CurrentAnimState = "";
                        enemy.FrameIndex = 0;
                        enemy.FrameTimer = 0;

                        if (enemy.AttackFrames != null && enemy.AttackFrames.Length > 0)
                        {
                            enemy.Visual.Source = enemy.AttackFrames[0];
                        }

                        map.DamagePlayer(enemy.ContactDamage, enemy);

                        enemy.DamageCooldownTimer = enemy.DamageCooldown;

                        enemy.Visual.Opacity = enemy.HitFlashOpacity;
                        enemy.HitFlashTimer = 0.2;
                    }
                }
            }
        }

        public void KillEnemy(Enemy enemy)
        {
            if (enemy.CountsForKill)
            {
                map.enemiesKilled++;
            }

            map.GameCanvas.Children.Remove(enemy.Visual);
            map.GameCanvas.Children.Remove(enemy.HpBarBackground);
            map.GameCanvas.Children.Remove(enemy.HpBarFill);
            map.GameCanvas.Children.Remove(enemy.AggroAlertIcon);

            map.enemies.Remove(enemy);
        }

        public void DamageEnemy(Enemy enemy)
        {
            enemy.CurrentHp -= map.attackDamage;

            enemy.IsAggro = true;

            enemy.Visual.Opacity = enemy.HitFlashOpacity;
            enemy.HitFlashTimer = 0.2;

            if (map.playerWorldX < enemy.WorldX)
                enemy.VelocityX = map.enemyKnockbackX;
            else
                enemy.VelocityX = -map.enemyKnockbackX;

            enemy.VelocityY = -map.enemyKnockbackY;

            enemy.StunTimer = Enemy.StunDuration;

            if (enemy.CurrentHp <= 0)
            {
                KillEnemy(enemy);
            }
        }

        public void DamageEnemy(Enemy enemy, double damage)
        {
            if (enemy.IsInvulnerable)
                return;

            if (!enemy.IsAggro)
            {
                enemy.IsAggro = true;
            }

            enemy.CurrentHp -= damage;

            enemy.Visual.Opacity = enemy.HitFlashOpacity;
            enemy.HitFlashTimer = 0.2;

            enemy.StunTimer = Enemy.StunDuration;

            if (enemy.CurrentHp <= 0)
            {
                KillEnemy(enemy);
            }
        }
    }
}
