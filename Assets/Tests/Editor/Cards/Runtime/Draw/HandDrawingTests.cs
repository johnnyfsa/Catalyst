using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Draw
{
    public sealed class InitialHandDrawingTests
    {
        private CardDefinition definition;
        private CardDrawService drawService;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();

            drawService = new CardDrawService(
                new CardMovementService()
            );
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
        public void DrawInitialHand_DrawsRequestedCards()
        {
            DeckRuntime deck = CreateDeck(10);
            HandRuntime hand = new HandRuntime(8);

            InitialHandResult result =
                drawService.DrawInitialHand(
                    deck,
                    hand,
                    8
                );

            Assert.That(result.RequestedCardCount, Is.EqualTo(8));
            Assert.That(result.TargetCardCount, Is.EqualTo(8));
            Assert.That(result.DrawnCardCount, Is.EqualTo(8));
            Assert.That(result.Completed, Is.True);

            Assert.That(
                result.StoppedBecauseDeckWasEmpty,
                Is.False
            );

            Assert.That(hand.Count, Is.EqualTo(8));
            Assert.That(deck.Count, Is.EqualTo(2));
        }

        [Test]
        public void DrawInitialHand_LimitsTargetToHandCapacity()
        {
            DeckRuntime deck = CreateDeck(10);
            HandRuntime hand = new HandRuntime(8);

            InitialHandResult result =
                drawService.DrawInitialHand(
                    deck,
                    hand,
                    12
                );

            Assert.That(
                result.RequestedCardCount,
                Is.EqualTo(12)
            );

            Assert.That(result.TargetCardCount, Is.EqualTo(8));
            Assert.That(result.DrawnCardCount, Is.EqualTo(8));
            Assert.That(result.Completed, Is.True);

            Assert.That(hand.Count, Is.EqualTo(8));
            Assert.That(deck.Count, Is.EqualTo(2));
        }

        [Test]
        public void DrawInitialHand_WithInsufficientDeck_DrawsPartialHand()
        {
            DeckRuntime deck = CreateDeck(5);
            HandRuntime hand = new HandRuntime(8);

            InitialHandResult result =
                drawService.DrawInitialHand(
                    deck,
                    hand,
                    8
                );

            Assert.That(result.TargetCardCount, Is.EqualTo(8));
            Assert.That(result.DrawnCardCount, Is.EqualTo(5));
            Assert.That(result.Completed, Is.False);

            Assert.That(
                result.StoppedBecauseDeckWasEmpty,
                Is.True
            );

            Assert.That(hand.Count, Is.EqualTo(5));
            Assert.That(deck.IsEmpty, Is.True);
        }

        [Test]
        public void DrawInitialHand_UsesSameTopToHandFlow()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(3);

            CardInstance bottom = CreateCard();
            CardInstance middle = CreateCard();
            CardInstance top = CreateCard();

            deck.TryAdd(bottom);
            deck.TryAdd(middle);
            deck.TryAdd(top);

            InitialHandResult result =
                drawService.DrawInitialHand(
                    deck,
                    hand,
                    3
                );

            Assert.That(result.Completed, Is.True);

            Assert.That(hand.Cards[0], Is.SameAs(top));
            Assert.That(hand.Cards[1], Is.SameAs(middle));
            Assert.That(hand.Cards[2], Is.SameAs(bottom));
        }

        [Test]
        public void DrawInitialHand_WithNonEmptyHand_Throws()
        {
            DeckRuntime deck = CreateDeck(8);
            HandRuntime hand = new HandRuntime(8);

            hand.TryAdd(CreateCard());

            Assert.That(
                () => drawService.DrawInitialHand(
                    deck,
                    hand,
                    8
                ),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(deck.Count, Is.EqualTo(8));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void DrawInitialHand_WithInvalidRequestedCount_Throws(
            int invalidCount
        )
        {
            DeckRuntime deck = CreateDeck(8);
            HandRuntime hand = new HandRuntime(8);

            Assert.That(
                () => drawService.DrawInitialHand(
                    deck,
                    hand,
                    invalidCount
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void DrawInitialHand_WithNullDeck_Throws()
        {
            HandRuntime hand = new HandRuntime();

            Assert.That(
                () => drawService.DrawInitialHand(
                    null,
                    hand,
                    8
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void DrawInitialHand_WithNullHand_Throws()
        {
            DeckRuntime deck = CreateDeck(8);

            Assert.That(
                () => drawService.DrawInitialHand(
                    deck,
                    null,
                    8
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private DeckRuntime CreateDeck(int count)
        {
            DeckRuntime deck = new DeckRuntime();

            for (int index = 0; index < count; index++)
            {
                deck.TryAdd(CreateCard());
            }

            return deck;
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