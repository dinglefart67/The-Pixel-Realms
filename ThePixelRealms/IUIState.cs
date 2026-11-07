using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public interface IUIState
    {
        Image Player { get; }
        UIElement Dead { get; }

        TextBlock DebugText { get; }

        Rectangle HpBarFill { get; }

        UIElement PauseOverlay { get; }

        ScaleTransform PlayerScaleTransform { get; }
    }
}
