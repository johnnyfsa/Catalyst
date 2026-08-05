using Catalyst.Game.Launch;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Game.Launch
{
    public sealed class SessionSeedGeneratorTests
    {
        [Test]
        public void GenerateDifferentFrom_DoesNotReturnPreviousSeed()
        {
            const int previousSeed = 12345;

            int generatedSeed =
                SessionSeedGenerator
                    .GenerateDifferentFrom(
                        previousSeed
                    );

            Assert.That(
                generatedSeed,
                Is.Not.EqualTo(previousSeed)
            );
        }

        [Test]
        public void GenerateDifferentFrom_AcceptsMaximumInteger()
        {
            int generatedSeed =
                SessionSeedGenerator
                    .GenerateDifferentFrom(
                        int.MaxValue
                    );

            Assert.That(
                generatedSeed,
                Is.Not.EqualTo(int.MaxValue)
            );
        }
    }
}