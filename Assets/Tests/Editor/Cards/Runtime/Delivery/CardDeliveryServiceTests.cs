using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Delivery;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Delivery
{
    public sealed class CardDeliveryServiceTests
    {
        private CardDefinition acceptedDefinition;
        private CardDefinition otherDefinition;

        private CardMovementService movementService;
        private CardDeliveryService deliveryService;

        [SetUp]
        public void SetUp()
        {
            acceptedDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            otherDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            movementService =
                new CardMovementService();

            deliveryService =
                new CardDeliveryService(
                    movementService
                );
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
        public void Constructor_WithNullMovementService_Throws()
        {
            Assert.That(
                () => new CardDeliveryService(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void TryDeliver_WithNullCard_ReturnsExplicitFailure()
        {
            HandRuntime hand =
                new HandRuntime();

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card: null,
                    hand,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure.NullCard
            );

            Assert.That(hand.IsEmpty, Is.True);
            Assert.That(deliveryZone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliver_WithNullHand_ReturnsExplicitFailure()
        {
            CardInstance card =
                CreateCard(acceptedDefinition);

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand: null,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure.NullHand
            );

            Assert.That(deliveryZone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliver_WithNullDeliveryZone_ReturnsExplicitFailure()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone: null
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure.NullDeliveryZone
            );

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.Contains(card), Is.True);
        }

        [Test]
        public void TryDeliver_WithCardNotInHand_ReturnsExplicitFailure()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure.CardNotInHand
            );

            Assert.That(hand.IsEmpty, Is.True);
            Assert.That(deliveryZone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliver_WithDifferentObjectUsingContainedId_ReturnsCardNotInHand()
        {
            HandRuntime hand =
                new HandRuntime();

            Guid sharedId =
                Guid.NewGuid();

            CardInstance containedCard =
                new CardInstance(
                    sharedId,
                    acceptedDefinition
                );

            CardInstance differentObject =
                new CardInstance(
                    sharedId,
                    acceptedDefinition
                );

            Assert.That(
                hand.TryAdd(containedCard),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    differentObject,
                    hand,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure.CardNotInHand
            );

            Assert.That(hand.Count, Is.EqualTo(1));

            Assert.That(
                hand.Cards[0],
                Is.SameAs(containedCard)
            );

            Assert.That(deliveryZone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliver_WithWrongDefinition_ReturnsRejectedFailure()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(otherDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure
                    .DeliveryZoneRejectedCard
            );

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.Contains(card), Is.True);
            Assert.That(deliveryZone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliver_WithAcceptedCard_ReturnsSuccess()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(CardDeliveryFailure.None)
            );
        }

        [Test]
        public void TryDeliver_WithAcceptedCard_RemovesCardFromHand()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(hand.IsEmpty, Is.True);
            Assert.That(hand.Contains(card), Is.False);
        }

        [Test]
        public void TryDeliver_WithAcceptedCard_AddsCardToDeliveryZone()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(deliveryZone.Count, Is.EqualTo(1));

            Assert.That(
                deliveryZone.Contains(card),
                Is.True
            );

            Assert.That(
                deliveryZone.Cards[0],
                Is.SameAs(card)
            );
        }

        [Test]
        public void TryDeliver_WithAcceptedCard_IncreasesZoneProgress()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance firstCard =
                CreateCard(acceptedDefinition);

            CardInstance secondCard =
                CreateCard(acceptedDefinition);

            Assert.That(hand.TryAdd(firstCard), Is.True);
            Assert.That(hand.TryAdd(secondCard), Is.True);

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone(
                    requiredAmount: 2
                );

            CardDeliveryResult firstResult =
                deliveryService.TryDeliver(
                    firstCard,
                    hand,
                    deliveryZone
                );

            Assert.That(firstResult.Succeeded, Is.True);

            Assert.That(
                deliveryZone.CurrentAmount,
                Is.EqualTo(1)
            );

            Assert.That(
                deliveryZone.IsCompleted,
                Is.False
            );

            CardDeliveryResult secondResult =
                deliveryService.TryDeliver(
                    secondCard,
                    hand,
                    deliveryZone
                );

            Assert.That(secondResult.Succeeded, Is.True);

            Assert.That(
                deliveryZone.CurrentAmount,
                Is.EqualTo(2)
            );

            Assert.That(
                deliveryZone.IsCompleted,
                Is.True
            );
        }

        [Test]
        public void TryDeliver_WhenZoneRejectsCard_DoesNotMutateHand()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance first =
                CreateCard(acceptedDefinition);

            CardInstance rejected =
                CreateCard(otherDefinition);

            CardInstance third =
                CreateCard(acceptedDefinition);

            Assert.That(hand.TryAdd(first), Is.True);
            Assert.That(hand.TryAdd(rejected), Is.True);
            Assert.That(hand.TryAdd(third), Is.True);

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    rejected,
                    hand,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure
                    .DeliveryZoneRejectedCard
            );

            Assert.That(hand.Count, Is.EqualTo(3));
            Assert.That(hand.Cards[0], Is.SameAs(first));
            Assert.That(hand.Cards[1], Is.SameAs(rejected));
            Assert.That(hand.Cards[2], Is.SameAs(third));
        }

        [Test]
        public void TryDeliver_WhenZoneRejectsCard_DoesNotMutateDeliveryZone()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance rejected =
                CreateCard(otherDefinition);

            Assert.That(
                hand.TryAdd(rejected),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            int initialAmount =
                deliveryZone.CurrentAmount;

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    rejected,
                    hand,
                    deliveryZone
                );

            AssertFailedWith(
                result,
                CardDeliveryFailure
                    .DeliveryZoneRejectedCard
            );

            Assert.That(
                deliveryZone.CurrentAmount,
                Is.EqualTo(initialAmount)
            );

            Assert.That(deliveryZone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliver_PreservesRemainingHandOrder()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance first =
                CreateCard(acceptedDefinition);

            CardInstance delivered =
                CreateCard(acceptedDefinition);

            CardInstance third =
                CreateCard(acceptedDefinition);

            Assert.That(hand.TryAdd(first), Is.True);
            Assert.That(hand.TryAdd(delivered), Is.True);
            Assert.That(hand.TryAdd(third), Is.True);

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    delivered,
                    hand,
                    deliveryZone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(hand.Count, Is.EqualTo(2));
            Assert.That(hand.Cards[0], Is.SameAs(first));
            Assert.That(hand.Cards[1], Is.SameAs(third));
        }

        [Test]
        public void DeliveredCard_CannotBeRemovedFromDeliveryZone()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone();

            CardDeliveryResult result =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                deliveryZone.CanRemove(card),
                Is.False
            );

            Assert.That(
                deliveryZone.TryRemove(card),
                Is.False
            );

            Assert.That(
                deliveryZone.Contains(card),
                Is.True
            );
        }

        [Test]
        public void TryDeliver_SameCardTwice_ReturnsCardNotInHand()
        {
            HandRuntime hand =
                new HandRuntime();

            CardInstance card =
                CreateCard(acceptedDefinition);

            Assert.That(
                hand.TryAdd(card),
                Is.True
            );

            CardDeliveryZoneRuntime deliveryZone =
                CreateDeliveryZone(
                    requiredAmount: 2
                );

            CardDeliveryResult firstResult =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            CardDeliveryResult secondResult =
                deliveryService.TryDeliver(
                    card,
                    hand,
                    deliveryZone
                );

            Assert.That(firstResult.Succeeded, Is.True);

            AssertFailedWith(
                secondResult,
                CardDeliveryFailure.CardNotInHand
            );

            Assert.That(deliveryZone.Count, Is.EqualTo(1));
            Assert.That(hand.IsEmpty, Is.True);
        }

        private CardDeliveryZoneRuntime CreateDeliveryZone(
            int requiredAmount = 10
        )
        {
            return new CardDeliveryZoneRuntime(
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

        private static void AssertFailedWith(
            CardDeliveryResult result,
            CardDeliveryFailure expectedFailure
        )
        {
            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(expectedFailure)
            );
        }
    }
}