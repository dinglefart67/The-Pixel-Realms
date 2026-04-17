using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace ThePixelRealms.Tests
{
    public class FakeGameState : IGameState
    {
        public FakeGameState()
        {
            GameCanvas = new Canvas();
            RootGrid = new Grid();
            Player = new Image
            {
                Width = 50,
                Height = 60,
                RenderTransform = new ScaleTransform()
            };

            enemies = new List<Enemy>();
            arrows = new List<Arrow>();

            input = new Input(this);
            dialogueSystem = null;
            cutscene = null;
            guideSystem = null;
            uiAndDebug = new UIAndDebug(this);
        }

        public bool isPaused { get; set; }
        public bool isDead { get; set; }
        public double deltaTime { get; set; } = 0.016;
        public int frameCount { get; set; }
        public double fps { get; set; }
        public DateTime lastFpsUpdate { get; set; } = DateTime.Now;

        public double playerWorldX { get; set; }
        public double playerWorldY { get; set; }
        public double playerVelocityX { get; set; }
        public double playerVelocityY { get; set; }

        public bool isOnGround { get; set; }
        public bool isJumping { get; set; }
        public bool isFacingRight { get; set; }

        public double playerWalkSpeed => 100;
        public double playerRunSpeed => 200;
        public double jumpForce => 300;

        public double currentHp { get; set; } = 100;
        public double maxHp => 100;

        public Rect GetPlayerHitboxRect() => new Rect(playerWorldX, playerWorldY, 50, 60);

        public Input input { get; set; }

        public bool leftPressed { get; set; }
        public bool rightPressed { get; set; }
        public bool spacePressed { get; set; }
        public bool shiftPressed { get; set; }

        public List<Enemy> enemies { get; set; }
        public List<Arrow> arrows { get; set; }

        public bool attackPressed { get; set; }
        public bool canMeleeAttack { get; set; } = true;
        public bool isMeleeAnimating { get; set; }

        public double attackDamage => 10;
        public double enemyKnockbackX => 100;
        public double enemyKnockbackY => 100;

        public double meleeCooldown => 0.5;
        public double meleeCooldownTimer { get; set; }

        public bool canShootBow { get; set; } = true;
        public bool isChargingBow { get; set; }
        public bool bowKeyPressed { get; set; }

        public double bowChargeTime { get; set; }
        public int currentChargeStep { get; set; }

        public double currentAimRatioY { get; set; }

        public bool isShielding { get; set; }

        public bool canTakeDamage => true;

        public double invulnerableVisualTimer { get; set; }
        public double invulnerableVisualTime => 0.5;

        public double playerStunTimer { get; set; }
        public double playerStunDuration => 0.3;

        public double playerKnockbackX => 200;
        public double playerKnockbackY => 200;

        public double attackRange => 100;

        public Rectangle bowChargeBarBg { get; set; } = new Rectangle();
        public Rectangle bowChargeBarFill { get; set; } = new Rectangle();

        public Line aimLineLeft { get; set; } = new Line();
        public Line aimLineRight { get; set; } = new Line();
        public Ellipse aimDot { get; set; } = new Ellipse();

        public BitmapImage[] idleFrames { get; set; }
        public BitmapImage[] walkFrames { get; set; }
        public BitmapImage[] meleeFrames { get; set; }
        public BitmapImage[] bowFrames { get; set; }

        public BitmapImage jumpAirFrame { get; set; }
        public BitmapImage shieldSprite { get; set; }

        public int idleFrameIndex { get; set; }
        public double idleFrameTimer { get; set; }

        public int walkFrameIndex { get; set; }
        public double walkFrameTimer { get; set; }

        public int meleeFrameIndex { get; set; }
        public double meleeFrameTimer { get; set; }

        public int lastBowFrameIndex { get; set; }

        public bool wasWalking { get; set; }

        public double walkFrameDuration => 0.1;
        public double meleeFrameDuration => 0.05;
        public double[] idleFrameDurations => new[] { 0.5, 0.05, 0.5, 0.05 };

        public Canvas GameCanvas { get; set; }
        public Grid RootGrid { get; set; }

        public double cameraX { get; set; }

        public double WorldWidth => 2000;
        public double screenLeftLimit => 200;
        public double screenRightLimit => 200;

        public Rectangle Ground { get; } = new Rectangle();
        public double GroundX => 0;

        public List<Rectangle> platforms { get; } = new();

        public double gravity => 1000;

        public List<Npc> npcs { get; } = new();

        public Npc currentNpc { get; set; }

        public bool isGreetingActive { get; set; }
        public double npcSpeechTimer { get; set; }

        public TextBlock npcSpeech { get; } = new();
        public Image npcBubbleLeft { get; } = new();
        public Image npcBubbleMiddle { get; } = new();
        public Image npcBubbleRight { get; } = new();

        public TextBlock interactionHint { get; } = new();

        public Dictionary<int, NpcType> talkSequence { get; } = new();
        public HashSet<NpcType> greetingAllowedTypes { get; } = new();

        public double currentShield { get; set; }
        public Rectangle ShieldBarFill { get; } = new Rectangle();
        public double appleHealAmount => 10;

        public StoryManager story { get; } = new();

        public Image Player { get; set; }
        public UIElement Dead { get; } = new Rectangle();

        public TextBlock DebugText { get; } = new();
        public Rectangle HpBarFill { get; } = new();

        public UIElement PauseOverlay { get; } = new Rectangle();

        public ScaleTransform PlayerScaleTransform { get; } = new();

        public DialogueSystem dialogueSystem { get; set; }
        public CutsceneSystem cutscene { get; set; }
        public GuideSystem guideSystem { get; set; }
        public UIAndDebug uiAndDebug { get; set; }

        public double ActualWidth => 1920;
        public double ActualHeight => 1080;

        public int enemiesKilled { get; set; }

        public double ChargeStepTime => 0.2;
        public int MaxChargeSteps => 5;

        public double bowShootCooldownTimer { get; set; }
        public double bowShootCooldown { get; set; } = 0.5;

        public double aimLineHeight { get; set; } = 100;

        public bool IsPlayerStunned => playerStunTimer > 0;

        public double maxShield => 100;

        public void DamagePlayer(double amount, Enemy sourceEnemy = null)
        {
            currentHp -= amount;
            if (currentHp < 0) currentHp = 0;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
        }

        public void ReleaseArrow()
        {
        }
    }
}