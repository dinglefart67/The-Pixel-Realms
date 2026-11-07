using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms
{
    public interface IInputState
    {
        Input input { get; }

        bool leftPressed { get; set; }
        bool rightPressed { get; set; }
        bool spacePressed { get; set; }

        bool shiftPressed { get; set; }
    }
}
