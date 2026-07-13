using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Movement
{
    public sealed class CardMovementServiceTests
    {
        private CardDefinition definition;
        private CardMovementService movementService;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();

            movementService = new CardMovementService();
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
        public void TryMove_WithValidMovement_MovesCardBetweenZones()
        {
            HandRuntime hand = new HandRuntime();
            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            CardInstance card = CreateCard();

            hand.TryAdd(card);

            CardMovementResult result =
                movementService.TryMove(
                    card,
                    hand,
                    reactionTable
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardMovementFailure.None)
            );

            Assert.That(hand.Contains(card), Is.False);
            Assert.That(hand.Count, Is.Zero);

            Assert.That(reactionTable.Contains(card), Is.True);
            Assert.That(reactionTable.Count, Is.EqualTo(1));
            Assert.That(
                reactionTable.Cards[0],
                Is.SameAs(card)
            );
        }

        [Test]
        public void TryMove_PreservesCardIdentity()
        {
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardInstance card = CreateCard();
            Guid originalId = card.InstanceId;

            deck.TryAdd(card);

            CardMovementResult result =
                movementService.TryMove(card, deck, hand);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(hand.Cards[0], Is.SameAs(card));
            Assert.That(
                hand.Cards[0].InstanceId,
                Is.EqualTo(originalId)
            );
            Assert.That(
                hand.Cards[0].Definition,
                Is.SameAs(definition)
            );
        }

        [Test]
        public void TryMove_WithCardAbsentFromSource_DoesNotChangeZones()
        {
            HandRuntime hand = new HandRuntime();
            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            CardInstance card = CreateCard();

            CardMovementResult result =
                movementService.TryMove(
                    card,
                    hand,
                    reactionTable
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardMovementFailure.CardNotInSource
                )
            );

            Assert.That(hand.IsEmpty, Is.True);
            Assert.That(reactionTable.IsEmpty, Is.True);
        }

        [Test]
        public void TryMove_ToSameZone_DoesNotChangeZone()
        {
            HandRuntime hand = new HandRuntime();
            CardInstance card = CreateCard();

            hand.TryAdd(card);

            CardMovementResult result =
                movementService.TryMove(card, hand, hand);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardMovementFailure.SameZone)
            );

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.Cards[0], Is.SameAs(card));
        }

        [Test]
        public void TryMove_WhenDestinationContainsSameIdentity_DoesNotChangeZones()
        {
            HandRuntime hand = new HandRuntime();
            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            Guid sharedId = Guid.NewGuid();

            CardInstance sourceCard =
                new CardInstance(sharedId, definition);

            CardInstance duplicateIdentity =
                new CardInstance(sharedId, definition);

            hand.TryAdd(sourceCard);
            reactionTable.TryAdd(duplicateIdentity);

            CardMovementResult result =
                movementService.TryMove(
                    sourceCard,
                    hand,
                    reactionTable
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardMovementFailure
                        .DestinationAlreadyContainsCard
                )
            );

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(
                hand.Cards[0],
                Is.SameAs(sourceCard)
            );

            Assert.That(
                reactionTable.Count,
                Is.EqualTo(1)
            );

            Assert.That(
                reactionTable.Cards[0],
                Is.SameAs(duplicateIdentity)
            );
        }

        [Test]
        public void TryMove_FromDiscardPile_IsRejected()
        {
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            HandRuntime hand = new HandRuntime();
            CardInstance card = CreateCard();

            discardPile.TryAdd(card);

            CardMovementResult result =
                movementService.TryMove(
                    card,
                    discardPile,
                    hand
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardMovementFailure
                        .SourceDoesNotAllowRemoval
                )
            );

            Assert.That(discardPile.Count, Is.EqualTo(1));
            Assert.That(discardPile.Contains(card), Is.True);
            Assert.That(hand.IsEmpty, Is.True);
        }

        [Test]
        public void TryMove_ToDiscardPile_MovesCardPermanently()
        {
            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance card = CreateCard();

            reactionTable.TryAdd(card);

            CardMovementResult result =
                movementService.TryMove(
                    card,
                    reactionTable,
                    discardPile
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(reactionTable.IsEmpty, Is.True);
            Assert.That(discardPile.Count, Is.EqualTo(1));
            Assert.That(discardPile.Cards[0], Is.SameAs(card));

            CardMovementResult returnResult =
                movementService.TryMove(
                    card,
                    discardPile,
                    reactionTable
                );

            Assert.That(returnResult.Succeeded, Is.False);
            Assert.That(discardPile.Contains(card), Is.True);
            Assert.That(reactionTable.IsEmpty, Is.True);
        }

        [Test]
        public void InvalidMovement_PreservesSourceOrder()
        {
            HandRuntime hand = new HandRuntime();
            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();
            CardInstance third = CreateCard();

            hand.TryAdd(first);
            hand.TryAdd(second);
            hand.TryAdd(third);

            reactionTable.TryAdd(
                new CardInstance(second.InstanceId, definition)
            );

            CardMovementResult result =
                movementService.TryMove(
                    second,
                    hand,
                    reactionTable
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(hand.Count, Is.EqualTo(3));
            Assert.That(hand.Cards[0], Is.SameAs(first));
            Assert.That(hand.Cards[1], Is.SameAs(second));
            Assert.That(hand.Cards[2], Is.SameAs(third));
        }

        [Test]
        public void TryMove_WithNullCard_ReturnsExplicitFailure()
        {
            HandRuntime hand = new HandRuntime();
            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            CardMovementResult result =
                movementService.TryMove(
                    null,
                    hand,
                    reactionTable
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardMovementFailure.NullCard)
            );
        }

        [Test]
        public void TryMove_WithNullSource_ReturnsExplicitFailure()
        {
            HandRuntime hand = new HandRuntime();
            CardInstance card = CreateCard();

            CardMovementResult result =
                movementService.TryMove(
                    card,
                    null,
                    hand
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(CardMovementFailure.NullSource)
            );
        }

        [Test]
        public void TryMove_WithNullDestination_ReturnsExplicitFailure()
        {
            HandRuntime hand = new HandRuntime();
            CardInstance card = CreateCard();

            hand.TryAdd(card);

            CardMovementResult result =
                movementService.TryMove(
                    card,
                    hand,
                    null
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardMovementFailure.NullDestination
                )
            );

            Assert.That(hand.Contains(card), Is.True);
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