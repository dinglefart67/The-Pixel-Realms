using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms.Tests
{
    internal class UIAndDebugTests
    {
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void TogglePause_ChangesPauseState()
        {
            var map = new FakeGameState();
            var ui = new UIAndDebug(map);

            map.isPaused = false;

            ui.TogglePause();

            Assert.IsTrue(map.isPaused);

            ui.TogglePause();

            Assert.IsFalse(map.isPaused);
        }
    }
}
