using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms.Tests
{
    public class StoryManagerTests
    {
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void StoryTest()
        {
            var story = new StoryManager();

            Assert.AreEqual(0, story.CurrentStep);
        }

        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void AdvanceStepTest()
        {
            var story = new StoryManager();

            story.AdvanceStep();

            Assert.AreEqual(1, story.CurrentStep);
        }
    }
}
