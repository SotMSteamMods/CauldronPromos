using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;

namespace CauldronPromos.OmnitronX
{
    // this is an alternative implementation of OmniXI that uses trigger manipulation rather than a status effect
    // it overall works, but in game, OOT uses of the power doesn't work on normal, but does work on rewind

    public class OmnitronXIVCharacterCardController : HeroCharacterCardController
    {
        public OmnitronXIVCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => $"{string.Join(" and ", OverclockedCardsInPlay.Select(c => c.Title).ToArray())} {OverclockedCardsInPlay.Count().ToString_IsOrAre()} overclocked and will act at the end of this turn.").Condition = SpecialStringCriteria;
        }

        public override void AddLastTriggers()
        {
            AddOverclockTriggers();
        }

        private bool SpecialStringCriteria()
        {
            return OverclockedCardsInPlay.Any();
        }

        private List<ITrigger> _triggersChanged = new List<ITrigger>();
        private Func<PhaseChangeAction, bool> _alternative;

        public static readonly string OverclockedCardsKeyPrefix = "OverclockedCards";
        public static readonly string OverclockedTurnTakerDictKey = "OverclockedTurnTakersDict";

        private bool IsOverclocked(Card c)
        {

            IEnumerable<string> overclockedTurnTakers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey);
            if(overclockedTurnTakers is null || overclockedTurnTakers.Count() == 0)
            {
                return false;
            }

            string constructedOverclockKey;
            IEnumerable<string> overclockedCards;
            foreach (string turnTakerPrefix in overclockedTurnTakers)
            {
                constructedOverclockKey = OverclockedCardsKeyPrefix + turnTakerPrefix;
                overclockedCards = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, constructedOverclockKey);
                if(overclockedCards.Any(overclockedQualifiedIdentifier => c.QualifiedIdentifier == overclockedQualifiedIdentifier))
                {
                    return true;
                }
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

            OverclockCards(selectedCards);

            AddLastTriggers();

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
            IEnumerable<string> overclockedTurnTakers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey);
            List<string> updatedListOfTurnTakers = new List<string>();
            if(overclockedTurnTakers != null)
            {
                updatedListOfTurnTakers = overclockedTurnTakers.ToList();
            }

            if (!updatedListOfTurnTakers.Contains(Game.ActiveTurnTaker.QualifiedIdentifier))
            {
                updatedListOfTurnTakers.Add(Game.ActiveTurnTaker.QualifiedIdentifier);
            }
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey, updatedListOfTurnTakers);

            string constructedOverclockKey = OverclockedCardsKeyPrefix + Game.ActiveTurnTaker.QualifiedIdentifier;

            IEnumerable<string> overclockedCards = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, constructedOverclockKey);
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
                    Log.Debug($"Recorded {c.QualifiedIdentifier} under key {constructedOverclockKey}");
                }
            }
            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, constructedOverclockKey, updatedListOfCards);
        }

        private void AddOverclockTriggers()
        {
            CardController cardController = FindCardController(CharacterCardWithoutReplacements);
            IEnumerable<string> overclockedTurnTakers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey);
            if (overclockedTurnTakers is null)
            {
                return;
            }
            foreach (string turnTakerQualifiedIdentifier in overclockedTurnTakers)
            {
                TurnTaker turnTakerToUse = Game.TurnTakers.FirstOrDefault(tt => tt.QualifiedIdentifier == turnTakerQualifiedIdentifier);//FindTurnTakersWhere(tt => tt.QualifiedIdentifier == turnTakerQualifiedIdentifier).FirstOrDefault();
                if (turnTakerToUse is null)
                {
                    Log.Warning($"Stored Qualified TurnTaker of ***{turnTakerQualifiedIdentifier}*** could not be found in the game!");
                    continue;
                }
                Trigger<PhaseChangeAction> trigger = new Trigger<PhaseChangeAction>(GameController, (PhaseChangeAction p) => p.ToPhase.IsEnd &&  p.ToPhase.TurnTaker == turnTakerToUse, p => ApplyChangesResponse(), new TriggerType[] { TriggerType.HiddenLast }, TriggerTiming.Before, GetCardSource());

                cardController.AddTrigger(trigger);
            }
        }

        private IEnumerator ApplyChangesResponse()
        {

            IEnumerable<string> overclockedTurnTakers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey);
            if (overclockedTurnTakers is null)
            {
                yield return DoNothing();
            }
            foreach (string turnTakerQualifiedIdentifier in overclockedTurnTakers)
            {
                TurnTaker turnTakerToUse = FindTurnTakersWhere(tt => tt.QualifiedIdentifier == turnTakerQualifiedIdentifier).FirstOrDefault();
                if (turnTakerToUse is null)
                {
                    Log.Warning($"Stored Qualified TurnTaker of {turnTakerQualifiedIdentifier} could not be found in the game!");
                    continue;
                }
                string constructedOverclockKey = OverclockedCardsKeyPrefix + turnTakerQualifiedIdentifier;
                IEnumerable<string> overclockedCardsQualifiedIdentifiers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, constructedOverclockKey);
                if(overclockedCardsQualifiedIdentifiers is null)
                {
                    continue;
                }
                IEnumerable<Card> selectedCards = FindCardsWhere(c => overclockedCardsQualifiedIdentifiers.Contains(c.QualifiedIdentifier) && c.IsInPlayAndHasGameText);
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
            IEnumerable<string> overclockedTurnTakers = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey);
            List<string> overclockedTurnTakersToModify = Game.Journal.GetCardPropertiesStringList(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey).ToList();

            if (overclockedTurnTakers is null)
            {
                yield return DoNothing();
            }
            foreach(string turnTakerQualifiedIdentifier in overclockedTurnTakers)
            {
                TurnTaker turnTakerToUse = FindTurnTakersWhere(tt => tt.QualifiedIdentifier == turnTakerQualifiedIdentifier).FirstOrDefault();
                if(turnTakerToUse is null)
                {
                    Log.Warning($"Stored Qualified TurnTaker of {turnTakerQualifiedIdentifier} could not be found in the game!");
                    continue;
                }

                if(GameController.ActiveTurnPhase.TurnTaker != turnTakerToUse || !GameController.ActiveTurnPhase.IsEnd)
                {
                    continue;
                }


                foreach (PhaseChangeTrigger trigger in _triggersChanged)
                {
                    // we first check to see if this trigger is one associated with this TurnTaker
                    // we then validate that the Game State matches the end conditions
                    PhaseChangeAction actionToVerify = new PhaseChangeAction(GetCardSource(), null, turnTakerToUse.TurnPhases.First(tp => tp.IsEnd), true);
                    if (!trigger.AdditionalCriteria(actionToVerify))
                    {
                        continue;
                    }
                    trigger.RemoveAlternativeCriteria(_alternative);
                    TurnPhase turnPhase = turnTakerToUse.TurnPhases.Where((TurnPhase phase) => phase.IsEnd).First();
                    PhaseChangeAction action2 = new PhaseChangeAction(GetCardSource(), null, turnPhase, turnPhase.IsEphemeral);
                    if (trigger.CardSource.CardController.CardWithoutReplacements.IsInPlayAndHasGameText && !trigger.DoesMatchTypeAndCriteria(action2))
                    {
                        AddTemporaryTriggerInhibitor((ITrigger t) => t == trigger, (PhaseChangeAction p) => true);
                    }
                    string constructedOverclockKey = OverclockedCardsKeyPrefix + turnTakerQualifiedIdentifier;
                    Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, constructedOverclockKey, (IEnumerable<string>)null);
                    overclockedTurnTakersToModify.Remove(turnTakerQualifiedIdentifier);
                }

            }

            Game.Journal.RecordCardProperties(CharacterCardWithoutReplacements, OverclockedTurnTakerDictKey, overclockedTurnTakersToModify);

            yield return DoNothing();
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
