using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public partial class TestMap : Window, IGameState
    {


        public Input input { get; set; }
        private PlayerMovement playerMovement { get; set; }
        private GamePhysics gamePhysics { get; set; }
        public UIAndDebug uiAndDebug { get; set; }
        private EnemyMovement enemyMovement { get; set; }
        private EnemyCombat enemyCombat { get; set; }
        private PlayerCombat playerCombat { get; set; }
        private NpcMovement npcMovement { get; set; }
        private NpcCombat npcCombat { get; set; }
        private PropManager propManager { get; set; }
        public DialogueSystem dialogueSystem { get; set; }
        public CutsceneSystem cutscene { get; set; }
        public GuideSystem guide { get; set; }
        public StoryManager story { get; set; }
        internal RespawnSystem respawn { get; set; }

        // ===================== PLAYER =====================

        public double playerWorldX { get; set; } = 300;
        public double playerWorldY { get; set; } = 700;

        public double playerVelocityX { get; set; } = 0;
        public double playerVelocityY { get; set; } = 0;

        public bool isOnGround { get; set; }
        public bool isJumping { get; set; }
        public bool wasOnGround { get; set; }

        public bool isFacingRight { get; set; } = true;

        public double playerWalkSpeed { get; set; } = 150;
        public double playerRunSpeed { get; set; } = 350;

        public double jumpForce { get; set; } = 450;
        public double gravity { get; set; } = 1500;

        public bool leftPressed { get; set; }
        public bool rightPressed { get; set; }
        public bool spacePressed { get; set; }
        public bool shiftPressed { get; set; }

        // ===================== COMBAT =====================

        public double maxHp { get; set; } = 100;
        public double currentHp { get; set; } = 100;

        public bool attackPressed { get; set; }
        public double attackRange { get; set; } = 100;
        public double attackDamage { get; set; } = 40;

        public bool canTakeDamage { get; set; } = true;
        public double damageTimer { get; set; }
        public double damageCooldown { get; set; } = 3.0;

        public double playerKnockbackX { get; set; } = 350;
        public double playerKnockbackY { get; set; } = 300;

        public double enemyKnockbackX { get; set; } = 300;
        public double enemyKnockbackY { get; set; } = 250;

        public double playerStunTimer { get; set; }
        public double playerStunDuration { get; set; } = 0.4;
        public bool IsPlayerStunned => playerStunTimer > 0;

        public double invulnerableVisualTime { get; set; } = 0.5;
        public double invulnerableVisualTimer { get; set; }

        public bool canMeleeAttack { get; set; } = true;
        public double meleeCooldownTimer { get; set; }
        public double meleeCooldown { get; set; } = 0.5;

        public BitmapImage[] meleeFrames { get; set; }
        public int meleeFrameIndex { get; set; }
        public double meleeFrameTimer { get; set; }

        public bool isMeleeAnimating { get; set; }
        public double meleeFrameDuration { get; set; } = 0.05;

        public bool canShootBow { get; set; } = true;
        public double bowShootCooldownTimer { get; set; }
        public double bowShootCooldown { get; set; } = 0.5;

        public bool bowKeyPressed { get; set; }
        public bool isChargingBow { get; set; }

        public double bowChargeTime { get; set; }
        public int currentChargeStep { get; set; }

        public const int MaxChargeSteps = 5;
        public const double ChargeStepTime = 0.2;

        public BitmapImage[] bowFrames { get; set; }
        public int lastBowFrameIndex { get; set; } = -1;

        public List<Arrow> arrows { get; set; } = new();

        public double currentAimRatioY { get; set; }

        public Line aimLineLeft { get; set; }
        public Line aimLineRight { get; set; }
        public Ellipse aimDot { get; set; }

        public double aimLineHeight { get; set; } = 120;

        public Rectangle bowChargeBarBg { get; set; }
        public Rectangle bowChargeBarFill { get; set; }

        // ===================== ANIMÁCIÓ =====================

        public BitmapImage[] idleFrames { get; set; }
        public int idleFrameIndex { get; set; }
        public double idleFrameTimer { get; set; }
        public double[] idleFrameDurations { get; } = { 0.5, 0.05, 0.5, 0.05 };

        public BitmapImage[] walkFrames { get; set; }
        public int walkFrameIndex { get; set; }
        public double walkFrameTimer { get; set; }
        public bool wasWalking { get; set; }
        public double walkFrameDuration { get; set; } = 0.08;

        public BitmapImage jumpAirFrame { get; set; }

        public BitmapImage shieldSprite { get; set; }
        public bool isShielding { get; set; }

        // ===================== WORLD =====================

        public double WorldWidth { get; set; } = 19000;
        public double cameraX { get; set; }

        public double screenLeftLimit { get; set; } = 400;
        public double screenRightLimit { get; set; } = 500;

        public double GroundX { get; set; } = 10;

        public const double PlayerHitboxWidth = 48;
        public const double PlayerHitboxHeight = 60;
        public const double PlayerHitboxOffsetX = 30;

        public List<Enemy> enemies { get; set; } = new();
        public List<Npc> npcs { get; set; } = new();

        public List<Rectangle> platforms { get; set; } = new();
        public List<Image> worldObjects { get; set; } = new();

        // ===================== NPC / UI =====================

        public TextBlock interactionHint { get; set; }

        public TextBlock npcSpeech { get; set; }
        public double npcSpeechTimer { get; set; }

        public bool isGreetingActive { get; set; }
        public double npcResponseDelay { get; set; }

        public Image npcBubbleLeft { get; set; }
        public Image npcBubbleMiddle { get; set; }
        public Image npcBubbleRight { get; set; }

        public Npc currentNpc { get; set; }

        public HashSet<NpcType> greetingAllowedTypes { get; set; } = new()
        {
            NpcType.Guard1,
            NpcType.Guard2,
            NpcType.Guard3,
            NpcType.Civilian1,
            NpcType.Civilian2,
            NpcType.Civilian3,
            NpcType.Civilian4,
            NpcType.Civilian5,
            NpcType.Civilian6,
            NpcType.Civilian7
        };

        public Dictionary<int, NpcType> talkSequence { get; set; } = new()
        {
            { 0, NpcType.Eldon },
            { 1, NpcType.Mira },
            { 2, NpcType.Dorin },
            { 3, NpcType.Noril },
            { 6, NpcType.Eldon },
            { 9, NpcType.Eldon }
        };

        // ===================== ITEMS =====================

        public double shieldWorldX { get; set; } = 1500;
        public double shieldWorldY { get; set; } = 745;

        public const double maxShield = 100;
        public double currentShield { get; set; }

        public double appleWorldX { get; set; } = 1700;
        public double appleWorldY { get; set; } = 760;
        public double appleHealAmount { get; set; } = 35;

        // ===================== GAME STATE =====================

        public bool isPaused { get; set; }
        public bool isDead { get; set; }

        public int enemiesSpawned { get; set; }
        public int enemiesKilled { get; set; }

        public bool fight1Started { get; set; }
        public bool fight2Started { get; set; }
        public bool fight2Finished { get; set; }

        public bool rooftopChaseStarted { get; set; }

        public bool cutscene1Played { get; set; }
        public bool cutscene2Played { get; set; }
        public bool cutscene3Played { get; set; }

        public bool rooftopChaseInitialized { get; set; }
        public bool finishShown { get; set; }

        public bool miraAppleSpawned { get; set; }
        public bool dorinShieldSpawned { get; set; }
        public bool eldonSuppliesSpawned { get; set; }

        // ===================== TIME / DEBUG =====================

        public DateTime lastUpdateTime { get; set; }
        public double deltaTime { get; set; }

        public List<string> debugInfo { get; set; } = new();
        public int frameCount { get; set; }
        public double fps { get; set; }
        public DateTime lastFpsUpdate { get; set; } = DateTime.Now;

        public TestMap()
        {
            InitializeComponent();
            Closing += Window_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();

            lastUpdateTime = DateTime.Now;
            CompositionTarget.Rendering += GameLoop;

            DebugText.Text = "";

            enemyCombat = new EnemyCombat(this);
            gamePhysics = new GamePhysics(this);
            playerCombat = new PlayerCombat(this, enemyCombat, gamePhysics);
            playerMovement = new PlayerMovement(this);
            enemyMovement = new EnemyMovement(this);
            uiAndDebug = new UIAndDebug(this);
            input = new Input(this);
            story = new StoryManager();
            npcMovement = new NpcMovement(this);
            npcCombat = new NpcCombat(this);
            propManager = new PropManager(this);
            dialogueSystem = new DialogueSystem(this);
            cutscene = new CutsceneSystem(this);
            guide = new GuideSystem(this);
            respawn = new RespawnSystem(this);

            InitializeEntities();

            uiAndDebug.LoadPlayerImage();
            playerCombat.LoadIdleFrames();
            playerCombat.LoadWalkFrames();
            playerCombat.LoadBowFrames();
            playerCombat.LoadMeleeFrames();
            playerMovement.LoadJumpAirFrame();
            playerCombat.LoadShieldSprite();
            uiAndDebug.InitializeSpeechUi();
            uiAndDebug.InitializeBowAndAimUI();
            uiAndDebug.UpdateHpBar();

            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is Rectangle rect && rect.Name.Contains("Platform"))
                {
                    platforms.Add(rect);
                }

                if (element is Image img && img.Name.Contains("House"))
                {
                    worldObjects.Add(img);
                }
            }

            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is FrameworkElement fe)
                {
                    fe.Tag = Canvas.GetLeft(fe);
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CompositionTarget.Rendering -= GameLoop;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) => input.Window_KeyDown(sender, e);
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) => input.Window_KeyUp(sender, e);

        private void GameLoop(object sender, EventArgs e)
        {
            if (cutscene.IsActive)
            {
                if (input.EPressed)
                {
                    cutscene.Advance();
                    input.EPressed = false;
                }

                cutscene.Update();
                return;
            }

            dialogueSystem.Update();

            if (dialogueSystem.IsActive || cutscene.IsActive)
            {
                leftPressed = false;
                rightPressed = false;
                spacePressed = false;
                bowKeyPressed = false;
                attackPressed = false;
                isShielding = false;

                playerVelocityX = 0;
                playerVelocityY = 0;
            }

            if (isPaused)
                return;

            DateTime currentTime = DateTime.Now;
            deltaTime = (currentTime - lastUpdateTime).TotalSeconds;
            if (deltaTime > 0.1) deltaTime = 0.1;
            lastUpdateTime = currentTime;

            uiAndDebug.UpdateFPS();
            debugInfo.Clear();

            playerMovement.HandleMovement();
            gamePhysics.ApplyPhysics();

            bool prevOnGround = wasOnGround;

            gamePhysics.CheckCollisions();

            if (!prevOnGround && isOnGround)
            {
                if (idleFrames != null && idleFrames.Length > 0)
                {
                    idleFrameIndex = 0;
                    idleFrameTimer = 0;
                    Player.Source = idleFrames[0];
                }
            }

            wasOnGround = isOnGround;

            playerMovement.UpdatePlayerPosition();
            gamePhysics.UpdateCamera();
            playerMovement.UpdatePlayerFacingDirection();

            uiAndDebug.UpdateDebugInfo();
            uiAndDebug.UpdateHpBar();

            enemyMovement.UpdateEnemies();
            enemyCombat.CheckEnemyCollisions();

            playerCombat.UpdateDamageCooldown();
            playerCombat.HandlePlayerAttack();
            playerCombat.UpdatePlayerStun();

            playerCombat.UpdateBowCharge();
            playerCombat.UpdateArrows();
            playerCombat.UpdateAimUI();
            playerCombat.UpdateMeleeCooldown();
            playerCombat.UpdateJumpAirSprite();
            playerCombat.UpdateBowAnimation();
            playerCombat.UpdateMeleeAnimation();
            playerMovement.UpdateWalkAnimation();
            playerCombat.UpdateBowCooldown();
            playerCombat.UpdateIdleAnimation();

            npcCombat.CheckNpcCollisions();
            npcMovement.UpdateNpcs();

            uiAndDebug.CheckDeath();

            propManager.UpdateApples();
            propManager.UpdateShields();

            dialogueSystem.Update();

            if (isShielding && shieldSprite != null)
                Player.Source = shieldSprite;

            foreach (var npc in npcs)
            {
                if (npc.InteractCooldown > 0)
                    npc.InteractCooldown -= deltaTime;
            }

            if (playerWorldX >= 100)
            {
                respawn.SetCheckpoint(100, 700);
                propManager.SpawnApple(200, 760);
                propManager.SpawnShield(150, 740);
            }
        }

        public void Resume_Click(object sender, RoutedEventArgs e) => uiAndDebug.TogglePause();

        private void ExitToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenuWindow menu = new MainMenuWindow();
            menu.Show();
            Close();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Respawn_Click(object sender, RoutedEventArgs e)
        {
            respawn.RespawnPlayer();
        }

        private void InitializeEntities()
        {
            npcs.Add(Npc.Create(GameCanvas, NpcType.Guard1, 6000, 600, 5700, 7000, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Guard2, 6000, 600, 5700, 7000, NpcMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Grivak, 1000, 600, 800, 2000, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Brogur_light, 1200, 600, 800, 2000, EnemyMode.Wander));
        }

        public void DamagePlayer(double amount, Enemy sourceEnemy = null) => playerCombat.DamagePlayer(amount, sourceEnemy);

        public Rect GetPlayerHitboxRect()
        {
            return new Rect(
                playerWorldX + PlayerHitboxOffsetX,
                playerWorldY,
                PlayerHitboxWidth,
                PlayerHitboxHeight
            );
        }

        public void TogglePause()
        {
            uiAndDebug.TogglePause();
        }

        public void ReleaseArrow()
        {
            playerCombat.ReleaseArrow();
        }

        // ================= IWorldState =================
        Canvas IWorldState.GameCanvas => GameCanvas;
        Grid IWorldState.RootGrid => RootGrid;
        Rectangle IWorldState.Ground => Ground;

        // ================= IUIState =================
        Image IUIState.Player => Player;
        UIElement IUIState.Dead => Dead;
        TextBlock IUIState.DebugText => DebugText;
        Rectangle IUIState.HpBarFill => HpBarFill;
        UIElement IUIState.PauseOverlay => PauseOverlay;
        ScaleTransform IUIState.PlayerScaleTransform => (ScaleTransform)Player.RenderTransform;

        // ================= IPropState =================
        Rectangle IPropState.ShieldBarFill => bowChargeBarFill;

        // ================= ISystemState =================
        GuideSystem ISystemState.guideSystem => guide;

        // ================= IGameState (maradékok) =================
        double IGameState.ChargeStepTime => 0.2;
        int IGameState.MaxChargeSteps => 5;
        double IGameState.maxShield => 100;
    }
}