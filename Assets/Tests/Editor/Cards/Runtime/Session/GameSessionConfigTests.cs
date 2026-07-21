using System;
using Catalyst.Cards.Runtime.Session;
using NUnit.Framework;
using Catalyst.Cards.Definitions;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionConfigTests
    {
        private CardDefinition water;

        [SetUp]
        public void SetUp()
        {
            water =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [Test]
        public void Constructor_WithoutDeliveryZones_UsesEmptyCollection()
        {
            var config =
                new GameSessionConfig(
                    initialHandSize: 5,
                    maxHandSize: 8
                );

            Assert.That(
                config.DeliveryZones,
                Is.Not.Null
            );

            Assert.That(
                config.DeliveryZones,
                Is.Empty
            );
        }

        [Test]
        public void Constructor_StoresDeliveryZoneConfigurations()
        {
            var deliveryZoneConfig =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            var config =
                new GameSessionConfig(
                    initialHandSize: 5,
                    maxHandSize: 8,
                    deliveryZones: new[]
                    {
                deliveryZoneConfig
                    }
                );

            Assert.That(
                config.DeliveryZones.Count,
                Is.EqualTo(1)
            );

            Assert.That(
                config.DeliveryZones[0],
                Is.SameAs(deliveryZoneConfig)
            );
        }

        [Test]
        public void Constructor_StoresMultipleDeliveryZoneConfigurations()
        {
            CardDefinition oxygen =
                ScriptableObject.CreateInstance<CardDefinition>();

            try
            {
                var waterZone =
                    new CardDeliveryZoneConfig(
                        water,
                        requiredAmount: 10
                    );

                var oxygenZone =
                    new CardDeliveryZoneConfig(
                        oxygen,
                        requiredAmount: 2
                    );

                var config =
                    new GameSessionConfig(
                        initialHandSize: 5,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    waterZone,
                    oxygenZone
                        }
                    );

                Assert.That(
                    config.DeliveryZones.Count,
                    Is.EqualTo(2)
                );

                Assert.That(
                    config.DeliveryZones[0],
                    Is.SameAs(waterZone)
                );

                Assert.That(
                    config.DeliveryZones[1],
                    Is.SameAs(oxygenZone)
                );
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(
                    oxygen
                );
                UnityEngine.Object.DestroyImmediate(
                    water
                );
            }
        }

        [Test]
        public void Constructor_WithNullDeliveryZoneEntry_Throws()
        {
            Assert.That(
                () => new GameSessionConfig(
                    initialHandSize: 5,
                    maxHandSize: 8,
                    deliveryZones:
                        new CardDeliveryZoneConfig[]
                        {
                    null
                        }
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void DeliveryZones_CannotBeModifiedExternally()
        {
            var deliveryZoneConfig =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            var config =
                new GameSessionConfig(
                    initialHandSize: 5,
                    maxHandSize: 8,
                    deliveryZones: new[]
                    {
                deliveryZoneConfig
                    }
                );

            var exposedCollection =
                config.DeliveryZones
                    as System.Collections.Generic
                        .IList<CardDeliveryZoneConfig>;

            Assert.That(
                exposedCollection,
                Is.Not.Null
            );

            Assert.That(
                exposedCollection.IsReadOnly,
                Is.True
            );

            Assert.That(
                () => exposedCollection.Add(
                    new CardDeliveryZoneConfig(
                        water,
                        requiredAmount: 2
                    )
                ),
                Throws.TypeOf<NotSupportedException>()
            );

            Assert.That(
                config.DeliveryZones.Count,
                Is.EqualTo(1)
            );
        }

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
        [TearDown]
        public void TearDown()
        {
            if (water != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    water
                );
            }
        }
    }
}