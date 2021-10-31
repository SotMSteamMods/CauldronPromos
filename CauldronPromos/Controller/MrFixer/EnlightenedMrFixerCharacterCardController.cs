using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CauldronPromos.MrFixer
{
    public class EnlightenedMrFixerCharacterCardController : HeroCharacterCardController
    {
        public EnlightenedMrFixerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Mr Fixer deals 1 target 2 radiant damage.

            int target = GetPowerNumeral(0, 1);
            int damage = GetPowerNumeral(1, 2);

            IEnumerator coroutine = GameController.SelectTargetsAndDealDamage(HeroTurnTakerController, new DamageSource(GameController, CharacterCard), damage, DamageType.Radiant, target, false, target, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            // One player discards a card.
            coroutine = GameController.SelectHeroToDiscardCard(HeroTurnTakerController, optionalDiscardCard: false, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                    //One player may play a card now.
                    IEnumerator coroutine = SelectHeroToPlayCard(DecisionMaker);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }

                    break;
                case 1:
                    //Destroy a target with 2 or fewer HP.
                    IEnumerator coroutine2 = GameController.SelectAndDestroyCard(HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints <= 2, "targets with 2 or fewer HP", useCardsSuffix: false), optional: false, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine2);
                    }
                    break;
                case 2:
                    //Discard the top card of 1 deck.
                    List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
                    IEnumerable<TurnTaker> possibleTurnTakers = Game.TurnTakers.Where(tt => !tt.IsIncapacitatedOrOutOfGame);
                    List<Location> possibleLocationsList = new List<Location>();
                    foreach (TurnTaker tt in possibleTurnTakers)
                    {
                        if (GameController.IsLocationVisibleToSource(tt.Deck, GetCardSource()))
                        {
                            possibleLocationsList.Add(tt.Deck);
                        }
                       
                        foreach (Location subdeck in tt.SubDecks)
                        {
                            if (subdeck.IsRealDeck && GameController.IsLocationVisibleToSource(subdeck, GetCardSource()))
                            {
                                possibleLocationsList.Add(subdeck);
                            }
                        }
                    }
                    List<LocationChoice> possibleDecks = possibleLocationsList.Select(loc => new LocationChoice(loc)).ToList();

                    IEnumerator coroutine3 = GameController.SelectLocation(HeroTurnTakerController, possibleDecks, SelectionType.DiscardFromDeck, storedResults, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine3);
                    }
                    if(!DidSelectLocation(storedResults))
                    {
                        yield break;
                    }

                    Location selectedDeck = GetSelectedLocation(storedResults);
                    coroutine3 = GameController.DiscardTopCards(HeroTurnTakerController, selectedDeck, 1, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine3);
                    }
                    break;
            }

        }
    }
}
