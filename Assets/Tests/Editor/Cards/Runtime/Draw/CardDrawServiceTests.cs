using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Drawing;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Drawing
{
    public sealed class CardDrawServiceTests
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
        public void TryDraw_MovesTopCardFromDeckToHand()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardInstance bottomCard = CreateCard();
            CardInstance topCard = CreateCard();

            deck.TryAdd(bottomCard);
            deck.TryAdd(topCard);

            CardDrawResult result =
                drawService.TryDraw(deck, hand);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.DrawnCard, Is.SameAs(topCard));

            Assert.That(deck.Count, Is.EqualTo(1));
            Assert.That(deck.Cards[0], Is.SameAs(bottomCard));

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.Cards[0], Is.SameAs(topCard));
        }

        [Test]
        public void TryDraw_PreservesCardIdentityAndDefinition()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardInstance card = CreateCard();
            Guid originalId = card.InstanceId;

            deck.TryAdd(card);

            CardDrawResult result =
                drawService.TryDraw(deck, hand);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.DrawnCard, Is.SameAs(card));

            Assert.That(
                result.DrawnCard.InstanceId,
                Is.EqualTo(originalId)
            );

            Assert.That(
                result.DrawnCard.Definition,
                Is.SameAs(definition)
            );
        }

        [Test]
        public void TryDraw_AddsCardAtEndOfHand()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardInstance existingCard = CreateCard();
            CardInstance drawnCard = CreateCard();

            hand.TryAdd(existingCard);
            deck.TryAdd(drawnCard);

            CardDrawResult result =
                drawService.TryDraw(deck, hand);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(hand.Count, Is.EqualTo(2));
            Assert.That(
                hand.Cards[0],
                Is.SameAs(existingCard)
            );
            Assert.That(
                hand.Cards[1],
                Is.SameAs(drawnCard)
            );
        }

        [Test]
        public void TryDraw_WithEmptyDeck_ReturnsDeckEmpty()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardDrawResult result =
                drawService.TryDraw(deck, hand);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardDrawFailure.DeckEmpty)
            );

            Assert.That(result.DrawnCard, Is.Null);
            Assert.That(hand.IsEmpty, Is.True);
        }

        [Test]
        public void TryDraw_WithFullHand_DoesNotRemoveTopCard()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(1);

            CardInstance handCard = CreateCard();
            CardInstance deckCard = CreateCard();

            hand.TryAdd(handCard);
            deck.TryAdd(deckCard);

            CardDrawResult result =
                drawService.TryDraw(deck, hand);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardDrawFailure.HandFull)
            );

            Assert.That(deck.Count, Is.EqualTo(1));
            Assert.That(deck.Cards[0], Is.SameAs(deckCard));

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.Cards[0], Is.SameAs(handCard));
        }

        [Test]
        public void SuccessiveDraws_FollowDeckTopOrder()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardInstance firstInserted = CreateCard();
            CardInstance secondInserted = CreateCard();
            CardInstance thirdInserted = CreateCard();

            deck.TryAdd(firstInserted);
            deck.TryAdd(secondInserted);
            deck.TryAdd(thirdInserted);

            CardDrawResult firstDraw =
                drawService.TryDraw(deck, hand);

            CardDrawResult secondDraw =
                drawService.TryDraw(deck, hand);

            CardDrawResult thirdDraw =
                drawService.TryDraw(deck, hand);

            Assert.That(
                firstDraw.DrawnCard,
                Is.SameAs(thirdInserted)
            );

            Assert.That(
                secondDraw.DrawnCard,
                Is.SameAs(secondInserted)
            );

            Assert.That(
                thirdDraw.DrawnCard,
                Is.SameAs(firstInserted)
            );

            Assert.That(
                hand.Cards[0],
                Is.SameAs(thirdInserted)
            );

            Assert.That(
                hand.Cards[1],
                Is.SameAs(secondInserted)
            );

            Assert.That(
                hand.Cards[2],
                Is.SameAs(firstInserted)
            );
        }

        [Test]
        public void TryDraw_WithNullDeck_ReturnsExplicitFailure()
        {
            HandRuntime hand = new HandRuntime();

            CardDrawResult result =
                drawService.TryDraw(null, hand);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardDrawFailure.NullDeck)
            );
        }

        [Test]
        public void TryDraw_WithNullHand_ReturnsExplicitFailure()
        {
            DeckRuntime deck = new DeckRuntime();

            CardDrawResult result =
                drawService.TryDraw(deck, null);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardDrawFailure.NullHand)
            );
        }

        [Test]
        public void Constructor_WithNullMovementService_Throws()
        {
            Assert.That(
                () => new CardDrawService(null),
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