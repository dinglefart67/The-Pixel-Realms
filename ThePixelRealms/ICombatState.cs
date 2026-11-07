using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public interface ICombatState
    {
        List<Enemy> enemies { get; }
        List<Arrow> arrows { get; }

        bool attackPressed { get; set; }

        bool canMeleeAttack { get; set; }
        bool isMeleeAnimating { get; set; }
        double attackDamage { get; }
        double enemyKnockbackX { get; }
        double enemyKnockbackY { get; }

        double meleeCooldown { get; }
        double meleeCooldownTimer { get; set; }

        bool canShootBow { get; set; }
        bool isChargingBow { get; set; }
        bool bowKeyPressed { get; set; }

        double bowChargeTime { get; set; }
        int currentChargeStep { get; set; }

        double currentAimRatioY { get; set; }

        bool isShielding { get; set; }

        bool canTakeDamage { get; }

        double invulnerableVisualTimer { get; set; }
        double invulnerableVisualTime { get; }

        double playerStunTimer { get; set; }
        double playerStunDuration { get; }

        double playerKnockbackX { get; }
        double playerKnockbackY { get; }

        double attackRange { get; }

        Rectangle bowChargeBarBg { get; }
        Rectangle bowChargeBarFill { get; }

        Line aimLineLeft { get; }
        Line aimLineRight { get; }
        Ellipse aimDot { get; }
    }
}
