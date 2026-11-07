using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ThePixelRealms
{
    public interface INpcState
    {
        List<Npc> npcs { get; }

        Npc currentNpc { get; set; }

        bool isGreetingActive { get; set; }
        double npcSpeechTimer { get; set; }

        TextBlock npcSpeech { get; }
        Image npcBubbleLeft { get; }
        Image npcBubbleMiddle { get; }
        Image npcBubbleRight { get; }

        TextBlock interactionHint { get; }

        Dictionary<int, NpcType> talkSequence { get; }
        HashSet<NpcType> greetingAllowedTypes { get; }
    }
}
