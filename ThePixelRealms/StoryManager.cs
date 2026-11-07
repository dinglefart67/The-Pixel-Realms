using System;

namespace ThePixelRealms
{
    public class StoryManager
    {
        public int CurrentStep { get; private set; } = 0;

        public void AdvanceStep()
        {
            CurrentStep++;
        }
    }
}