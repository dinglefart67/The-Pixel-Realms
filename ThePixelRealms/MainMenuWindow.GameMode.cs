using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace ThePixelRealms
{
    public partial class MainMenuWindow
    {
        private void StoryMode_Click(object sender, RoutedEventArgs e)
        {
            new Level1().Show();
            Close();
        }

        // A folytatás funkcióját később kötjük be.
        private void ContinueStory_Click(object sender, RoutedEventArgs e)
        {
        }

        // -------------------- STORY / ARCADE HOVER --------------------

        private void Story_MouseEnter(object sender, MouseEventArgs e)
        {
            SetGameModeHover(StoryImage, StoryTitle, StoryDescription, true, true);
            StorySubButtons.Visibility = Visibility.Visible;
        }

        private void Story_MouseLeave(object sender, MouseEventArgs e)
        {
            SetGameModeHover(StoryImage, StoryTitle, StoryDescription, false, true);
            StorySubButtons.Visibility = Visibility.Collapsed;
        }

        private void Arcade_MouseEnter(object sender, MouseEventArgs e) =>
            SetGameModeHover(ArcadeImage, ArcadeTitle, ArcadeDescription, true, false);

        private void Arcade_MouseLeave(object sender, MouseEventArgs e) =>
            SetGameModeHover(ArcadeImage, ArcadeTitle, ArcadeDescription, false, false);

        private void SetGameModeHover(Image image, TextBlock title, TextBlock description, bool active, bool story)
        {
            image.Effect = active ? new BlurEffect { Radius = 15, KernelType = KernelType.Gaussian } : null;
            title.Visibility = active ? Visibility.Collapsed : Visibility.Visible;
            description.Visibility = active ? Visibility.Visible : Visibility.Collapsed;

            Image[] border = story
                ? new[] { StoryUpLeft, StoryUpRight, StoryDownLeft, StoryDownRight, StoryUp, StoryDown, StoryLeft, StoryRight }
                : new[] { ArcadeUpLeft, ArcadeUpRight, ArcadeDownLeft, ArcadeDownRight, ArcadeUp, ArcadeDown, ArcadeLeft, ArcadeRight };

            SetBigBorder(border, active);
        }

        private void SetBigBorder(Image[] border, bool active)
        {
            string suffix = active ? "_Active.png" : ".png";
            string[] parts = { "UpLeft", "UpRight", "DownLeft", "DownRight", "Up", "Down", "Left", "Right" };

            for (int i = 0; i < border.Length; i++)
                border[i].Source = LoadMenuImage($"BigBorder_{parts[i]}{suffix}");
        }
    }
}
