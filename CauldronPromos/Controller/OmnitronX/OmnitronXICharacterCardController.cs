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
            SpecialStringMaker.ShowSpecialString(() => $"{string.Join(" and ", Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey).ToArray())} {Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey).Count().ToString_IsOrAre()} overclocked and will act at the end of {Game.Journal.GetCardPropertiesTurnTaker(CharacterCardWithoutReplacements, OverclockedTurnTakerKey).Name}'s turn.", showInEffectsList: () => true, relatedCards: () => OverclockedCards).Condition = SpecialStringCriteria;
        }

        private bool SpecialStringCriteria()
        {
            return  Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey) != null &&
                    Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey).Count() > 0 && 
                    Game.Journal.GetCardPropertiesTurnTaker(CharacterCardWithoutReplacements, OverclockedTurnTakerKey) != null;
        }

        private List<ITrigger> _triggersChanged = new List<ITrigger>();
        private Func<PhaseChangeAction, bool> _alternative;

        public static readonly string OverclockedCardsKey = "OverclockedCards";
        public static readonly string OverclockedTurnTakerKey = "OverclockedTurnTaker";

        private IEnumerable<Card> OverclockedCards
        {
            get
            {
                if(Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey) is null)
                {
                    return new List<Card>();
                }

                //this doesn't work perfectly if multiple cards have been overclocked and multiple of those have the same title, but thats a niche case i am not too worried about
                return FindCardsWhere(c => c.IsInPlayAndHasGameText && Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey).Any(s => s == c.Title));
            }
        }

        public override void AddStartOfGameTriggers()
        {
            AddOverclockTriggers();
        }

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

            OverclockCards(selectedCards);

            coroutine = GameController.SendMessageAction($"{string.Join(" and ", selectedCards.Select(c => c.Title).ToArray())} {selectedCards.Count().ToString_IsOrAre()} overclocked and will act at the end of {Game.ActiveTurnTaker.Name}'s turn.", Priority.Medium, GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }

        private void OverclockCards(IEnumerable<Card> selectedCards)
        {
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedCardsKey, selectedCards.Select(c => c.Title));
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedTurnTakerKey, Game.ActiveTurnTaker);
            AddOverclockTriggers();
        }

        private void AddOverclockTriggers()
        {
            CardController cardController = FindCardController(CharacterCardWithoutReplacements);
            Trigger<PhaseChangeAction> trigger = new Trigger<PhaseChangeAction>(GameController, (PhaseChangeAction p) => p.ToPhase.IsEnd && Game.Journal.GetCardPropertiesTurnTaker(CharacterCardWithoutReplacements, OverclockedTurnTakerKey) != null && p.ToPhase.TurnTaker == Game.Journal.GetCardPropertiesTurnTaker(CharacterCardWithoutReplacements, OverclockedTurnTakerKey), p => ApplyChangesResponse(),new TriggerType[] { TriggerType.HiddenLast }, TriggerTiming.Before, GetCardSource());
            cardController.AddTrigger(trigger);
        }

        private IEnumerator ApplyChangesResponse()
        {
            IEnumerable<Card> selectedCards = FindCardsWhere(c => Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedCardsKey).Contains(c.Title) && c.IsInPlayAndHasGameText);
            TurnTaker turnTakerToUse = Game.Journal.GetCardPropertiesTurnTaker(CharacterCardWithoutReplacements, OverclockedTurnTakerKey);
            IEnumerable<TurnPhase> turnPhases = selectedCards.Select(c => c.Owner.TurnPhases.Where((TurnPhase phase) => phase.IsStart).First()).Distinct();
            IEnumerable<PhaseChangeAction> fakeStarts = turnPhases.Select(tp => new PhaseChangeAction(GetCardSource(), null, tp, tp.IsEphemeral));
            FindTriggersWhere((ITrigger t) => !_triggersChanged.Contains(t) && selectedCards.Contains(t.CardSource.Card) && t is PhaseChangeTrigger && fakeStarts.Any(fs => t.DoesMatchTypeAndCriteria(fs))).ForEach(delegate (ITrigger t)
            {
                ApplyChangeTo((PhaseChangeTrigger)t, turnTakerToUse);
            });

            AddEndOfTurnTrigger(tt => tt == turnTakerToUse, RestoreTriggers, TriggerType.HiddenLast);
            foreach (CardController cc in selectedCards.Select(c => FindCardController(c)))
            {
                cc.AddBeforeLeavesPlayActions(RestoreTriggers);
            }
            yield return DoNothing();
        }

        private void ApplyChangeTo(PhaseChangeTrigger trigger, TurnTaker turnTakerToUse)
        {
            _alternative = (PhaseChangeAction p) => p.ToPhase.TurnTaker == turnTakerToUse && p.ToPhase.IsEnd;
            trigger.AddAlternativeCriteria(_alternative);


            _triggersChanged.Add(trigger);
        }

        private IEnumerator RestoreTriggers(GameAction action)
        {
            TurnTaker turnTakerToUse = Game.Journal.GetCardPropertiesTurnTaker(CharacterCardWithoutReplacements, OverclockedTurnTakerKey);
            if(turnTakerToUse is null)
            {
                yield break;
            }
            foreach (PhaseChangeTrigger trigger in _triggersChanged)
            {
                trigger.RemoveAlternativeCriteria(_alternative);
                if (GameController.ActiveTurnPhase.TurnTaker != turnTakerToUse || !GameController.ActiveTurnPhase.IsEnd)
                {
                    continue;
                }
                TurnPhase turnPhase = turnTakerToUse.TurnPhases.Where((TurnPhase phase) => phase.IsEnd).First();
                PhaseChangeAction action2 = new PhaseChangeAction(GetCardSource(), null, turnPhase, turnPhase.IsEphemeral);
                if (trigger.CardSource.CardController.CardWithoutReplacements.IsInPlayAndHasGameText && !trigger.DoesMatchTypeAndCriteria(action2))
                {
                    AddTemporaryTriggerInhibitor((ITrigger t) => t == trigger, (PhaseChangeAction p) => true);
                }
            }

            //theoretically if multiple cards got overclocked and then one left play before the end phase, this could omit one of the possible cards
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedCardsKey, (IEnumerable<string>) null);
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedTurnTakerKey, (TurnTaker) null);

            yield return null;
        }

        private void AddTemporaryTriggerInhibitor<T>(Func<ITrigger, bool> triggerCriteria, Func<T, bool> expiryCriteria) where T : GameAction
        {
            GameController.AddTemporaryTriggerInhibitor(triggerCriteria, expiryCriteria, GetCardSource());
        }

        protected TurnTaker GetOriginalOwner(Card c)
        {
            return (FindTurnTakersWhere((TurnTaker tt) => tt.Identifier == c.ParentDeck.Identifier)).FirstOrDefault();
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
