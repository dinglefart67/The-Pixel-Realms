using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public enum NpcType
    {
        Civilian1,
        Civilian2,
        Civilian3,
        Civilian4,
        Civilian5,
        Civilian6,
        Civilian7,
        Guard1,
        Guard2,
        Guard3,
        Eldon,
        Mira,
        Dorin,
        Noril
    }

    public enum NpcMode
    {
        Stand,
        Patrol,
        Wander,
        Waypoint,
        Dead
    }

    public class Npc
    {
        public NpcType Type { get; }
        public NpcMode Mode { get; }

        public Image Visual;

        public BitmapImage[] IdleFrames;
        public BitmapImage[] WalkFrames;
        public BitmapImage[] DeadFrames;

        public int FrameIndex = 0;
        public double FrameTimer = 0;

        public double WalkFrameDuration = 0.08;
        public double[] IdleFrameDurations = { 0.5, 0.05, 0.5, 0.05 };

        public string CurrentAnimState = "";

        public double WorldX;
        public double WorldY;
        public double VelocityX;
        public double VelocityY;

        public double TargetX;
        public bool ReachedTarget = false;

        public double WalkSpeed;

        public double InteractCooldown = 0;

        public double LeftLimit;
        public double RightLimit;
        public bool OnGround;

        public string DialogueFile { get; set; } = null;
        public string DisplayName { get; set; } = "";
        public string InteractionHintText { get; set; } = "Press (E) to talk";

        public double PatrolWaitTime { get; set; } = 4.0;
        public double PatrolWaitTimer { get; set; } = 0.0;

        public double WanderTimer { get; set; } = 0.0;

        public int MoveDir { get; set; } = 1;
        public bool IsWaiting { get; set; } = false;

        public ScaleTransform Scale { get; }

        private readonly Canvas canvas;

        public Npc(
            Canvas canvas,
            NpcType type,
            NpcMode mode,
            double startX,
            double startY,
            double leftLimit,
            double rightLimit,
            double width,
            double height,
            Brush fill,
            double walkSpeed)
        {
            this.canvas = canvas;

            Type = type;
            Mode = mode;

            WorldX = startX;
            WorldY = startY;

            LeftLimit = leftLimit;
            RightLimit = rightLimit;

            WalkSpeed = walkSpeed;

            VelocityX = WalkSpeed;
            VelocityY = 0;

            Visual = new Image
            {
                Width = width,
                Height = height,
                Source = (fill as ImageBrush)?.ImageSource
            };

            RenderOptions.SetBitmapScalingMode(Visual, BitmapScalingMode.NearestNeighbor);

            Scale = new ScaleTransform(1, 1);
            Visual.RenderTransformOrigin = new Point(0.5, 0.5);
            Visual.RenderTransform = Scale;

            Panel.SetZIndex(Visual, 900);

            canvas.Children.Add(Visual);
        }

        public static Npc Create(
            Canvas canvas,
            NpcType type,
            double startX,
            double startY,
            double leftLimit,
            double rightLimit,
            NpcMode mode = NpcMode.Patrol)
        {
            switch (type)
            {
                case NpcType.Guard1:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Guards/Guard1/Guard1_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Guards/Guard1/Guard1_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Guards/Guard1/Guard1_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Guard2:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Guards/Guard2/Guard2_Stand2.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Guards/Guard2/Guard2_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Guards/Guard2/Guard2_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Guard3:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Guards/Guard3/Guard3_Stand2.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Guards/Guard3/Guard3_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Guards/Guard3/Guard3_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Mira:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/SideCharacters/Mira/Mira_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/SideCharacters/Mira/Mira_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/SideCharacters/Mira/Mira_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Dorin:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/SideCharacters/Dorin/Dorin_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/SideCharacters/Dorin/Dorin_Stand", 4);
                        npc.WalkFrames = new[] { bmp };

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Noril:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Guards/Guard3/Guard3_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Guards/Guard3/Guard3_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Guards/Guard3/Guard3_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian1:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian1_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian1_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian1_Walk", 4); 

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian2:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian2_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian2_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian2_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian3:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian3_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian3_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian3_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian4:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian4_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian4_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian4_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian5:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian5_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian5_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian5_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian6:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian6_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian6_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian6_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Civilian7:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/Civilians/Civilian7_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 60
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/Civilians/Civilian7_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/Civilians/Civilian7_Walk", 4);

                        npc.CurrentAnimState = "Idle";
                        npc.FrameIndex = 0;
                        npc.FrameTimer = 0;
                        npc.Visual.Source = npc.IdleFrames[0];

                        return npc;
                    }

                case NpcType.Eldon:
                default:
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("Assets/Sprites/SideCharacters/Eldon/Eldon_Stand1.png", UriKind.Relative);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };

                        var npc = new Npc(
                            canvas, type, mode,
                            startX, startY, leftLimit, rightLimit,
                            width: 48,
                            height: 60,
                            fill: brush,
                            walkSpeed: 70
                        );

                        npc.IdleFrames = LoadFrames("Assets/Sprites/SideCharacters/Eldon/Eldon_Stand", 4);
                        npc.WalkFrames = LoadFrames("Assets/Sprites/SideCharacters/Eldon/Eldon_Walk", 4);
                        npc.DeadFrames = LoadFrames("Assets/Sprites/SideCharacters/Eldon/Eldon_Dead", 1);

                        if (mode == NpcMode.Dead)
                        {
                            npc.IdleFrames = npc.DeadFrames;
                            npc.WalkFrames = npc.DeadFrames;

                            npc.CurrentAnimState = "Idle";
                            npc.FrameIndex = 0;
                            npc.FrameTimer = 0;

                            npc.Visual.Source = npc.IdleFrames[0];
                        }

                        return npc;
                    }
            }
        }

        private static BitmapImage[] LoadFrames(string basePath, int count)
        {
            BitmapImage[] frames = new BitmapImage[count];

            for (int i = 0; i < count; i++)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri($"{basePath}{i + 1}.png", UriKind.Relative);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                frames[i] = bmp;
            }

            return frames;
        }
    }
}