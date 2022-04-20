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
    public class NorthernWindMrFixerTests : RandomGameTest
    {

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(fixer.CharacterCard, 1);
            DealDamage(villain, fixer, 2, DamageType.Melee);
        }

        [Test()]
        public void TestLoadNorthernWindMrFixer()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(fixer);
            Assert.IsInstanceOf(typeof(NorthernWindMrFixerCharacterCardController), fixer.CharacterCardController);

            Assert.AreEqual(26, fixer.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestNorthernWindMrFixerInnatePower()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            GoToPlayCardPhase(fixer);
            Card pipeWrench = PlayCard("PipeWrench");
            GoToUsePowerPhase(fixer);

            //Mr Fixer deals 1 target 0 cold damage. One of your non-character cards in play becomes indestructible until your next power phase
            DecisionSelectCards = new Card[] { baron.CharacterCard, pipeWrench};
            DecisionAutoDecideIfAble = true;
            QuickHPStorage(baron, fixer, legacy, haka, ra);
            UsePower(fixer);
            QuickHPCheck(-1, 0, 0, 0, 0);

            DestroyCard(pipeWrench);
            AssertInPlayArea(fixer, pipeWrench);
            Card tireIron = PlayCard("TireIron");
            AssertInPlayArea(fixer, pipeWrench);
            AssertInPlayArea(fixer, tireIron);

            GoToUsePowerPhase(fixer);
            Card dualCrowbars = PlayCard("DualCrowbars");
            AssertInPlayArea(fixer, dualCrowbars);
            AssertInHand(fixer, pipeWrench);
            AssertInHand(fixer, tireIron);

        }

        [Test()]
        public void TestNorthernWindMrFixerInnatePower_GuiseCopies()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Guise", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            GoToPlayCardPhase(fixer);
            Card pipeWrench = PlayCard("PipeWrench");

            GoToPlayCardPhase(guise);
            Card blatantReference = PlayCard("BlatantReference");
            Card guiseTheBarbarian = PlayCard("GuiseTheBarbarian");

            Card iCanDoThatToo = GetCard("ICanDoThatToo");


            //Mr Fixer deals 1 target 0 cold damage. One of your non-character cards in play becomes indestructible until your next power phase
            DecisionSelectPowers = new Card[] { fixer.CharacterCard };
            DecisionSelectCards = new Card[] { baron.CharacterCard, guiseTheBarbarian };

            AssertNextDecisionsChoices(included: new List<IEnumerable<Card>>()
            {
                new List<Card>()
                {
                    fixer.CharacterCard, guise.CharacterCard, haka.CharacterCard, ra.CharacterCard
                },
                new List<Card>()
                {
                    blatantReference, guiseTheBarbarian
                },
                                new List<Card>()
                {
                    baron.CharacterCard, fixer.CharacterCard, guise.CharacterCard, haka.CharacterCard, ra.CharacterCard
                }

            }, notIncluded: new List<IEnumerable<Card>>()
            {
                new List<Card>()
                {
                },
                new List<Card>()
                {
                    iCanDoThatToo, guise.CharacterCard
                },
                new List<Card>()
                {
                }

            });

            PlayCard(iCanDoThatToo);



        }

        [Test()]
        public void TestNorthernWindMrFixerIncap1()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //One player may use a power now.
            AssertIncapLetsHeroUsePower(fixer, 0, legacy);
            
        }

        [Test()]
        public void TestNorthernWindMrFixerIncap2()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card policeBackup = PlayCard("PoliceBackup");
            Card trafficPileup = PlayCard("TrafficPileup");

            //Destroy an environment card.
            DecisionSelectCards = new Card[] { trafficPileup };
            UseIncapacitatedAbility(fixer, 1);
            AssertInTrash(env, trafficPileup);
            AssertInPlayArea(env, policeBackup);

        }

        [Test()]
        public void TestNorthernWindMrFixerIncap3()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card haka1 = PutInTrash("Mere");
            Card haka2 = PutInTrash("Dominion");
            Card legacy1 = PutInTrash("NextEvolution");
            Card baron1 = PutInTrash("BladeBattalion");

            //Put a card in a trash pile under its associated deck.
            DecisionSelectCards = new Card[] { haka2 };
            AssertNextDecisionChoices(included: new List<Card>() { haka1, haka2, legacy1, baron1 });

            UseIncapacitatedAbility(fixer, 2);

            AssertOnBottomOfDeck(haka2);
            AssertInTrash(haka, haka1);
            AssertInTrash(legacy, legacy1);
            AssertInTrash(baron, baron1);

        }

        #region Random Tests
        [Test]
        public void TestNorthernWindMrFixerRandomGame_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                useHeroes: new List<string> { "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestNorthernWindMrFixerRandomGame_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                useHeroes: new List<string> { "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestNorthernWindMrFixerAndGuiseRandomGame_Random()
        {
            GameController gameController = SetupRandomGameController(false,
                useHeroes: new List<string> { "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Guise" });
            RunGame(gameController);
        }

        [Test]
        public void TestNorthernWindMrFixerAndGuiseRandomGame_Reasonable()
        {
            GameController gameController = SetupRandomGameController(true,
                useHeroes: new List<string> { "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter", "Guise" });
            RunGame(gameController);
        }

        [Test]
        public void TestNorthernWindMrFixerRandomOblivAeonGame_Random()
        {
            GameController gameController = SetupRandomOblivAeonGameController(false,
                useHeroes: new List<string> { "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter" });
            RunGame(gameController);
        }

        [Test]
        public void TestNorthernWindMrFixerRandomOblivAeonGame_Reasonable()
        {
            GameController gameController = SetupRandomOblivAeonGameController(true,
                useHeroes: new List<string> { "MrFixer/CauldronPromos.NorthernWindMrFixerCharacter" });
            RunGame(gameController);
        }

        #endregion
    }
}
