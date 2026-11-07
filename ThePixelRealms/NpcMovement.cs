using System;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public class NpcMovement
    {
        public IGameState map;
        private static readonly Random Rng = new Random();

        public NpcMovement(IGameState map)
        {
            this.map = map;
        }

        public void UpdateNpcs()
        {
            foreach (var npc in map.npcs)
            {
                bool isTalkingNpc =
                    map.dialogueSystem.IsActive &&
                    npc == map.dialogueSystem.CurrentNpc;

                if (!isTalkingNpc)
                {
                    UpdateNpcNonAggro(npc);

                    if (!npc.OnGround)
                        npc.VelocityY += map.gravity * map.deltaTime;

                    if (npc.VelocityY > 800)
                        npc.VelocityY = 800;

                    npc.WorldX += npc.VelocityX * map.deltaTime;
                    npc.WorldY += npc.VelocityY * map.deltaTime;

                    npc.OnGround = false;

                    ResolveNpcPlatform(npc, map.Ground, map.GroundX);
                }

                Canvas.SetLeft(npc.Visual, npc.WorldX - map.cameraX);
                Canvas.SetTop(npc.Visual, npc.WorldY);

                UpdateNpcFacing(npc);
                UpdateNpcAnimation(npc);
            }
        }

        private void UpdateNpcNonAggro(Npc npc)
        {
            if (npc.Mode == NpcMode.Stand)
            {
                npc.VelocityX = 0;
                npc.IsWaiting = false;
                npc.PatrolWaitTimer = 0;
                npc.WanderTimer = 0;
                return;
            }

            if (npc.Mode == NpcMode.Patrol)
            {
                if (npc.IsWaiting)
                {
                    npc.VelocityX = 0;
                    npc.PatrolWaitTimer -= map.deltaTime;

                    if (npc.PatrolWaitTimer <= 0)
                    {
                        npc.IsWaiting = false;
                        npc.PatrolWaitTimer = 0;

                        if (npc.MoveDir == 0) npc.MoveDir = 1;
                        npc.VelocityX = npc.MoveDir * npc.WalkSpeed;
                    }
                    return;
                }

                npc.VelocityX = npc.MoveDir * npc.WalkSpeed;

                if (npc.WorldX <= npc.LeftLimit)
                {
                    npc.WorldX = npc.LeftLimit;
                    npc.MoveDir = 1;
                    StartPatrolWait(npc);
                    return;
                }

                if (npc.WorldX >= npc.RightLimit)
                {
                    npc.WorldX = npc.RightLimit;
                    npc.MoveDir = -1;
                    StartPatrolWait(npc);
                    return;
                }

                return;
            }

            if (npc.Mode == NpcMode.Wander)
            {
                if (npc.IsWaiting)
                {
                    npc.VelocityX = 0;
                    npc.WanderTimer -= map.deltaTime;

                    if (npc.WanderTimer <= 0)
                    {
                        npc.IsWaiting = false;

                        npc.MoveDir = Rng.Next(0, 2) == 0 ? -1 : 1;

                        if (npc.WorldX <= npc.LeftLimit + 1) npc.MoveDir = 1;
                        if (npc.WorldX >= npc.RightLimit - 1) npc.MoveDir = -1;

                        npc.WanderTimer = NextDouble(3, 8);
                        npc.VelocityX = npc.MoveDir * npc.WalkSpeed;
                    }

                    return;
                }

                npc.VelocityX = npc.MoveDir * npc.WalkSpeed;
                npc.WanderTimer -= map.deltaTime;

                if (npc.WorldX <= npc.LeftLimit ||
                    npc.WorldX >= npc.RightLimit ||
                    npc.WanderTimer <= 0)
                {
                    npc.WorldX = Math.Clamp(npc.WorldX, npc.LeftLimit, npc.RightLimit);
                    StartWanderWait(npc);
                }

                return;
            }

            if (npc.Mode == NpcMode.Waypoint)
            {
                double dx = npc.TargetX - npc.WorldX;
                npc.WalkSpeed = 150;

                if (Math.Abs(dx) < 5)
                {
                    npc.WorldX = npc.TargetX;
                    npc.VelocityX = 0;
                    npc.ReachedTarget = true;

                    return;
                }

                npc.ReachedTarget = false;

                npc.MoveDir = dx > 0 ? 1 : -1;
                npc.VelocityX = npc.MoveDir * npc.WalkSpeed;

                return;
            }

            if (npc.Mode == NpcMode.Dead)
            {
                npc.VelocityX = 0;
                npc.VelocityY = 0;
                return;
            }
        }

        private void StartPatrolWait(Npc npc)
        {
            npc.IsWaiting = true;
            npc.PatrolWaitTimer = npc.PatrolWaitTime;
            npc.VelocityX = 0;
        }

        private void StartWanderWait(Npc npc)
        {
            npc.IsWaiting = true;
            npc.WanderTimer = NextDouble(3, 5);
            npc.VelocityX = 0;
        }

        private double NextDouble(double min, double max)
        {
            return min + (Rng.NextDouble() * (max - min));
        }

        private void ResolveNpcPlatform(Npc npc, Rectangle platform, double platformWorldX)
        {
            Rect npcRect = new Rect(npc.WorldX, npc.WorldY, npc.Visual.Width, npc.Visual.Height);
            Rect platformRect = new Rect(platformWorldX, Canvas.GetTop(platform), platform.Width, platform.Height);

            if (npc.VelocityY >= 0 &&
                npcRect.Bottom >= platformRect.Top &&
                npcRect.Top < platformRect.Top &&
                npcRect.Right > platformRect.Left &&
                npcRect.Left < platformRect.Right)
            {
                npc.WorldY = platformRect.Top - npcRect.Height;
                npc.VelocityY = 0;
                npc.OnGround = true;
                return;
            }

            if (npc.VelocityX > 0 &&
                npcRect.Right >= platformRect.Left &&
                npcRect.Left < platformRect.Left &&
                npcRect.Bottom > platformRect.Top &&
                npcRect.Top < platformRect.Bottom)
            {
                npc.WorldX = platformRect.Left - npcRect.Width;
                npc.VelocityX = -npc.WalkSpeed;
                return;
            }

            if (npc.VelocityX < 0 &&
                npcRect.Left <= platformRect.Right &&
                npcRect.Right > platformRect.Right &&
                npcRect.Bottom > platformRect.Top &&
                npcRect.Top < platformRect.Bottom)
            {
                npc.WorldX = platformRect.Right;
                npc.VelocityX = npc.WalkSpeed;
                return;
            }
        }

        private void UpdateNpcFacing(Npc npc)
        {
            if (npc.VelocityX > 1)
                npc.Scale.ScaleX = 1;
            else if (npc.VelocityX < -1)
                npc.Scale.ScaleX = -1;
        }

        private void UpdateNpcAnimation(Npc npc)
        {
            if (npc.CurrentAnimState == "Dead")
                return;

            BitmapImage[] frames;
            string nextState;

            if (Math.Abs(npc.VelocityX) > 1 && npc.WalkFrames != null && npc.WalkFrames.Length > 0)
            {
                frames = npc.WalkFrames;
                nextState = "Walk";
            }
            else
            {
                frames = npc.IdleFrames;
                nextState = "Idle";
            }

            if (frames == null || frames.Length == 0)
                return;

            if (npc.CurrentAnimState != nextState)
            {
                npc.CurrentAnimState = nextState;
                npc.FrameIndex = 0;
                npc.FrameTimer = 0;
                npc.Visual.Source = frames[0];
                return;
            }

            npc.FrameTimer += map.deltaTime;

            double currentDuration;

            if (nextState == "Walk")
            {
                currentDuration = npc.WalkFrameDuration;
            }
            else
            {
                if (npc.IdleFrameDurations != null && npc.IdleFrameDurations.Length > 0)
                    currentDuration = npc.IdleFrameDurations[npc.FrameIndex % npc.IdleFrameDurations.Length];
                else
                    currentDuration = 0.1;
            }

            if (npc.FrameTimer < currentDuration)
                return;

            npc.FrameTimer -= currentDuration;
            npc.FrameIndex++;

            if (npc.FrameIndex >= frames.Length)
                npc.FrameIndex = 0;

            npc.Visual.Source = frames[npc.FrameIndex];
        }
    }
}