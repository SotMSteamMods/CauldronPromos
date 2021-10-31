using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CauldronPromos.AbsoluteZero
{
    public class NitroBoostAbsoluteZeroCharacterCardController : HeroCharacterCardController
    {
        public NitroBoostAbsoluteZeroCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int fireDamage = GetPowerNumeral(0, 1);
            int coldDamage = GetPowerNumeral(1, 1);

            //Play a card.
            IEnumerator coroutine = SelectAndPlayCardFromHand(HeroTurnTakerController, optional: false);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //Absolute Zero deals himself 1 fire and 1 cold damage.
            coroutine = DealDamage(CharacterCard, CharacterCard, fireDamage, DamageType.Fire, cardSource: GetCardSource());
            IEnumerator coroutine2 = DealDamage(CharacterCard, CharacterCard, coldDamage, DamageType.Cold, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
                yield return GameController.StartCoroutine(coroutine2);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
                GameController.ExhaustCoroutine(coroutine2);
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
                    //Select a hero. Increase the next damage dealt by that hero by 2.
                    IEnumerator coroutine2 = GameController.SelectHeroAndIncreaseNextDamageDealt(HeroTurnTakerController, 2, cardSource: GetCardSource());
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
                    //Destroy a target with 1 HP.
                    IEnumerator coroutine3 = GameController.SelectAndDestroyCard(HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1, "targets with 1 HP", useCardsSuffix: false), optional: false, cardSource: GetCardSource());
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
