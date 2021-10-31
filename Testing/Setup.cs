using Handelabra.Sentinels.Engine.Model;
using NUnit.Framework;
using CauldronPromos.MrFixer;

namespace CauldronPromosTests
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            ModHelper.AddAssembly("CauldronPromos", typeof(EnlightenedMrFixerCharacterCardController).Assembly);
        }
    }
}
