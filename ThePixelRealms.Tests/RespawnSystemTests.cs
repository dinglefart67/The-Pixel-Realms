using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ThePixelRealms.Tests
{
    public class RespawnSystemTests
    {
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void Respawn_SetsPlayerToCheckpoint_AndRestoresHp()
        {
            var map = new FakeGameState();

            map.Player = new Image();
            map.GameCanvas = new Canvas();
            map.RootGrid = new Grid();

            var respawn = new RespawnSystem(map);

            respawn.SetCheckpoint(100, 200);
            respawn.RespawnPlayer();

            Assert.AreEqual(100, map.playerWorldX);
            Assert.AreEqual(200, map.playerWorldY);

            // checkpoint beállítás
            respawn.SetCheckpoint(100, 200);

            // előtte "halott" állapot
            map.currentHp = 0;
            map.currentShield = 50;
            map.playerWorldX = 999;
            map.playerWorldY = 999;
            map.isDead = true;

            respawn.RespawnPlayer();

            Assert.AreEqual(map.maxHp, map.currentHp);
            Assert.AreEqual(0, map.currentShield);

            Assert.AreEqual(100, map.playerWorldX);
            Assert.AreEqual(200, map.playerWorldY);

            Assert.IsFalse(map.isDead);
            Assert.IsFalse(map.isPaused);
        }
    }
}
