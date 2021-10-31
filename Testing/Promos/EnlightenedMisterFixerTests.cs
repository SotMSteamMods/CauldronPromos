using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CauldronPromos.MrFixer;

namespace CauldronPromosTests
{
    [TestFixture()]
    public class EnlightenedMrFixerTests : CauldronBaseTest
    {

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(fixer.CharacterCard, 1);
            DealDamage(villain, fixer, 2, DamageType.Melee);
        }

        [Test()]
        public void TestLoadEnlightenedMrFixer()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(fixer);
            Assert.IsInstanceOf(typeof(EnlightenedMrFixerCharacterCardController), fixer.CharacterCardController);

            Assert.AreEqual(27, fixer.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestEnlightenedMrFixerInnatePower()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();

            //Mr Fixer deals 1 target 2 radiant damage. One player discards a card.
        }

        [Test()]
        public void TestEnlightenedMrFixerIncap1()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            SetupIncap(baron);

            //One player may play a card now.
        }

        [Test()]
        public void TestEnlightenedMrFixerIncap2()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            SetupIncap(baron);

            //Destroy a target with 2 or fewer HP.

        }

        [Test()]
        public void TestEnlightenedMrFixerIncap3()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            SetupIncap(baron);

            //Discard the top card of 1 deck.
        }
    }
}
