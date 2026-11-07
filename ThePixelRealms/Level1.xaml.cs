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
    public partial class Level1 : Window, IGameState
    {
        public Input input { get; set; }
        private PlayerMovement playerMovement { get; set; }
        public GamePhysics gamePhysics { get; set; }
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
        private WorldStreamingSystem worldStreaming { get; set; }

        // ===================== PLAYER =====================

        public double playerWorldX { get; set; } = 3800;
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

        public double WorldWidth { get; set; } = 18400;
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
        public bool IsDeadWindow => !IsLoaded || !IsVisible;

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

        public Level1()
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
            InitializeHousePlatforms();
            InitializeWorldStreaming();

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
            worldStreaming.Update(cameraX);
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

            if (story.CurrentStep == 0)
            {
                guide.ShowLine(0);
                currentHp = maxHp * 0.75;
                EnableBarricade(Barricade1Platform, Barricade1);
            }

            if (story.CurrentStep == 1)
            {
                guide.ShowLine(1);
            }

            if (story.CurrentStep == 2 && !miraAppleSpawned)
            {
                miraAppleSpawned = true;

                propManager.SpawnApple(6550, 760);

                guide.ShowLine(2);
            }

            if (story.CurrentStep == 3 && !dorinShieldSpawned)
            {
                dorinShieldSpawned = true;

                propManager.SpawnShield(8550, 745);

                guide.ShowLine(3);
            }

            if (!cutscene1Played && story.CurrentStep == 4)
            {
                cutscene1Played = true;
                OnCutsceneStart(1);
                cutscene.Start("Assets/Cutscenes/Cutscene1");
                DisableBarricade(Barricade1Platform, Barricade1);
            }

            if (story.CurrentStep == 5 && !fight1Started)
            {
                fight1Started = true;

                DespawnAllNpcs();
                SpawnFight1Enemies();

                guide.ShowLine(4);

                respawn.SetCheckpoint(9600, 700);
                EnableBarricade(Barricade2Platform, Barricade2);
            }

            if (fight1Started && enemiesKilled >= enemiesSpawned)
            {
                fight1Started = false;
                story.AdvanceStep();
                SpawnAfterFightNpcs();
            }

            bool everyoneArrived = true;

            foreach (var npc in npcs)
            {
                if (npc.Mode == NpcMode.Waypoint && !npc.ReachedTarget)
                {
                    everyoneArrived = false;
                    break;
                }
            }

            if (story.CurrentStep == 6 && !eldonSuppliesSpawned)
            {
                eldonSuppliesSpawned = true;

                guide.ShowLine(5);

                propManager.SpawnApple(2500, 760);
                propManager.SpawnShield(2600, 745);

            }

            uiAndDebug.UpdateSpeech();
            uiAndDebug.HandleNpcInteraction();

            if (!cutscene2Played && story.CurrentStep == 7 && playerWorldX <= 30)
            {
                cutscene2Played = true;
                OnCutsceneStart(2);
                cutscene.Start("Assets/Cutscenes/Cutscene2");
                DisableBarricade(Barricade2Platform, Barricade2);
            }

            if (story.CurrentStep == 8 && !rooftopChaseInitialized)
            {
                rooftopChaseInitialized = true;
                rooftopChaseStarted = true;
                StartRooftopChase();
                DisableBarricade(Barricade2Platform, TowerLayer1);

                guide.ShowLine(6);
                respawn.SetCheckpoint(900, 300);
            }

            if (rooftopChaseStarted && playerWorldX >= 5000)
            {
                rooftopChaseStarted = false;
                respawn.SetCheckpoint(5000, 700);
                story.AdvanceStep();
            }

            if (story.CurrentStep == 9 && playerWorldX >= 10000)
            {
                respawn.SetCheckpoint(10000, 700);
            }

            if (story.CurrentStep == 9 && playerWorldX >= 13500)
            {
                respawn.SetCheckpoint(13500, 700);
            }

            if (rooftopChaseStarted && enemiesKilled >= enemiesSpawned)
            {
                rooftopChaseStarted = false;

                RemoveChaseEnemies();

                story.AdvanceStep();

                guide.ShowLine(7);
            }

            if (!cutscene3Played && story.CurrentStep == 10 && playerWorldX >= 18200)
            {
                cutscene3Played = true;
                OnCutsceneStart(3);
                cutscene.Start("Assets/Cutscenes/Cutscene3");
            }

            if (story.CurrentStep == 11 && !finishShown)
            {
                finishShown = true;
                ShowFinishScreen();
            }

            guide.Update();
        }

        public void Resume_Click(object sender, RoutedEventArgs e) => uiAndDebug.TogglePause();

        private void PauseSettings_Click(object sender, RoutedEventArgs e)
        {
            PauseOverlay.Visibility = Visibility.Collapsed;
            PauseMenuContent.Visibility = Visibility.Collapsed;

            MainMenuWindow settingsWindow = new MainMenuWindow(this);
            settingsWindow.Show();
        }

        public void RestorePauseMenuAfterSettings()
        {
            if (!isPaused || isDead)
                return;

            PauseOverlay.Visibility = Visibility.Visible;
            PauseMenuContent.Visibility = Visibility.Visible;
            Focus();
        }

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

        private void InitializeWorldStreaming()
        {
            worldStreaming = new WorldStreamingSystem(GameCanvas, preloadDistance: 1800);

            // Large repeating world layers are split into one-PNG chunks.
            // This means only nearby pieces exist in the Canvas visual tree.
            worldStreaming.RegisterTiledRectangle(Clouds, 840);

            worldStreaming.RegisterTiledRectangle(Houses1, 1656);
            worldStreaming.RegisterTiledRectangle(Houses2, 1656);
            worldStreaming.RegisterTiledRectangle(Houses3, 1656);

            worldStreaming.RegisterTiledRectangle(HousesLayer06, 1362);
            worldStreaming.RegisterTiledRectangle(HousesLayer07, 1436);

            worldStreaming.RegisterTiledRectangle(HousesLayer08_1, 1508);
            worldStreaming.RegisterTiledRectangle(HousesLayer09_1, 1582);
            worldStreaming.RegisterTiledRectangle(HousesLayer08_2, 1508);
            worldStreaming.RegisterTiledRectangle(HousesLayer09_2, 1582);
            worldStreaming.RegisterTiledRectangle(HousesLayer08_3, 1508);
            worldStreaming.RegisterTiledRectangle(HousesLayer09_3, 1582);

            worldStreaming.RegisterTiledRectangle(Walls, 324);

            // Individual static map visuals. NPCs/enemies/platform collisions are NOT registered.
            worldStreaming.Register(Tree1);
            worldStreaming.Register(Castle);
            worldStreaming.Register(FinalWall);
            worldStreaming.Register(TowerLayer1);
            worldStreaming.Register(TowerLayer2);
            worldStreaming.Register(TowerLayer3);
            worldStreaming.Register(TowerLayer4);
            worldStreaming.Register(MarketStall1);
            worldStreaming.Register(MarketStall2);
            worldStreaming.Register(MarketStall3);
            worldStreaming.Register(MarketStall4);
            worldStreaming.Register(Blacksmith);
            worldStreaming.Register(Piactabla);

            // Ground remains permanent because it is also the main collision surface.
            // Barricades remain controlled by the story system.

            worldStreaming.Update(cameraX);
        }

        private void InitializeHousePlatforms()
        {
            CreateHousePlatforms(Houses1);
            CreateHousePlatforms(Houses2);
            CreateHousePlatforms(Houses3);
        }

        private void CreateHousePlatforms(Rectangle housesBlock)
        {
            const double houseSpacing = 414;
            const double roofOffsetX = 22;
            const double platformWidth = 330;
            const double platformHeight = 20;
            const double platformY = 394;

            double blockX = Canvas.GetLeft(housesBlock);
            int houseCount = (int)Math.Round(housesBlock.Width / houseSpacing);

            for (int i = 0; i < houseCount; i++)
            {
                Rectangle platform = new Rectangle
                {
                    Width = platformWidth,
                    Height = platformHeight,
                    Fill = Brushes.Transparent
                };

                Canvas.SetLeft(platform, blockX + roofOffsetX + (i * houseSpacing));
                Canvas.SetTop(platform, platformY);
                Panel.SetZIndex(platform, 98);

                GameCanvas.Children.Add(platform);
                platforms.Add(platform);
            }
        }

        private void InitializeEntities()
        {
            npcs.Add(Npc.Create(GameCanvas, NpcType.Guard1, 6500, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Guard2, 8000, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Eldon, 3500, 600, 200, 700, NpcMode.Stand));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Mira, 6500, 600, 550, 900, NpcMode.Stand));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Dorin, 8500, 600, 800, 1100, NpcMode.Stand));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Noril, 8000, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian1, 6000, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian2, 6500, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian3, 8000, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian4, 7500, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian5, 6500, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian6, 6500, 600, 5800, 8300, NpcMode.Wander));
            npcs.Add(Npc.Create(GameCanvas, NpcType.Civilian7, 7000, 600, 5800, 8300, NpcMode.Wander));
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

        internal void DespawnAllNpcs()
        {
            foreach (var npc in npcs)
            {
                GameCanvas.Children.Remove(npc.Visual);
            }

            npcs.Clear();
        }

        internal void SpawnFight1Enemies()
        {
            enemies.Clear();

            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Brogur_light, 11000, 700, 10800, 11200, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11300, 11800, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11300, 11800, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11300, 12000, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11300, 12000, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Brogur_light, 12500, 700, 12400, 13000, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Brogur_light, 12500, 700, 12400, 13000, EnemyMode.Wander));
            enemies.Add(Enemy.Create(GameCanvas, EnemyType.Brogur_medium, 13500, 700, 13300, 13600, EnemyMode.Wander));

            enemiesSpawned = 8;
            enemiesKilled = 0;
        }

        internal void SpawnAfterFightNpcs()
        {
            var eldon = Npc.Create(GameCanvas, NpcType.Eldon, 12500, 600, 0, 0, NpcMode.Waypoint);
            eldon.TargetX = 13000;

            var guard1 = Npc.Create(GameCanvas, NpcType.Guard1, 12350, 600, 0, 0, NpcMode.Waypoint);
            guard1.TargetX = 12900;

            var guard2 = Npc.Create(GameCanvas, NpcType.Guard2, 12270, 600, 0, 0, NpcMode.Waypoint);
            guard2.TargetX = 12850;

            var guard3 = Npc.Create(GameCanvas, NpcType.Noril, 12200, 600, 0, 0, NpcMode.Waypoint);
            guard3.TargetX = 12800;

            npcs.Add(eldon);
            npcs.Add(guard1);
            npcs.Add(guard2);
            npcs.Add(guard3);
        }

        internal void OnCutsceneStart(int id)
        {
            if (id == 1)
            {
                DespawnAllNpcs();

                playerWorldX = 9600;
                playerWorldY = 700;

                cameraX = playerWorldX - 300;
            }
            else if (id == 2)
            {
                DespawnAllNpcs();

                playerWorldX = 350;
                playerWorldY = 100;

                cameraX = playerWorldX - 300;
            }
        }

        internal void StartRooftopChase()
        {
            enemies.Clear();
            npcs.Clear();

            var g1 = Enemy.Create(GameCanvas, EnemyType.Grivak, 900, 150, 0, 0, EnemyMode.Waypoint);
            g1.TargetX = 20000;
            g1.AggroOnSight = false;
            g1.CountsForKill = false;
            enemies.Add(g1);

            var g2 = Enemy.Create(GameCanvas, EnemyType.Grivak, 850, 200, 0, 0, EnemyMode.Waypoint);
            g2.TargetX = 20000;
            g2.AggroOnSight = false;
            g2.CountsForKill = false;
            enemies.Add(g2);

            var g3 = Enemy.Create(GameCanvas, EnemyType.Grivak, 900, 250, 0, 0, EnemyMode.Waypoint);
            g3.TargetX = 20000;
            g3.AggroOnSight = false;
            g3.CountsForKill = false;
            enemies.Add(g3);

            var g4 = Enemy.Create(GameCanvas, EnemyType.Grivak, 1000, 150, 0, 0, EnemyMode.Waypoint);
            g4.TargetX = 20000;
            g4.AggroOnSight = false;
            g4.CountsForKill = false;
            enemies.Add(g4);

            var g5 = Enemy.Create(GameCanvas, EnemyType.Grivak, 1000, 200, 0, 0, EnemyMode.Waypoint);
            g5.TargetX = 20000;
            g5.AggroOnSight = false;
            g5.CountsForKill = false;
            enemies.Add(g5);

            var carrier = Enemy.Create(GameCanvas, EnemyType.CarryGrivak, 1050, 200, 0, 0, EnemyMode.Waypoint);
            carrier.TargetX = 20000;
            carrier.IsInvulnerable = true;
            carrier.AggroOnSight = false;
            carrier.AggroRange = 0;
            carrier.ContactDamage = 0;
            carrier.CountsForKill = false;
            enemies.Add(carrier);

            var e1 = Enemy.Create(GameCanvas, EnemyType.Skulk, 6000, 700, 5800, 6500, EnemyMode.Wander);
            var e2 = Enemy.Create(GameCanvas, EnemyType.Skulk, 6000, 700, 5800, 6500, EnemyMode.Wander);
            var e3 = Enemy.Create(GameCanvas, EnemyType.Skulk, 6000, 700, 5800, 6500, EnemyMode.Wander);
            var e4 = Enemy.Create(GameCanvas, EnemyType.Brogur_medium, 6500, 700, 6200, 6700, EnemyMode.Wander);
            var e5 = Enemy.Create(GameCanvas, EnemyType.Skulk, 7500, 700, 7300, 8000, EnemyMode.Wander);
            var e6 = Enemy.Create(GameCanvas, EnemyType.Brogur_light, 7500, 700, 7300, 8000, EnemyMode.Wander);
            var e7 = Enemy.Create(GameCanvas, EnemyType.Brogur_medium, 7500, 700, 7300, 8000, EnemyMode.Wander);
            var e8 = Enemy.Create(GameCanvas, EnemyType.Brogur_light, 9200, 700, 9000, 10000, EnemyMode.Wander);
            var e9 = Enemy.Create(GameCanvas, EnemyType.Brogur_light, 9200, 700, 9000, 10000, EnemyMode.Wander);
            var e10 = Enemy.Create(GameCanvas, EnemyType.Skulk, 9200, 700, 9000, 10000, EnemyMode.Wander);
            var e11 = Enemy.Create(GameCanvas, EnemyType.Brogur_medium, 9200, 700, 9000, 10000, EnemyMode.Wander);
            var e12 = Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11200, 11800, EnemyMode.Wander);
            var e13 = Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11200, 11800, EnemyMode.Wander);
            var e14 = Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11200, 11800, EnemyMode.Wander);
            var e15 = Enemy.Create(GameCanvas, EnemyType.Skulk, 11500, 700, 11200, 11800, EnemyMode.Wander);
            var e16 = Enemy.Create(GameCanvas, EnemyType.Brogur_heavy, 12300, 700, 12200, 12700, EnemyMode.Wander);
            var e17 = Enemy.Create(GameCanvas, EnemyType.Brogur_heavy, 16500, 700, 16300, 17000, EnemyMode.Wander);
            var e18 = Enemy.Create(GameCanvas, EnemyType.Brogur_heavy, 16500, 700, 16300, 17000, EnemyMode.Wander);
            var eldonDead = Npc.Create(GameCanvas, NpcType.Eldon, 17820, 700, 0, 0, NpcMode.Dead);

            enemies.Add(e1);
            enemies.Add(e2);
            enemies.Add(e3);
            enemies.Add(e4);
            enemies.Add(e5);
            enemies.Add(e6);
            enemies.Add(e7);
            enemies.Add(e8);
            enemies.Add(e9);
            enemies.Add(e10);
            enemies.Add(e11);
            enemies.Add(e12);
            enemies.Add(e13);
            enemies.Add(e14);
            enemies.Add(e15);
            enemies.Add(e16);
            enemies.Add(e17);
            enemies.Add(e18);
            npcs.Add(eldonDead);

            enemiesSpawned = 18;
            enemiesKilled = 0;

            propManager.SpawnApple(6650, 760);

            propManager.SpawnShield(8400, 745);

            propManager.SpawnApple(10500, 760);
            propManager.SpawnShield(10600, 745);

            propManager.SpawnApple(15000, 760);
            propManager.SpawnShield(15100, 745);
        }

        internal void RemoveChaseEnemies()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];

                if (!e.CountsForKill)
                {
                    GameCanvas.Children.Remove(e.Visual);
                    GameCanvas.Children.Remove(e.HpBarBackground);
                    GameCanvas.Children.Remove(e.HpBarFill);
                    GameCanvas.Children.Remove(e.AggroAlertIcon);

                    enemies.RemoveAt(i);
                }
            }
        }

        internal void EnableBarricade(Rectangle platform, Image visual)
        {
            visual.Visibility = Visibility.Visible;
            platform.Visibility = Visibility.Visible;

            if (!platforms.Contains(platform))
                platforms.Add(platform);
        }

        internal void DisableBarricade(Rectangle platform, Image visual)
        {
            visual.Visibility = Visibility.Collapsed;
            platform.Visibility = Visibility.Collapsed;

            platforms.Remove(platform);
        }

        private void ShowFinishScreen()
        {
            isPaused = true;
            FinishOverlay.Visibility = Visibility.Visible;
        }

        public void TogglePause()
        {
            uiAndDebug.TogglePause();
        }

        public void ReleaseArrow()
        {
            playerCombat.ReleaseArrow();
        }

        Canvas IWorldState.GameCanvas => GameCanvas;
        Grid IWorldState.RootGrid => RootGrid;
        Rectangle IWorldState.Ground => Ground;
        Image IUIState.Player => Player;
        UIElement IUIState.Dead => Dead;
        TextBlock IUIState.DebugText => DebugText;
        Rectangle IUIState.HpBarFill => HpBarFill;
        UIElement IUIState.PauseOverlay => PauseOverlay;
        ScaleTransform IUIState.PlayerScaleTransform => (ScaleTransform)Player.RenderTransform;
        Rectangle IPropState.ShieldBarFill => bowChargeBarFill;
        GuideSystem ISystemState.guideSystem => guide;

        double IGameState.ChargeStepTime => 0.2;
        int IGameState.MaxChargeSteps => 5;
        double IGameState.maxShield => 100;
    }
}