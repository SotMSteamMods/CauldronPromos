using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CauldronPromos.AbsoluteZero;

namespace CauldronPromosTests
{
    [TestFixture()]
    public class NitroBoostAbsoluteZeroTests : RandomGameTest
    {

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(az.CharacterCard, 1);
            DealDamage(villain, az, 2, DamageType.Melee);
        }

        [Test()]
        public void TestLoadNitroBoostAbsoluteZero()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Legacy", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(az);
            Assert.IsInstanceOf(typeof(NitroBoostAbsoluteZeroCharacterCardController), az.CharacterCardController);

            Assert.AreEqual(28, az.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestNitroBoostAbsoluteZeroInnatePower()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            Card focusedApertures = PutInHand("FocusedApertures");

            //Play a card. Absolute Zero deals himself 1 fire and 1 cold damage.
            DecisionSelectCards = new Card[] { focusedApertures };
            QuickHPStorage(baron, az, legacy, haka, ra);
            UsePower(az);
            QuickHPCheck(0, -3, 0, 0, 0);
            AssertInPlayArea(az, focusedApertures);
        }

        [Test()]
        public void TestNitroBoostAbsoluteZeroIncap1()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //One player may use a power now.
            AssertIncapLetsHeroUsePower(az, 0, legacy);
            
        }

        [Test()]
        public void TestNitroBoostAbsoluteZeroIncap2()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //Select a hero. Increase the next damage dealt by that hero by 2.
            DecisionSelectCards = new Card[] { haka.CharacterCard };

            UseIncapacitatedAbility(az, 1);

            QuickHPStorage(baron);
            DealDamage(haka, baron, 2, DamageType.Melee);
            QuickHPCheck(-4);

            //should expire after 1 use
            QuickHPUpdate();
            DealDamage(haka, baron, 2, DamageType.Melee);
            QuickHPCheck(-2);

        }

        [Test()]
        public void TestNitroBoostAbsoluteZeroIncap3()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card battalion = PlayCard("BladeBattalion");
            Card redistributor = PlayCard("ElementalRedistributor");

            SetHitPoints(battalion, 1);
            SetHitPoints(redistributor, 1);

            //Destroy a target with 1 HP.
            DecisionSelectCards = new Card[] { redistributor };

            UseIncapacitatedAbility(az, 2);

            AssertInTrash(baron, redistributor);
            AssertInPlayArea(baron, battalion);

        }

        #region Random Tests
        [Test]
        public void TestNitroBoostAbsoluteZeroRandomGame_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                useHeroes: new List<string> { "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestNitroBoostAbsoluteZeroRandomGame_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                useHeroes: new List<string> { "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestNitroBoostAbsoluteZeroAndGuiseRandomGame_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                useHeroes: new List<string> { "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Guise" });
            RunGame(gameController);
        }

        [Test]
        public void TestNitroBoostAbsoluteZeroAndGuiseRandomGame_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                useHeroes: new List<string> { "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter", "Guise" });
            RunGame(gameController);
        }

        [Test]
        public void TestNitroBoostAbsoluteZeroRandomOblivAeonGame_Random()
        {
            GameController gameController = SetupRandomOblivAeonGameController(false,
                useHeroes: new List<string> { "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestNitroBoostAbsoluteZeroRandomOblivAeonGame_Reasonable()
        {
            GameController gameController = SetupRandomOblivAeonGameController(true,
                useHeroes: new List<string> { "AbsoluteZero/CauldronPromos.NitroBoostAbsoluteZeroCharacter" });
            RunGame(gameController);
        }

        #endregion
    }
}
