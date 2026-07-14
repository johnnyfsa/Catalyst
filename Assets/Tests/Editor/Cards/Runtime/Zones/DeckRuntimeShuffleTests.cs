using System;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Zones
{
    public sealed class DeckRuntimeShuffleTests
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
        public void Shuffle_WithSameSeed_ProducesSameOrder()
        {
            DeckRuntime firstDeck = CreateDeck(10);
            DeckRuntime secondDeck = CloneDeckIdentityOrder(
                firstDeck
            );

            firstDeck.Shuffle(
                new SeededRandomSource(12345)
            );

            secondDeck.Shuffle(
                new SeededRandomSource(12345)
            );

            Guid[] firstOrder = firstDeck.Cards
                .Select(card => card.InstanceId)
                .ToArray();

            Guid[] secondOrder = secondDeck.Cards
                .Select(card => card.InstanceId)
                .ToArray();

            Assert.That(
                firstOrder,
                Is.EqualTo(secondOrder)
            );
        }

        [Test]
        public void Shuffle_PreservesAllCards()
        {
            DeckRuntime deck = CreateDeck(10);

            Guid[] originalIds = deck.Cards
                .Select(card => card.InstanceId)
                .OrderBy(id => id)
                .ToArray();

            deck.Shuffle(
                new SeededRandomSource(9876)
            );

            Guid[] shuffledIds = deck.Cards
                .Select(card => card.InstanceId)
                .OrderBy(id => id)
                .ToArray();

            Assert.That(deck.Count, Is.EqualTo(10));

            Assert.That(
                shuffledIds,
                Is.EqualTo(originalIds)
            );
        }

        [Test]
        public void Shuffle_PreservesCardReferences()
        {
            DeckRuntime deck = CreateDeck(5);

            CardInstance[] originalCards =
                deck.Cards.ToArray();

            deck.Shuffle(
                new SeededRandomSource(2468)
            );

            foreach (CardInstance card in originalCards)
            {
                Assert.That(
                    deck.Cards,
                    Does.Contain(card)
                );
            }
        }

        [Test]
        public void Shuffle_EmptyDeck_DoesNotFail()
        {
            DeckRuntime deck = new DeckRuntime();

            Assert.That(
                () => deck.Shuffle(
                    new SeededRandomSource(1)
                ),
                Throws.Nothing
            );

            Assert.That(deck.IsEmpty, Is.True);
        }

        [Test]
        public void Shuffle_DeckWithOneCard_DoesNotChangeDeck()
        {
            DeckRuntime deck = CreateDeck(1);
            CardInstance originalCard = deck.Cards[0];

            deck.Shuffle(
                new SeededRandomSource(1)
            );

            Assert.That(deck.Count, Is.EqualTo(1));
            Assert.That(
                deck.Cards[0],
                Is.SameAs(originalCard)
            );
        }

        [Test]
        public void Shuffle_WithNullRandomSource_Throws()
        {
            DeckRuntime deck = CreateDeck(3);

            Assert.That(
                () => deck.Shuffle(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private DeckRuntime CreateDeck(int cardCount)
        {
            DeckRuntime deck = new DeckRuntime();

            for (int index = 0;
                 index < cardCount;
                 index++)
            {
                deck.TryAdd(
                    new CardInstance(
                        Guid.NewGuid(),
                        definition
                    )
                );
            }

            return deck;
        }

        private DeckRuntime CloneDeckIdentityOrder(
            DeckRuntime source
        )
        {
            DeckRuntime clone = new DeckRuntime();

            foreach (CardInstance sourceCard in source.Cards)
            {
                clone.TryAdd(
                    new CardInstance(
                        sourceCard.InstanceId,
                        sourceCard.Definition
                    )
                );
            }

            return clone;
        }
    }
}