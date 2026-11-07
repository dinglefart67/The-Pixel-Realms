using System.Windows;
using ThePixelRealms;

namespace ThePixelRealms
{
    public partial class LevelSelectWindow : Window
    {
        public LevelSelectWindow()
        {
            InitializeComponent();
        }

        private void Level1_Click(object sender, RoutedEventArgs e)
        {
            Level1 gameWindow = new Level1();
            gameWindow.Show();

            this.Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainMenuWindow menu = new MainMenuWindow();
            menu.Show();

            this.Close();
        }
        private void TestMap_Click(object sender, RoutedEventArgs e)
        {
            TestMap testMap = new TestMap();
            testMap.Show();

            this.Close();
        }

    }
}
