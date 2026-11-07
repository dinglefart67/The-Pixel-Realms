using System.Windows;

namespace ThePixelRealms
{
    public class RespawnSystem
    {
        private readonly IGameState map;

        private double checkpointX;
        private double checkpointY;

        private bool hasCheckpoint = false;

        public RespawnSystem(IGameState map)
        {
            this.map = map;
        }

        public void SetCheckpoint(double x, double y)
        {
            checkpointX = x;
            checkpointY = y;
            hasCheckpoint = true;
        }

        public void RespawnPlayer()
        {
            if (!hasCheckpoint)
                return;

            map.currentHp = map.maxHp;
            map.uiAndDebug.UpdateHpBar();

            map.currentShield = 0;
            map.uiAndDebug.UpdateShieldBar();
            map.isShielding = false;

            map.playerWorldX = checkpointX;
            map.playerWorldY = checkpointY;

            map.playerVelocityX = 0;
            map.playerVelocityY = 0;

            map.cameraX = checkpointX - 300;

            map.isDead = false;
            map.isPaused = false;

            map.Player.Visibility = Visibility.Visible;
            map.Dead.Visibility = Visibility.Collapsed;

            ResetEnemies();
        }

        private void ResetEnemies()
        {
            foreach (var enemy in map.enemies)
            {
                enemy.IsAggro = false;
                enemy.IsInAggroAlert = false;
            }
        }
    }
}