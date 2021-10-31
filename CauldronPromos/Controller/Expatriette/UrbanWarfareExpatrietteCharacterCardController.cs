using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CauldronPromos.Expatriette
{
	public class UrbanWarfareExpatrietteCharacterCardController : HeroCharacterCardController
	{
		public UrbanWarfareExpatrietteCharacterCardController(Card card, TurnTakerController turnTakerController)
			: base(card, turnTakerController)
		{
		}

		public override IEnumerator UsePower(int index = 0)
		{
			//Draw a card
			IEnumerator coroutine = DrawCard();
			if (UseUnityCoroutines)
			{
				yield return GameController.StartCoroutine(coroutine);
			}
			else
			{
				GameController.ExhaustCoroutine(coroutine);
			}

			//You may use a power now.
			coroutine = GameController.SelectAndUsePower(HeroTurnTakerController, cardSource: GetCardSource());
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
					{
						//One hero may use a power now.
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
					}
				case 1:
					{
						//Destroy a target with 1 HP.
						IEnumerator coroutine2 = GameController.SelectAndDestroyCard(HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1, "targets with 1 HP", useCardsSuffix: false), optional: false, cardSource: GetCardSource());
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(coroutine2);
						}
						else
						{
							GameController.ExhaustCoroutine(coroutine2);
						}
						break;
					}
				case 2:
					{
						//Select a hero. The next time that hero uses a power, they also deal 1 projectile damage to 1 target.
						List<SelectTurnTakerDecision> storedResults = new List<SelectTurnTakerDecision>();
						IEnumerator coroutine3 = GameController.SelectHeroTurnTaker(HeroTurnTakerController, SelectionType.DealDamageAfterUsePower, optional: false, allowAutoDecide: false, storedResults, cardSource: GetCardSource());
						if (UseUnityCoroutines)
						{
							yield return GameController.StartCoroutine(coroutine3);
						}
						else
						{
							GameController.ExhaustCoroutine(coroutine3);
						}
						if(!DidSelectTurnTaker(storedResults))
                        {
							yield break;
                        }
						
						TurnTaker selectedTurnTaker = GetSelectedTurnTaker(storedResults);
						if (!selectedTurnTaker.IsHero)
						{
							yield break;
						}
						HeroTurnTaker heroTurnTaker = selectedTurnTaker.ToHero();
						Card damageSource = ((!heroTurnTaker.HasMultipleCharacterCards) ? heroTurnTaker.CharacterCard : null);
						DealDamageAfterUsePowerStatusEffect dealDamageAfterUsePowerStatusEffect = new DealDamageAfterUsePowerStatusEffect(heroTurnTaker, damageSource, null, 1, DamageType.Projectile, 1, isIrreducible: false);
						dealDamageAfterUsePowerStatusEffect.TurnTakerCriteria.IsSpecificTurnTaker = heroTurnTaker;
						if (!heroTurnTaker.HasMultipleCharacterCards)
						{
							dealDamageAfterUsePowerStatusEffect.CardDestroyedExpiryCriteria.Card = heroTurnTaker.CharacterCard;
						}
						dealDamageAfterUsePowerStatusEffect.NumberOfUses = 1;
						coroutine3 = AddStatusEffect(dealDamageAfterUsePowerStatusEffect);
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
}
