using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public interface IGameState :
        ICoreState,
        IPlayerState,
        IWorldState,
        INpcState,
        IPropState,
        IUIState,
        IStoryState,
        IInputState,
        ICombatState,
        IAnimationState,
        ISystemState
    {
        double ActualWidth { get; }
        double ActualHeight { get; }
        void DamagePlayer(double amount, Enemy sourceEnemy = null);
        int enemiesKilled { get; set; }
        double ChargeStepTime { get; }
        int MaxChargeSteps { get; }

        double bowShootCooldownTimer { get; set; }
        public double bowShootCooldown { get; set; }

        double aimLineHeight { get; set; }

        bool IsPlayerStunned { get; }

        double maxShield { get; }

        void TogglePause();
        void ReleaseArrow();
    }

}
