using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace CauldronPromos.OmnitronX
{
    public class OmnitronXICharacterCardController : HeroCharacterCardController
    {
        public OmnitronXICharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => $"{string.Join(" and ", OverclockedCardsInPlay.Select(c => c.Title).ToArray())} {OverclockedCardsInPlay.Count().ToString_IsOrAre()} overclocked and will act at the end of this turn.").Condition = () => OverclockedCardsInPlay.Any();
        }

        public static readonly string OverclockedCardsKey = "OverclockedCards";
        private bool IsOverclocked(Card c)
        {
            IEnumerable<string> overclockedCardsQualifiedIdentifiers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey);
            if (overclockedCardsQualifiedIdentifiers != null && overclockedCardsQualifiedIdentifiers.Any(overclockedQualifiedIdentifier => c.QualifiedIdentifier == overclockedQualifiedIdentifier))
            {
                return true;
            }
 
            return false;
        }
        private IEnumerable<Card> OverclockedCardsInPlay => FindCardsWhere(c => c.IsInPlayAndHasGameText && IsOverclocked(c));

        public override IEnumerator UsePower(int index = 0)
        {
            //Select 1 component in play. That component's start of turn effects act at the end of this turn as well.
            int numComponents = GetPowerNumeral(0, 1);
            List<SelectCardsDecision> storedResults = new List<SelectCardsDecision>();
            IEnumerator coroutine = GameController.SelectCardsAndStoreResults(HeroTurnTakerController, SelectionType.Custom, c => c.IsComponent && c.IsInPlayAndHasGameText, numComponents, storedResults, optional: false, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidSelectCards(storedResults))
            {
                yield break;
            }

            IEnumerable<Card> selectedCards = GetSelectedCards(storedResults);

            string description = $"{string.Join(" and ", selectedCards.Select(c => c.Title).ToArray())} {selectedCards.Count().ToString_IsOrAre()} overclocked and will act at the end of {Game.ActiveTurnTaker.Name}'s turn.";
            OnPhaseChangeStatusEffect phaseChangeStatusEffect = new OnPhaseChangeStatusEffect(CardWithoutReplacements, "OverclockComponentEndOfTurnEffect", description, new TriggerType[] { TriggerType.ActivateTriggers }, Card);
            phaseChangeStatusEffect.TurnPhaseCriteria.Phase = Phase.End;
            phaseChangeStatusEffect.TurnTakerCriteria.IsSpecificTurnTaker = Game.ActiveTurnTaker;
            phaseChangeStatusEffect.Identifier = String.Join(",", selectedCards.Select(c => c.QualifiedIdentifier).ToArray());
            phaseChangeStatusEffect.UntilThisTurnIsOver(Game);
            coroutine = AddStatusEffect(phaseChangeStatusEffect);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            // update the journal so we can reference the card name with a Special String
            IEnumerable<string> overclockedCards = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey);
            List<string> updatedListOfCards = new List<string>();
            if (overclockedCards != null)
            {
                updatedListOfCards = overclockedCards.ToList();
            }
            foreach (Card c in selectedCards)
            {
                if (!updatedListOfCards.Contains(c.QualifiedIdentifier))
                {
                    updatedListOfCards.Add(c.QualifiedIdentifier);
                }
            }
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedCardsKey, updatedListOfCards);

            yield return DoNothing();
        }

        public IEnumerator OverclockComponentEndOfTurnEffect(PhaseChangeAction pca, StatusEffect statusEffect)
        {
            string[] selectedCardIdentifiers = statusEffect.Identifier.Split(',');
            IEnumerator coroutine;
            foreach (string identifier in selectedCardIdentifiers)
            {
                Card card = FindCardsWhere(c => c.QualifiedIdentifier == identifier && c.IsInPlayAndHasGameText).FirstOrDefault();
                if (card is null)
                {
                    continue;
                }
                CardController cc = FindCardController(card);
                IEnumerable<PhaseChangeTrigger> triggers = FindTriggersWhere(trigger => trigger.CardSource.CardController == cc && trigger is PhaseChangeTrigger pct && pct.PhaseCriteria(Phase.Start)).Select(trigger => trigger as PhaseChangeTrigger);
                foreach (PhaseChangeTrigger trigger in triggers)
                {
                    coroutine = trigger.Response(pca);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedCardsKey, (IEnumerable<string>)null);
            yield return DoNothing();
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        { 
            switch (index)
            {
                case 0:
                    //Destroy an environment card.
                    IEnumerator coroutine = GameController.SelectAndDestroyCard(HeroTurnTakerController, new LinqCardCriteria(c => c.IsEnvironment, "environment"), optional: false, cardSource: GetCardSource());
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
                    //The target with the lowest HP deals 1 target 1 irreducible fire damage.
                    List<Card> storedResults = new List<Card>();
                    IEnumerator coroutine2 = GameController.FindTargetsWithLowestHitPoints(1, 1, c => true, storedResults, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine2);
                    }
                    coroutine2 = GameController.SelectTargetsAndDealDamage(HeroTurnTakerController, new DamageSource(GameController, storedResults.First()), 1, DamageType.Fire, 1, false, 1, isIrreducible: true, cardSource: GetCardSource());
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
                    //Select a deck and put its top card into play.
                    List<SelectLocationDecision> storedResults2 = new List<SelectLocationDecision>();
                    IEnumerator coroutine3 = GameController.SelectADeck(DecisionMaker, SelectionType.PlayTopCard, (Location l) => true, storedResults2, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine3);
                    }
                    if (!DidSelectDeck(storedResults2))
                    {
                        yield break;
                    }
                    Location location = GetSelectedLocation(storedResults2);
                    if (location != null)
                    {
                        coroutine3 = GameController.PlayTopCardOfLocation(TurnTakerController, location, isPutIntoPlay: true, cardSource: GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(coroutine3);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(coroutine3);
                        }
                    }
                    break;
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {

            return new CustomDecisionText("Select a component that's start of turn effects should act at the end of this turn.", "Select a component that's start of turn effects should act at the end of this turn.", "Vote for a component that's start of turn effects should act at the end of this turn.", "component that's start of turn effects should act at the end of this turn");

        }
    }
}
