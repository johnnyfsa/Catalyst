using System;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Delivery
{
    public sealed class CardDeliveryService
    {
        private readonly CardMovementService movementService;

        public CardDeliveryService(
            CardMovementService movementService
        )
        {
            this.movementService = movementService
                ?? throw new ArgumentNullException(
                    nameof(movementService)
                );
        }

        public CardDeliveryResult TryDeliver(
            CardInstance card,
            HandRuntime hand,
            CardDeliveryZoneRuntime deliveryZone
        )
        {
            CardDeliveryResult validationResult =
                Validate(
                    card,
                    hand,
                    deliveryZone
                );

            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            CardMovementResult movementResult =
                movementService.TryMove(
                    card,
                    hand,
                    deliveryZone
                );

            if (movementResult.Succeeded)
            {
                return CardDeliveryResult.Success();
            }

            return MapMovementFailure(
                movementResult.Failure
            );
        }

        private static CardDeliveryResult Validate(
    CardInstance card,
    HandRuntime hand,
    CardDeliveryZoneRuntime deliveryZone
)
        {
            if (card == null)
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure.NullCard
                );
            }

            if (hand == null)
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure.NullHand
                );
            }

            if (deliveryZone == null)
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure.NullDeliveryZone
                );
            }

            if (!ContainsExactInstance(hand, card))
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure.CardNotInHand
                );
            }

            if (!deliveryZone.CanAdd(card))
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure
                        .DeliveryZoneRejectedCard
                );
            }

            return CardDeliveryResult.Success();
        }

        private static bool ContainsExactInstance(
            HandRuntime hand,
            CardInstance card
        )
        {
            foreach (CardInstance containedCard in hand.Cards)
            {
                if (ReferenceEquals(
                    containedCard,
                    card
                ))
                {
                    return true;
                }
            }

            return false;
        }

        private static CardDeliveryResult MapMovementFailure(
            CardMovementFailure failure
        )
        {
            switch (failure)
            {
                case CardMovementFailure.CardNotInSource:
                    return CardDeliveryResult.Fail(
                        CardDeliveryFailure.CardNotInHand
                    );

                case CardMovementFailure.DestinationCannotReceiveCard:
                case CardMovementFailure.DestinationRejectedCard:
                case CardMovementFailure
                    .DestinationAlreadyContainsCard:
                    return CardDeliveryResult.Fail(
                        CardDeliveryFailure
                            .DeliveryZoneRejectedCard
                    );

                default:
                    return CardDeliveryResult.Fail(
                        CardDeliveryFailure.MovementFailed
                    );
            }
        }
    }
}