using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePixelRealms.Tests
{
    internal class PlayerMovementTests
    {
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void Jump_SetsNegativeVelocity()
        {
            var map = new FakeGameState();
            var movement = new PlayerMovement(map);

            map.spacePressed = true;
            map.isOnGround = true;

            movement.HandleMovement();

            Assert.Less(map.playerVelocityY, 0);
        }
    }
}
