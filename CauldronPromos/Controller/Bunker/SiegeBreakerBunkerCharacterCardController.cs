using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CauldronPromos.Bunker
{
    public class SiegeBreakerBunkerCharacterCardController : HeroCharacterCardController
    {
        public SiegeBreakerBunkerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Reveal the top 2 cards of your deck. Put 1 into play and 1 into your trash.
            List<MoveCardDestination> list = new List<MoveCardDestination>();
            list.Add(new MoveCardDestination(TurnTaker.PlayArea));
            list.Add(new MoveCardDestination(TurnTaker.Trash));
            IEnumerator coroutine = RevealCardsFromDeckToMoveToOrderedDestinations(HeroTurnTakerController, TurnTaker.Deck, list, sendCleanupMessageIfNecessary: true, isPutIntoPlay: true);
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
                    IEnumerator coroutine = SelectHeroToPlayCard(HeroTurnTakerController);
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
                    //Select a hero. Prevent the next damage dealt to that hero.
                    List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                    IEnumerator coroutine2 = GameController.SelectHeroCharacterCard(HeroTurnTakerController, SelectionType.PreventDamage, storedResults,cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine2);
                    }
                    if(!DidSelectCard(storedResults))
                    {
                        yield break;
                    }

                    Card selectedCard = GetSelectedCard(storedResults);

                    CannotDealDamageStatusEffect cannotDealDamageStatusEffect = new CannotDealDamageStatusEffect();
                    cannotDealDamageStatusEffect.TargetCriteria.IsSpecificCard = selectedCard;
                    cannotDealDamageStatusEffect.NumberOfUses = 1;
                    cannotDealDamageStatusEffect.UntilCardLeavesPlay(selectedCard);
                    cannotDealDamageStatusEffect.IsPreventEffect = true;
                    coroutine2 = AddStatusEffect(cannotDealDamageStatusEffect);
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
                    //One player may take a card from their trash into their hand.
                    List<SelectCardDecision> storedResults2 = new List<SelectCardDecision>();
                    IEnumerator coroutine3 = GameController.SelectCardAndStoreResults(HeroTurnTakerController, SelectionType.MoveCardToHandFromTrash, new LinqCardCriteria((Card c) => c.IsInTrash && c.Location.IsHero, "cards in a player's trash", useCardsSuffix: false), storedResults2, optional: false, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine3);
                    }

                    if(!DidSelectCard(storedResults2))
                    {
                        yield break;
                    }

                    Card selectedCard2 = GetSelectedCard(storedResults2);
                    Location hand = selectedCard2.Location.OwnerTurnTaker.ToHero().Hand;
                    coroutine = GameController.MoveCard(TurnTakerController, selectedCard2, hand,cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                    
                    break;
            }
        }
    }
}
