using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms
{
    public interface ICoreState
    {
        bool isPaused { get; set; }
        bool isDead { get; set; }

        double deltaTime { get; }

        int frameCount { get; set; }
        double fps { get; set; }
        DateTime lastFpsUpdate { get; set; }
    }
}
