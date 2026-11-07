using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ThePixelRealms
{
    public interface IWorldState
    {
        Canvas GameCanvas { get; }
        Grid RootGrid { get; }

        double cameraX { get; set; }

        double WorldWidth { get; }
        double screenLeftLimit { get; }
        double screenRightLimit { get; }

        Rectangle Ground { get; }
        double GroundX { get; }

        List<Rectangle> platforms { get; }

        double gravity { get; }
    }
}
