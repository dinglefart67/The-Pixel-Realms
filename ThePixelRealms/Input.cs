using System.Windows;
using System.Windows.Input;

namespace ThePixelRealms
{
    public class Input
    {
        public IGameState map;

        public Input(IGameState map)
        {
            this.map = map;
        }

        public bool EPressed = false;

        public void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (map.isDead)
                return;

            if (map.isPaused && !map.cutscene.IsActive && e.Key != Key.Escape)
                return;

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == GameKeyBindings.Get("Bow"))
                map.bowKeyPressed = true;
            else if (key == Key.E)
                EPressed = true;
            else if (key == GameKeyBindings.Get("Shield"))
            {
                if (map.currentShield > 0)
                    map.isShielding = true;
            }
            else if (key == GameKeyBindings.Get("Run"))
                map.shiftPressed = true;
            else if (key == Key.H)
            {
                map.currentHp -= 10;
                if (map.currentHp < 0)
                    map.currentHp = 0;
            }
            else if (key == GameKeyBindings.Get("MoveLeft") || key == Key.Left)
                map.leftPressed = true;
            else if (key == GameKeyBindings.Get("MoveRight") || key == Key.Right)
                map.rightPressed = true;
            else if (key == GameKeyBindings.Get("Jump") || key == Key.Up)
                map.spacePressed = true;
            else if (key == Key.Escape)
                map.TogglePause();
            else if (key == GameKeyBindings.Get("Sword"))
                map.attackPressed = true;
            else if (key == Key.F1)
                map.DebugText.Visibility = map.DebugText.Visibility == Visibility.Visible
                    ? Visibility.Hidden
                    : Visibility.Visible;

            if (key == GameKeyBindings.Get("Jump"))
                e.Handled = true;
        }

        public void Window_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == GameKeyBindings.Get("Bow"))
            {
                map.bowKeyPressed = false;
                map.ReleaseArrow();
            }
            else if (key == GameKeyBindings.Get("Shield"))
            {
                map.isShielding = false;

                if (map.idleFrames != null && map.idleFrames.Length > 0)
                {
                    map.idleFrameIndex = 0;
                    map.idleFrameTimer = 0;
                    map.Player.Source = map.idleFrames[0];
                }
            }
            else if (key == GameKeyBindings.Get("Run"))
                map.shiftPressed = false;
            else if (key == GameKeyBindings.Get("MoveLeft") || key == Key.Left)
                map.leftPressed = false;
            else if (key == GameKeyBindings.Get("MoveRight") || key == Key.Right)
                map.rightPressed = false;
            else if (key == GameKeyBindings.Get("Jump") || key == Key.Up)
                map.spacePressed = false;
            else if (key == GameKeyBindings.Get("Sword"))
                map.attackPressed = false;
            else if (key == Key.E)
                EPressed = false;
        }
    }
}
