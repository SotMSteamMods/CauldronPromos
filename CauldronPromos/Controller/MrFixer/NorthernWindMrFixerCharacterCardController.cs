using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CauldronPromos.MrFixer
{
    public class NorthernWindMrFixerCharacterCardController : HeroCharacterCardController
    {
        public NorthernWindMrFixerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //{MrFixer} deals 1 target 0 cold damage.

            int target = GetPowerNumeral(0, 1);
            int damage = GetPowerNumeral(1, 0);

            IEnumerator coroutine = GameController.SelectTargetsAndDealDamage(HeroTurnTakerController, new DamageSource(GameController, CharacterCard), damage, DamageType.Cold, target, false, target, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //One of your non-character cards in play becomes indestructible until your next power phase.
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
            coroutine = GameController.SelectCardAndStoreResults(HeroTurnTakerController, SelectionType.Custom, new LinqCardCriteria(c => c.IsInPlayAndHasGameText && c.Owner == TurnTaker && !c.IsCharacter, "non-character"), storedResults, false, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidSelectCard(storedResults))
            {
                yield break;
            }

            Card selectedCard = GetSelectedCard(storedResults);

            MakeIndestructibleStatusEffect effect = new MakeIndestructibleStatusEffect();
            effect.CardsToMakeIndestructible.IsSpecificCard = selectedCard;
            effect.ToTurnPhaseExpiryCriteria.TurnTaker = TurnTaker;
            effect.ToTurnPhaseExpiryCriteria.Phase = Phase.UsePower;
            coroutine = AddStatusEffect(effect);
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
                    //One player may use a power now.
                    IEnumerator coroutine = GameController.SelectHeroToUsePower(HeroTurnTakerController, cardSource: GetCardSource());
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
                    //Destroy an environment card.
                    IEnumerator coroutine2 = GameController.SelectAndDestroyCard(HeroTurnTakerController, new LinqCardCriteria(c => c.IsEnvironment, "environment"), optional: false, cardSource: GetCardSource());
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
                    //Put a card in a trash pile under its associated deck.
                    List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                    IEnumerator coroutine3 = GameController.SelectCardAndStoreResults(HeroTurnTakerController, SelectionType.MoveCardOnBottomOfDeck, new LinqCardCriteria((Card c) => c.IsInTrash, "cards in any trash", useCardsSuffix: false), storedResults,optional: false, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine3);
                    }

                    if(!DidSelectCard(storedResults))
                    {
                        yield break;
                    }

                    Card selectedCard = GetSelectedCard(storedResults);
                    Location deck = selectedCard.Owner.Deck;
                    coroutine3 = GameController.MoveCard(HeroTurnTakerController, selectedCard, deck, toBottom: true, cardSource: GetCardSource());
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

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {

            return new CustomDecisionText("Select a card to make indestructible until your next power phase.", "Select a card to make indestructible until their next power phase.", "Vote for which card to make indestructible until their next power phase.", "card to become indestructible");

        }
    }
}
