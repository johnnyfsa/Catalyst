using System;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Discard
{
    public sealed class ManualDiscardService
    {
        private readonly CardMovementService movementService;

        public ManualDiscardService(
            CardMovementService movementService
        )
        {
            this.movementService = movementService
                ?? throw new ArgumentNullException(
                    nameof(movementService)
                );
        }

        public ManualDiscardResult TryDiscard(
            CardInstance card,
            CardZoneRuntime source,
            DiscardPileRuntime discardPile
        )
        {
            if (card == null)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.NullCard
                );
            }

            if (source == null)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.NullSource
                );
            }

            if (discardPile == null)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.NullDiscardPile
                );
            }

            if (!IsSupportedSource(source))
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.UnsupportedSource
                );
            }

            if (!source.Contains(card))
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.CardNotInSource
                );
            }

            CardMovementResult movementResult =
                movementService.TryMove(
                    card,
                    source,
                    discardPile
                );

            if (!movementResult.Succeeded)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.MovementFailed,
                    movementResult.Failure
                );
            }

            return ManualDiscardResult.Success(card);
        }

        private static bool IsSupportedSource(
            CardZoneRuntime source
        )
        {
            return source is HandRuntime
                || source is ReactionTableRuntime;
        }
    }
}