using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Zones
{
    public sealed class CardObjectiveZoneRuntimeTests
    {
        private CardDefinition acceptedDefinition;
        private CardDefinition otherDefinition;

        [SetUp]
        public void SetUp()
        {
            acceptedDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            otherDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (acceptedDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    acceptedDefinition
                );
            }

            if (otherDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    otherDefinition
                );
            }
        }

        [Test]
        public void Constructor_WithNullAcceptedDefinition_Throws()
        {
            Assert.That(
                () => new CardObjectiveZoneRuntime(
                    acceptedDefinition: null,
                    requiredAmount: 10
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidRequiredAmount_Throws(
            int requiredAmount
        )
        {
            Assert.That(
                () => new CardObjectiveZoneRuntime(
                    acceptedDefinition,
                    requiredAmount
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void NewZone_ExposesConfiguredObjective()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone(requiredAmount: 10);

            Assert.That(
                zone.AcceptedDefinition,
                Is.SameAs(acceptedDefinition)
            );

            Assert.That(
                zone.RequiredAmount,
                Is.EqualTo(10)
            );

            Assert.That(zone.CurrentAmount, Is.Zero);
            Assert.That(zone.IsCompleted, Is.False);
            Assert.That(zone.IsEmpty, Is.True);
        }

        [Test]
        public void CanAdd_WithAcceptedDefinition_ReturnsTrue()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                zone.CanAdd(card),
                Is.True
            );
        }

        [Test]
        public void CanAdd_WithDifferentDefinition_ReturnsFalse()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone();

            CardInstance card =
                CreateCard(otherDefinition);

            Assert.That(
                zone.CanAdd(card),
                Is.False
            );
        }

        [Test]
        public void CanAdd_WithDifferentDefinitionInstance_ReturnsFalse()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone();

            CardDefinition equivalentDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            try
            {
                CardInstance card =
                    CreateCard(equivalentDefinition);

                Assert.That(
                    zone.CanAdd(card),
                    Is.False
                );
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(
                    equivalentDefinition
                );
            }
        }

        [Test]
        public void TryAdd_WithAcceptedDefinition_AddsCard()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone(requiredAmount: 3);

            CardInstance card =
                CreateCard(acceptedDefinition);

            bool added = zone.TryAdd(card);

            Assert.That(added, Is.True);
            Assert.That(zone.Count, Is.EqualTo(1));
            Assert.That(zone.CurrentAmount, Is.EqualTo(1));
            Assert.That(zone.Cards, Does.Contain(card));
        }

        [Test]
        public void TryAdd_WithDifferentDefinition_DoesNotMutateZone()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone();

            CardInstance card =
                CreateCard(otherDefinition);

            bool added = zone.TryAdd(card);

            Assert.That(added, Is.False);
            Assert.That(zone.IsEmpty, Is.True);
            Assert.That(zone.CurrentAmount, Is.Zero);
        }

        [Test]
        public void AddingRequiredAmount_CompletesObjective()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone(requiredAmount: 2);

            Assert.That(
                zone.TryAdd(CreateCard(acceptedDefinition)),
                Is.True
            );

            Assert.That(zone.IsCompleted, Is.False);

            Assert.That(
                zone.TryAdd(CreateCard(acceptedDefinition)),
                Is.True
            );

            Assert.That(zone.CurrentAmount, Is.EqualTo(2));
            Assert.That(zone.IsCompleted, Is.True);
        }

        [Test]
        public void AddingMoreThanRequiredAmount_RemainsCompleted()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone(requiredAmount: 2);

            zone.TryAdd(CreateCard(acceptedDefinition));
            zone.TryAdd(CreateCard(acceptedDefinition));
            zone.TryAdd(CreateCard(acceptedDefinition));

            Assert.That(zone.CurrentAmount, Is.EqualTo(3));
            Assert.That(zone.IsCompleted, Is.True);
        }

        [Test]
        public void StoredCard_CannotBeRemoved()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(zone.TryAdd(card), Is.True);
            Assert.That(zone.CanRemove(card), Is.False);
            Assert.That(zone.TryRemove(card), Is.False);

            Assert.That(zone.Count, Is.EqualTo(1));
            Assert.That(zone.Contains(card), Is.True);
        }

        [Test]
        public void SameCard_CannotBeAddedTwice()
        {
            CardObjectiveZoneRuntime zone =
                CreateZone();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(zone.TryAdd(card), Is.True);
            Assert.That(zone.TryAdd(card), Is.False);
            Assert.That(zone.Count, Is.EqualTo(1));
        }

        private CardObjectiveZoneRuntime CreateZone(
            int requiredAmount = 10
        )
        {
            return new CardObjectiveZoneRuntime(
                acceptedDefinition,
                requiredAmount
            );
        }

        private static CardInstance CreateCard(
            CardDefinition definition
        )
        {
            return new CardInstance(
                Guid.NewGuid(),
                definition
            );
        }
    }
}