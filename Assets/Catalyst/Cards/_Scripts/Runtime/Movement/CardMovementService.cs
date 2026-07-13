using System;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Movement
{
    public sealed class CardMovementService
    {
        public CardMovementResult TryMove(
            CardInstance card,
            CardZoneRuntime source,
            CardZoneRuntime destination
        )
        {
            CardMovementResult validationResult =
                Validate(card, source, destination);

            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            bool removed = source.TryRemove(
                card,
                out int originalIndex
            );

            if (!removed)
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.CardNotInSource
                );
            }

            bool added = destination.TryAdd(card);

            if (added)
            {
                return CardMovementResult.Success();
            }

            bool restored = source.TryInsertAt(
                card,
                originalIndex
            );

            if (!restored)
            {
                throw new InvalidOperationException(
                    "Card movement failed and the source zone could not be restored."
                );
            }

            return CardMovementResult.Fail(
                CardMovementFailure.DestinationRejectedCard
            );
        }

        private static CardMovementResult Validate(
            CardInstance card,
            CardZoneRuntime source,
            CardZoneRuntime destination
        )
        {
            if (card == null)
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.NullCard
                );
            }

            if (source == null)
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.NullSource
                );
            }

            if (destination == null)
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.NullDestination
                );
            }

            if (ReferenceEquals(source, destination))
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.SameZone
                );
            }

            if (!source.Contains(card))
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.CardNotInSource
                );
            }

            if (!source.CanRemove(card))
            {
                return CardMovementResult.Fail(
                    CardMovementFailure.SourceDoesNotAllowRemoval
                );
            }

            if (destination.Contains(card))
            {
                return CardMovementResult.Fail(
                    CardMovementFailure
                        .DestinationAlreadyContainsCard
                );
            }

            return CardMovementResult.Success();
        }
    }
}