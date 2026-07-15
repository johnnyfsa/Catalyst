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
            HandRuntime hand,
            DiscardPileRuntime discardPile
        )
        {
            if (card == null)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.NullCard
                );
            }

            if (hand == null)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.NullHand
                );
            }

            if (discardPile == null)
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.NullDiscardPile
                );
            }

            if (!hand.Contains(card))
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.CardNotInHand
                );
            }

            CardMovementResult movementResult =
                movementService.TryMove(
                    card,
                    hand,
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
    }
}