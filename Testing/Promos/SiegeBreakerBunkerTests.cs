using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CauldronPromos.Bunker;

namespace CauldronPromosTests
{
    [TestFixture()]
    public class SiegeBreakerBunkerTests : CauldronBaseTest
    {

        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(bunker.CharacterCard, 1);
            DealDamage(villain, bunker, 2, DamageType.Melee);
        }

        [Test()]
        public void TestLoadSiegeBreakerBunker()
        {
            SetupGameController("BaronBlade", "Bunker/CauldronPromos.SiegeBreakerBunkerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(bunker);
            Assert.IsInstanceOf(typeof(SiegeBreakerBunkerCharacterCardController), bunker.CharacterCardController);

            Assert.AreEqual(28, bunker.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestSiegeBreakerBunkerInnatePower()
        {
            SetupGameController("BaronBlade", "Bunker/CauldronPromos.SiegeBreakerBunkerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();

            Card ammoDrop = PutOnDeck("AmmoDrop");
            Card heavyPlating = PutOnDeck("HeavyPlating");

            //Reveal the top 2 cards of your deck. Put 1 into play and 1 into your trash.
            DecisionSelectCards = new Card[] { heavyPlating };
            UsePower(bunker);
            AssertInPlayArea(bunker, heavyPlating);
            AssertInTrash(ammoDrop);
        }

        [Test()]
        public void TestSiegeBreakerBunkerIncap1()
        {
            SetupGameController("BaronBlade", "Bunker/CauldronPromos.SiegeBreakerBunkerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //One player may play a card now.
            Card cardToPlay = PutInHand("NextEvolution");
            AssertIncapLetsHeroPlayCard(bunker, 0, legacy, cardToPlay.Identifier);
        }

        [Test()]
        public void TestSiegeBreakerBunkerIncap2()
        {
            SetupGameController("BaronBlade", "Bunker/CauldronPromos.SiegeBreakerBunkerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //Select a hero. Prevent the next damage dealt to that hero.
            DecisionSelectCards = new Card[] { haka.CharacterCard };
            UseIncapacitatedAbility(bunker, 1);

            QuickHPStorage(haka);
            DealDamage(baron, haka, 3, DamageType.Lightning);
            QuickHPCheckZero();

            QuickHPUpdate();
            DealDamage(baron, haka, 3, DamageType.Lightning);
            QuickHPCheck(-3);
        }

        [Test()]
        public void TestSiegeBreakerBunkerIncap3()
        {
            SetupGameController("BaronBlade", "Bunker/CauldronPromos.SiegeBreakerBunkerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card haka1 = PutInTrash("Mere");
            Card haka2 = PutInTrash("Dominion");
            Card legacy1 = PutInTrash("NextEvolution");
            Card baron1 = PutInTrash("BladeBattalion");

            //One player may take a card from their trash into their hand.
            DecisionSelectCards = new Card[] { haka2 };
            AssertNextDecisionChoices(included: new List<Card>() { haka1, haka2, legacy1 }, notIncluded: new List<Card>() { baron1 });
            
            UseIncapacitatedAbility(bunker, 2);

            AssertInHand(haka, haka2);
            AssertInTrash(haka, haka1);
            AssertInTrash(legacy, legacy1);
            AssertInTrash(baron, baron1);


        }
    }
}
