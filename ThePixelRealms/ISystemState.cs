using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms
{
    public interface ISystemState
    {
        DialogueSystem dialogueSystem { get; }
        CutsceneSystem cutscene { get; }
        GuideSystem guideSystem { get; }
        UIAndDebug uiAndDebug { get; }
    }
}
