using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Discard
{
    public sealed class ManualDiscardServiceTests
    {
        private CardDefinition definition;
        private ManualDiscardService discardService;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();

            discardService = new ManualDiscardService(
                new CardMovementService()
            );
        }

        [TearDown]
        public void TearDown()
        {
            if (definition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    definition
                );
            }
        }

        [Test]
        public void TryDiscard_WithCardInHand_MovesCardToDiscardPile()
        {
            HandRuntime hand = new HandRuntime();
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance card = CreateCard();
            hand.TryAdd(card);

            ManualDiscardResult result =
                discardService.TryDiscard(
                    card,
                    hand,
                    discardPile
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(
                result.Failure,
                Is.EqualTo(ManualDiscardFailure.None)
            );

            Assert.That(
                result.DiscardedCard,
                Is.SameAs(card)
            );

            Assert.That(hand.IsEmpty, Is.True);
            Assert.That(discardPile.Count, Is.EqualTo(1));
            Assert.That(
                discardPile.Cards[0],
                Is.SameAs(card)
            );
        }

        [Test]
        public void TryDiscard_PreservesIdentityAndDefinition()
        {
            HandRuntime hand = new HandRuntime();
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance card = CreateCard();
            Guid originalId = card.InstanceId;

            hand.TryAdd(card);

            ManualDiscardResult result =
                discardService.TryDiscard(
                    card,
                    hand,
                    discardPile
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.DiscardedCard.InstanceId,
                Is.EqualTo(originalId)
            );

            Assert.That(
                result.DiscardedCard.Definition,
                Is.SameAs(definition)
            );
        }

        [Test]
        public void TryDiscard_CompactsRemainingHandOrder()
        {
            HandRuntime hand = new HandRuntime();
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();
            CardInstance third = CreateCard();

            hand.TryAdd(first);
            hand.TryAdd(second);
            hand.TryAdd(third);

            ManualDiscardResult result =
                discardService.TryDiscard(
                    second,
                    hand,
                    discardPile
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(hand.Count, Is.EqualTo(2));

            Assert.That(
                hand.Cards[0],
                Is.SameAs(first)
            );

            Assert.That(
                hand.Cards[1],
                Is.SameAs(third)
            );
        }

        [Test]
        public void TryDiscard_FromFullHand_FreesOneSlot()
        {
            HandRuntime hand = new HandRuntime(2);
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance first = CreateCard();
            CardInstance second = CreateCard();

            hand.TryAdd(first);
            hand.TryAdd(second);

            Assert.That(hand.IsFull, Is.True);

            ManualDiscardResult result =
                discardService.TryDiscard(
                    first,
                    hand,
                    discardPile
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(hand.IsFull, Is.False);
            Assert.That(hand.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryDiscard_WithCardNotInHand_DoesNotChangeZones()
        {
            HandRuntime hand = new HandRuntime();
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance card = CreateCard();

            ManualDiscardResult result =
                discardService.TryDiscard(
                    card,
                    hand,
                    discardPile
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ManualDiscardFailure.CardNotInHand
                )
            );

            Assert.That(hand.IsEmpty, Is.True);
            Assert.That(discardPile.IsEmpty, Is.True);
        }

        [Test]
        public void TryDiscard_SameCardTwice_SecondAttemptFails()
        {
            HandRuntime hand = new HandRuntime();
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance card = CreateCard();
            hand.TryAdd(card);

            ManualDiscardResult firstResult =
                discardService.TryDiscard(
                    card,
                    hand,
                    discardPile
                );

            ManualDiscardResult secondResult =
                discardService.TryDiscard(
                    card,
                    hand,
                    discardPile
                );

            Assert.That(firstResult.Succeeded, Is.True);
            Assert.That(secondResult.Succeeded, Is.False);

            Assert.That(
                secondResult.Failure,
                Is.EqualTo(
                    ManualDiscardFailure.CardNotInHand
                )
            );

            Assert.That(discardPile.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryDiscard_WithNullCard_ReturnsExplicitFailure()
        {
            ManualDiscardResult result =
                discardService.TryDiscard(
                    null,
                    new HandRuntime(),
                    new DiscardPileRuntime()
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(ManualDiscardFailure.NullCard)
            );
        }

        [Test]
        public void TryDiscard_WithNullHand_ReturnsExplicitFailure()
        {
            ManualDiscardResult result =
                discardService.TryDiscard(
                    CreateCard(),
                    null,
                    new DiscardPileRuntime()
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(ManualDiscardFailure.NullHand)
            );
        }

        [Test]
        public void TryDiscard_WithNullDiscardPile_ReturnsExplicitFailure()
        {
            HandRuntime hand = new HandRuntime();
            CardInstance card = CreateCard();

            hand.TryAdd(card);

            ManualDiscardResult result =
                discardService.TryDiscard(
                    card,
                    hand,
                    null
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ManualDiscardFailure.NullDiscardPile
                )
            );

            Assert.That(hand.Contains(card), Is.True);
        }

        [Test]
        public void Constructor_WithNullMovementService_Throws()
        {
            Assert.That(
                () => new ManualDiscardService(null),
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