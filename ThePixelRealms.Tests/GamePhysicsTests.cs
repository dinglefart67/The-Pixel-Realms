using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms.Tests
{
    internal class GamePhysicsTests
    {
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void ApplyPhysics_AddsGravityToVelocity()
        {
            var map = new FakeGameState();
            var physics = new GamePhysics(map);

            map.playerVelocityY = 0;
            map.deltaTime = 1.0;

            physics.ApplyPhysics();

            Assert.Greater(map.playerVelocityY, 0);
        }
    }
}
