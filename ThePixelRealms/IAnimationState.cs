using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ThePixelRealms
{
    public interface IAnimationState
    {
        BitmapImage[] idleFrames { get; set; }
        BitmapImage[] walkFrames { get; set; }
        BitmapImage[] meleeFrames { get; set; }
        BitmapImage[] bowFrames { get; set; }

        BitmapImage jumpAirFrame { get; set; }
        BitmapImage shieldSprite { get; set; }

        int idleFrameIndex { get; set; }
        double idleFrameTimer { get; set; }

        int walkFrameIndex { get; set; }
        double walkFrameTimer { get; set; }

        int meleeFrameIndex { get; set; }
        double meleeFrameTimer { get; set; }

        int lastBowFrameIndex { get; set; }

        bool wasWalking { get; set; }

        double walkFrameDuration { get; }
        double meleeFrameDuration { get; }
        double[] idleFrameDurations { get; }
    }
}
