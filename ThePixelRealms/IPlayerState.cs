using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ThePixelRealms
{
    public interface IPlayerState
    {
        double playerWorldX { get; set; }
        double playerWorldY { get; set; }

        double playerVelocityX { get; set; }
        double playerVelocityY { get; set; }

        bool isOnGround { get; set; }
        bool isJumping { get; set; }
        bool isFacingRight { get; set; }

        double playerWalkSpeed { get; }
        double playerRunSpeed { get; }
        double jumpForce { get; }

        double currentHp { get; set; }
        double maxHp { get; }

        Rect GetPlayerHitboxRect();
    }
}
