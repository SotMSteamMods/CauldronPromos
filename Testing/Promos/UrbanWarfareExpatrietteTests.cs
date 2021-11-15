using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CauldronPromos.Expatriette;

namespace CauldronPromosTests
{
    [TestFixture()]
    public class UrbanWarfareExpatrietteTests : RandomGameTest
    {

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(expatriette.CharacterCard, 1);
            DealDamage(villain, expatriette, 2, DamageType.Melee);
        }

        [Test()]
        public void TestLoadUrbanWarfareExpatriette()
        {
            SetupGameController("BaronBlade", "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Legacy", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(expatriette);
            Assert.IsInstanceOf(typeof(UrbanWarfareExpatrietteCharacterCardController), expatriette.CharacterCardController);

            Assert.AreEqual(29, expatriette.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestUrbanWarfareExpatrietteInnatePower()
        {
            SetupGameController("BaronBlade", "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();


            Card shotgun = PlayCard("TacticalShotgun");

            //Draw a card. You may use a power.
            DecisionSelectCards = new Card[] { baron.CharacterCard };

            QuickHandStorage(expatriette, legacy, haka, ra);
            QuickHPStorage(baron, expatriette, legacy, haka, ra);

            UsePower(expatriette);

            QuickHandCheck(1, 0, 0, 0);
            QuickHPCheck(-4, 0, 0, 0, 0);

           
        }

        [Test()]
        public void TestUrbanWarfareExpatrietteIncap1()
        {
            SetupGameController("BaronBlade", "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //One player may use a power now.
            AssertIncapLetsHeroUsePower(expatriette, 0, legacy);
            
        }

        [Test()]
        public void TestUrbanWarfareExpatrietteIncap2()
        {
            SetupGameController("BaronBlade", "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card battalion = PlayCard("BladeBattalion");
            Card redistributor = PlayCard("ElementalRedistributor");

            SetHitPoints(battalion, 1);
            SetHitPoints(redistributor, 1);

            //Destroy a target with 1 HP.
            DecisionSelectCards = new Card[] { redistributor };

            UseIncapacitatedAbility(expatriette, 1);

            AssertInTrash(baron, redistributor);
            AssertInPlayArea(baron, battalion);

        }

        [Test()]
        public void TestUrbanWarfareExpatrietteIncap3()
        {
            SetupGameController("BaronBlade", "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //Select a hero. The next time that hero uses a power, they also deal 1 projectile damage to 1 target.
            DecisionSelectTurnTakers = new TurnTaker[] { ra.TurnTaker };
            DecisionSelectCards = new Card[] { baron.CharacterCard,  baron.CharacterCard };
            UseIncapacitatedAbility(expatriette, 2);

            QuickHPStorage(baron);
            UsePower(ra);
            QuickHPCheck(-3);
        }

        #region Random Tests
        [Test]
        public void TestUrbanWarfareExpatrietteRandomGame_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                useHeroes: new List<string> { "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestUrbanWarfareExpatrietteRandomGame_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                useHeroes: new List<string> { "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestUrbanWarfareExpatrietteAndGuiseRandomGame_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                useHeroes: new List<string> { "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Guise" });
            RunGame(gameController);
        }

        [Test]
        public void TestUrbanWarfareExpatrietteAndGuiseRandomGame_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                useHeroes: new List<string> { "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter", "Guise" });
            RunGame(gameController);
        }

        [Test]
        public void TestUrbanWarfareExpatrietteRandomOblivAeonGame_Random()
        {
            GameController gameController = SetupRandomOblivAeonGameController(false,
                useHeroes: new List<string> { "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestUrbanWarfareExpatrietteRandomOblivAeonGame_Reasonable()
        {
            GameController gameController = SetupRandomOblivAeonGameController(true,
                useHeroes: new List<string> { "Expatriette/CauldronPromos.UrbanWarfareExpatrietteCharacter" });
            RunGame(gameController);
        }

        #endregion
    }
}
