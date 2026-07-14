using System;
using Catalyst.Cards.Runtime.Session;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionConfigTests
    {
        [Test]
        public void Constructor_StoresConfiguration()
        {
            GameSessionConfig config =
                new GameSessionConfig(
                    initialHandSize: 8,
                    maxHandSize: 8
                );

            Assert.That(
                config.InitialHandSize,
                Is.EqualTo(8)
            );

            Assert.That(
                config.MaxHandSize,
                Is.EqualTo(8)
            );
        }

        [Test]
        public void Constructor_AllowsInitialSizeAboveCapacity()
        {
            GameSessionConfig config =
                new GameSessionConfig(
                    initialHandSize: 12,
                    maxHandSize: 8
                );

            Assert.That(
                config.InitialHandSize,
                Is.EqualTo(12)
            );

            Assert.That(
                config.MaxHandSize,
                Is.EqualTo(8)
            );
        }

        [TestCase(0, 8)]
        [TestCase(-1, 8)]
        [TestCase(8, 0)]
        [TestCase(8, -1)]
        public void Constructor_WithInvalidValues_Throws(
            int initialHandSize,
            int maxHandSize
        )
        {
            Assert.That(
                () => new GameSessionConfig(
                    initialHandSize,
                    maxHandSize
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
    }
}