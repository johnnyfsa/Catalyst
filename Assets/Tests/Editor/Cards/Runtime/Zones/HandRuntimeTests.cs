using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Zones
{
    public sealed class HandRuntimeTests
    {
        private CardDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (definition != null)
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void DefaultConstructor_UsesEightCardCapacity()
        {
            HandRuntime hand = new HandRuntime();

            Assert.That(
                hand.Capacity,
                Is.EqualTo(HandRuntime.DefaultCapacity)
            );

            Assert.That(hand.Capacity, Is.EqualTo(8));
        }

        [Test]
        public void Constructor_WithCustomCapacity_StoresCapacity()
        {
            HandRuntime hand = new HandRuntime(3);

            Assert.That(hand.Capacity, Is.EqualTo(3));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidCapacity_Throws(
            int invalidCapacity
        )
        {
            Assert.That(
                () => new HandRuntime(invalidCapacity),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void TryAdd_BelowCapacity_AddsCard()
        {
            HandRuntime hand = new HandRuntime(2);
            CardInstance card = CreateCard();

            bool result = hand.TryAdd(card);

            Assert.That(result, Is.True);
            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.IsFull, Is.False);
        }

        [Test]
        public void TryAdd_WhenCapacityIsReached_RejectsCard()
        {
            HandRuntime hand = new HandRuntime(2);

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();
            CardInstance rejected = CreateCard();

            hand.TryAdd(first);
            hand.TryAdd(second);

            bool result = hand.TryAdd(rejected);

            Assert.That(result, Is.False);
            Assert.That(hand.Count, Is.EqualTo(2));
            Assert.That(hand.IsFull, Is.True);
            Assert.That(hand.Contains(rejected), Is.False);
        }

        [Test]
        public void RemovingCard_FromFullHand_MakesSpaceAvailable()
        {
            HandRuntime hand = new HandRuntime(1);

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();

            hand.TryAdd(first);

            Assert.That(hand.IsFull, Is.True);

            hand.TryRemove(first);

            Assert.That(hand.IsFull, Is.False);
            Assert.That(hand.TryAdd(second), Is.True);
        }

        private CardInstance CreateCard()
        {
            return new CardInstance(
                Guid.NewGuid(),
                definition
            );
        }
    }
}