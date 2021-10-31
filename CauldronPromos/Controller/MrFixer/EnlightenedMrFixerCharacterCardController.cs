using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace CauldronPromos.MrFixer
{
    public class EnlightenedMrFixerCharacterCardController : HeroCharacterCardController
    {
        public EnlightenedMrFixerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                   
                    
                    break;
                case 1:
                    
                    break;
                case 2:
                   
                    break;
            }

            throw new NotImplementedException();
        }

        public override IEnumerator UsePower(int index = 0)
        {
            throw new NotImplementedException();
        }
    }
}
