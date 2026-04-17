using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ThePixelRealms.Tests
{
    public class PlayerCombatTests
    {
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void DamagePlayer_ReducesHp()
        {
            var map = new FakeGameState();
            var enemyCombat = new EnemyCombat(map);
            var physics = new GamePhysics(map);
            var combat = new PlayerCombat(map, enemyCombat, physics);
            map.Player = new Image();
            map.GameCanvas = new Canvas();
            map.RootGrid = new Grid();
            map.enemies = new List<Enemy>();
            map.arrows = new List<Arrow>();

            map.currentHp = 100;

            combat.DamagePlayer(30);

            Assert.AreEqual(70, map.currentHp);
        }
    }
}
