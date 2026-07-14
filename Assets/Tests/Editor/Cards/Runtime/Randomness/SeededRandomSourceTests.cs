using System;
using Catalyst.Cards.Runtime.Randomness;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Randomness
{
    public sealed class SeededRandomSourceTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            SeededRandomSource first =
                new SeededRandomSource(12345);

            SeededRandomSource second =
                new SeededRandomSource(12345);

            for (int index = 0; index < 20; index++)
            {
                Assert.That(
                    first.Next(0, 1000),
                    Is.EqualTo(second.Next(0, 1000))
                );
            }
        }

        [Test]
        public void Next_ReturnsValueWithinRequestedRange()
        {
            SeededRandomSource randomSource =
                new SeededRandomSource(12345);

            for (int index = 0; index < 100; index++)
            {
                int result = randomSource.Next(3, 8);

                Assert.That(
                    result,
                    Is.GreaterThanOrEqualTo(3)
                );

                Assert.That(
                    result,
                    Is.LessThan(8)
                );
            }
        }

        [Test]
        public void Next_WithInvalidRange_Throws()
        {
            SeededRandomSource randomSource =
                new SeededRandomSource(12345);

            Assert.That(
                () => randomSource.Next(5, 5),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
    }
}