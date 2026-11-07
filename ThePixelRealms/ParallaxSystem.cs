using System.Windows;
using System.Windows.Controls;

namespace ThePixelRealms
{
    /// <summary>
    /// Reusable horizontal parallax system.
    ///
    /// Factor:
    /// 1.0 = normal world movement
    /// 0.0 = fixed to the screen
    /// 0.1 = very distant background
    /// 0.5 = mid-distance background
    /// >1.0 = foreground, moves faster than the world
    ///
    /// Usage from any level XAML:
    /// local:ParallaxSystem.Factor="0.5"
    /// </summary>
    public static class ParallaxSystem
    {
        public static readonly DependencyProperty FactorProperty =
            DependencyProperty.RegisterAttached(
                "Factor",
                typeof(double),
                typeof(ParallaxSystem),
                new FrameworkPropertyMetadata(1.0));

        public static void SetFactor(DependencyObject element, double value)
        {
            element.SetValue(FactorProperty, value);
        }

        public static double GetFactor(DependencyObject element)
        {
            return (double)element.GetValue(FactorProperty);
        }

        public static void UpdateElement(FrameworkElement element, double worldX, double cameraX)
        {
            double factor = GetFactor(element);
            Canvas.SetLeft(element, worldX - (cameraX * factor));
        }
    }
}
