using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CauldronPromos.OmnitronX;

namespace CauldronPromosTests
{
    [TestFixture()]
    public class OmnitronXITests : CauldronBaseTest
    {

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(omnix.CharacterCard, 1);
            DealDamage(villain, omnix, 2, DamageType.Melee);
        }

        [Test()]
        public void TestLoadOmnitronXI()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(omnix);
            Assert.IsInstanceOf(typeof(OmnitronXICharacterCardController), omnix.CharacterCardController);

            Assert.AreEqual(29, omnix.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestOmnitronXIInnatePower()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            GoToPlayCardPhase(omnix);
            Card focusedPlasmaCannon = PlayCard("FocusedPlasmaCannon");
            Card gaussianCoilBlaster = PlayCard("GaussianCoilBlaster");


            GoToUsePowerPhase(omnix);
            //Select 1 component in play. That component's start of turn effects act at the end of this turn as well.
            DecisionSelectCards = new Card[] { focusedPlasmaCannon, baron.CharacterCard };
            UsePower(omnix);

            PrintSpecialStringsForCard(omnix.CharacterCard);

            QuickHPStorage(baron);
            GoToEndOfTurn(omnix);
            QuickHPCheck(-2);

            DestroyCard(gaussianCoilBlaster);


            //should only be the end phase of the turn it was activated on
            GoToPlayCardPhase(omnix);
            QuickHPUpdate();
            GoToEndOfTurn(omnix);
            QuickHPCheckZero();
        }

        [Test()]
        public void TestOmnitronXIInnatePower_OutOfTurn()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy/AmericasGreatestLegacyCharacter", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            GoToPlayCardPhase(omnix);
            Card focusedPlasmaCannon = PlayCard("FocusedPlasmaCannon");
            Card gaussianCoilBlaster = PlayCard("GaussianCoilBlaster");


            GoToUsePowerPhase(legacy);
            //Select 1 component in play. That component's start of turn effects act at the end of this turn as well.
            DecisionSelectCards = new Card[] { omnix.CharacterCard, focusedPlasmaCannon, baron.CharacterCard };
            UsePower(legacy);

            PrintSpecialStringsForCard(omnix.CharacterCard);

            QuickHPStorage(baron);
            GoToEndOfTurn(legacy);
            QuickHPCheck(-2);

            
        }

        [Test()]
        public void TestOmnitronXIInnatePower_Guise()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Guise/CompletionistGuiseCharacter", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            GoToPlayCardPhase(omnix);
            Card focusedPlasmaCannon = PlayCard("FocusedPlasmaCannon");
            Card gaussianCoilBlaster = PlayCard("GaussianCoilBlaster");


            GoToUsePowerPhase(guise);

            //Select 1 component in play. That component's start of turn effects act at the end of this turn as well.
            DecisionSelectCards = new Card[] { omnix.CharacterCard, focusedPlasmaCannon, baron.CharacterCard };
            DecisionSelectFunction = 0;
            UsePower(guise);

            QuickHPStorage(baron);
            GoToEndOfTurn(guise);
            QuickHPCheck(-2);


        }

        [Test()]
        public void TestOmnitronXIInnatePower_OmniIV()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Haka", "Ra", "OmnitronIV");
            StartGame();
            DestroyNonCharacterVillainCards();

            GoToPlayCardPhase(omnix);
            Card focusedPlasmaCannon = PlayCard("FocusedPlasmaCannon");
            Card conveyorPanels = PlayCard("ConveyorPanels");
            Card partialOmniDrone = PutOnDeck("PartialOmniDrone");

            GoToUsePowerPhase(omnix);
            //Select 1 component in play. That component's start of turn effects act at the end of this turn as well.
            DecisionSelectCards = new Card[] { conveyorPanels};
            UsePower(omnix);

            PrintSpecialStringsForCard(omnix.CharacterCard);

            GoToEndOfTurn(omnix);
            AssertInPlayArea(env, partialOmniDrone);
        }

        [Test()]
        public void TestOmnitronXIIncap1()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card policeBackup = PlayCard("PoliceBackup");
            Card trafficPileup = PlayCard("TrafficPileup");

            //Destroy an environment card.
            DecisionSelectCards = new Card[] { trafficPileup };
            UseIncapacitatedAbility(omnix, 0);
            AssertInTrash(env, trafficPileup);
            AssertInPlayArea(env, policeBackup);
        }

        [Test()]
        public void TestOmnitronXIIncap2()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card battalion = PlayCard("BladeBattalion");
            //The target with the lowest HP deals 1 target 1 irreducible fire damage.
            DecisionSelectCards = new Card[] { baron.CharacterCard };
            
            QuickHPStorage(baron.CharacterCard, battalion, legacy.CharacterCard, haka.CharacterCard, ra.CharacterCard);
            UseIncapacitatedAbility(omnix, 1);
            QuickHPCheck(-1, 0, 0, 0, 0);

        }


        [Test()]
        public void TestOmnitronXIIncap3()
        {
            SetupGameController("BaronBlade", "OmnitronX/CauldronPromos.OmnitronXICharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);


            Card nextEvolution = PutOnDeck("NextEvolution");
            //Select a deck and put its top card into play.
            DecisionSelectLocations = new LocationChoice[] { new LocationChoice(legacy.TurnTaker.Deck) };
            UseIncapacitatedAbility(omnix, 2);
            AssertInPlayArea(legacy, nextEvolution);

        }
    }
}
