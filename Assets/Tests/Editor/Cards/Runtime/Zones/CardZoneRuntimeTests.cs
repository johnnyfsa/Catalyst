using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Zones
{
    public sealed class CardZoneRuntimeTests
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
        public void NewZone_StartsEmpty()
        {
            HandRuntime zone = new HandRuntime();

            Assert.That(zone.Count, Is.Zero);
            Assert.That(zone.IsEmpty, Is.True);
            Assert.That(zone.Cards, Is.Empty);
        }

        [Test]
        public void TryAdd_WithNewCard_AddsCard()
        {
            HandRuntime zone = new HandRuntime();
            CardInstance card = CreateCard();

            bool result = zone.TryAdd(card);

            Assert.That(result, Is.True);
            Assert.That(zone.Count, Is.EqualTo(1));
            Assert.That(zone.Contains(card), Is.True);
            Assert.That(zone.Cards[0], Is.SameAs(card));
        }

        [Test]
        public void TryAdd_WithSameCardTwice_DoesNotDuplicateCard()
        {
            HandRuntime zone = new HandRuntime();
            CardInstance card = CreateCard();

            bool firstResult = zone.TryAdd(card);
            bool secondResult = zone.TryAdd(card);

            Assert.That(firstResult, Is.True);
            Assert.That(secondResult, Is.False);
            Assert.That(zone.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryAdd_WithDifferentObjectUsingSameId_DoesNotDuplicateIdentity()
        {
            HandRuntime zone = new HandRuntime();
            Guid sharedId = Guid.NewGuid();

            CardInstance firstCard =
                new CardInstance(sharedId, definition);

            CardInstance duplicateIdentity =
                new CardInstance(sharedId, definition);

            bool firstResult = zone.TryAdd(firstCard);
            bool secondResult = zone.TryAdd(duplicateIdentity);

            Assert.That(firstResult, Is.True);
            Assert.That(secondResult, Is.False);
            Assert.That(zone.Count, Is.EqualTo(1));
            Assert.That(zone.Cards[0], Is.SameAs(firstCard));
        }

        [Test]
        public void TryRemove_WithContainedCard_RemovesCard()
        {
            HandRuntime zone = new HandRuntime();
            CardInstance card = CreateCard();

            zone.TryAdd(card);

            bool result = zone.TryRemove(card);

            Assert.That(result, Is.True);
            Assert.That(zone.IsEmpty, Is.True);
            Assert.That(zone.Contains(card), Is.False);
        }

        [Test]
        public void TryRemove_WithAbsentCard_DoesNotChangeZone()
        {
            HandRuntime zone = new HandRuntime();
            CardInstance containedCard = CreateCard();
            CardInstance absentCard = CreateCard();

            zone.TryAdd(containedCard);

            bool result = zone.TryRemove(absentCard);

            Assert.That(result, Is.False);
            Assert.That(zone.Count, Is.EqualTo(1));
            Assert.That(zone.Cards[0], Is.SameAs(containedCard));
        }

        [Test]
        public void Cards_PreserveInsertionOrder()
        {
            HandRuntime zone = new HandRuntime();

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();
            CardInstance third = CreateCard();

            zone.TryAdd(first);
            zone.TryAdd(second);
            zone.TryAdd(third);

            Assert.That(zone.Cards[0], Is.SameAs(first));
            Assert.That(zone.Cards[1], Is.SameAs(second));
            Assert.That(zone.Cards[2], Is.SameAs(third));
        }

        [Test]
        public void RemovingCard_PreservesRemainingOrder()
        {
            HandRuntime zone = new HandRuntime();

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();
            CardInstance third = CreateCard();

            zone.TryAdd(first);
            zone.TryAdd(second);
            zone.TryAdd(third);

            zone.TryRemove(second);

            Assert.That(zone.Count, Is.EqualTo(2));
            Assert.That(zone.Cards[0], Is.SameAs(first));
            Assert.That(zone.Cards[1], Is.SameAs(third));
        }

        [Test]
        public void Cards_CannotBeModifiedExternally()
        {
            HandRuntime zone = new HandRuntime();
            CardInstance card = CreateCard();

            IList<CardInstance> exposedCollection =
                zone.Cards as IList<CardInstance>;

            Assert.That(exposedCollection, Is.Not.Null);
            Assert.That(exposedCollection.IsReadOnly, Is.True);

            Assert.That(
                () => exposedCollection.Add(card),
                Throws.TypeOf<NotSupportedException>()
            );

            Assert.That(zone.IsEmpty, Is.True);
        }

        [Test]
        public void TryAdd_WithNullCard_Throws()
        {
            HandRuntime zone = new HandRuntime();

            Assert.That(
                () => zone.TryAdd(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void TryRemove_WithNullCard_Throws()
        {
            HandRuntime zone = new HandRuntime();

            Assert.That(
                () => zone.TryRemove(null),
                Throws.TypeOf<ArgumentNullException>()
            );
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