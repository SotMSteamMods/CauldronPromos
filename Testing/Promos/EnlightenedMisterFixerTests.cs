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
            DestroyNonCharacterVillainCards();

            //Mr Fixer deals 1 target 2 radiant damage. One player discards a card.
            Card cardToDiscard = GetRandomCardFromHand(legacy);
            DecisionSelectCards = new Card[] { baron.CharacterCard, cardToDiscard };
            DecisionSelectTurnTakers = new TurnTaker[] { legacy.TurnTaker };

            QuickHPStorage(baron, fixer, legacy, haka, ra);
            QuickHandStorage(fixer, legacy, haka, ra);

            UsePower(fixer);

            QuickHPCheck(-2, 0, 0, 0, 0);
            QuickHandCheck(0, -1, 0, 0);
            AssertInTrash(legacy, cardToDiscard);
            


        }

        [Test()]
        public void TestEnlightenedMrFixerIncap1()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            //One player may play a card now.
            Card cardToPlay = PutInHand("NextEvolution");
            AssertIncapLetsHeroPlayCard(fixer, 0, legacy, cardToPlay.Identifier);
        }

        [Test()]
        public void TestEnlightenedMrFixerIncap2()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card battalion = PlayCard("BladeBattalion");
            Card redistributor = PlayCard("ElementalRedistributor");

            SetHitPoints(battalion, 2);
            SetHitPoints(redistributor, 1);

            //Destroy a target with 2 or fewer HP.

            DecisionSelectCards = new Card[] { redistributor };

            UseIncapacitatedAbility(fixer, 1);

            AssertInTrash(baron, redistributor);
            AssertInPlayArea(baron, battalion);

        }

        [Test()]
        public void TestEnlightenedMrFixerIncap3()
        {
            SetupGameController("BaronBlade", "MrFixer/CauldronPromos.EnlightenedMrFixerCharacter", "Legacy", "Haka", "Ra", "Megalopolis");
            StartGame();
            DestroyNonCharacterVillainCards();
            SetupIncap(baron);

            Card baronTopCard = baron.TurnTaker.Deck.TopCard;
            Card legacyTopCard = legacy.TurnTaker.Deck.TopCard;
            Card hakaTopCard = haka.TurnTaker.Deck.TopCard;
            Card raTopCard = ra.TurnTaker.Deck.TopCard;

            //Discard the top card of 1 deck.
            DecisionSelectLocations = new LocationChoice[] { new LocationChoice(haka.TurnTaker.Deck) };

            UseIncapacitatedAbility(fixer, 2);

            AssertOnTopOfDeck(baronTopCard);
            AssertOnTopOfDeck(legacyTopCard);
            AssertInTrash(hakaTopCard);
            AssertOnTopOfDeck(raTopCard);


        }
    }
}
