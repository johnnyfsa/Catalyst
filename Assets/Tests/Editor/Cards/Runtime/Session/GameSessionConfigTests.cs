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

        [Test]
        public void Constructor_WithoutResources_UsesZeroAmounts()
        {
            var config =
                new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 8
                );

            Assert.That(
                config.InitialHeat,
                Is.EqualTo(0)
            );

            Assert.That(
                config.InitialElectricity,
                Is.EqualTo(0)
            );
        }

        [Test]
        public void Constructor_WithResourceAmounts_PreservesValues()
        {
            var config =
                new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 8,
                    initialHeat: 3,
                    initialElectricity: 4
                );

            Assert.That(
                config.InitialHeat,
                Is.EqualTo(3)
            );

            Assert.That(
                config.InitialElectricity,
                Is.EqualTo(4)
            );
        }

        [TestCase(-1)]
        [TestCase(-5)]
        public void Constructor_WithNegativeInitialHeat_Throws(
            int initialHeat
        )
        {
            Assert.That(
                () => new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 8,
                    initialHeat: initialHeat
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [TestCase(-1)]
        [TestCase(-5)]
        public void Constructor_WithNegativeInitialElectricity_Throws(
            int initialElectricity
        )
        {
            Assert.That(
                () => new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 8,
                    initialElectricity: initialElectricity
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
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