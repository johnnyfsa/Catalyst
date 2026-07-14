using System;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Draw
{
    public sealed class CardDrawService
    {
        private readonly CardMovementService movementService;

        public CardDrawService(
            CardMovementService movementService
        )
        {
            this.movementService = movementService
                ?? throw new ArgumentNullException(
                    nameof(movementService)
                );
        }

        public CardDrawResult TryDraw(
            DeckRuntime deck,
            HandRuntime hand
        )
        {
            if (deck == null)
            {
                return CardDrawResult.Fail(
                    CardDrawFailure.NullDeck
                );
            }

            if (hand == null)
            {
                return CardDrawResult.Fail(
                    CardDrawFailure.NullHand
                );
            }

            if (hand.IsFull)
            {
                return CardDrawResult.Fail(
                    CardDrawFailure.HandFull
                );
            }

            if (!deck.TryPeekTop(out CardInstance topCard))
            {
                return CardDrawResult.Fail(
                    CardDrawFailure.DeckEmpty
                );
            }

            CardMovementResult movementResult =
                movementService.TryMove(
                    topCard,
                    deck,
                    hand
                );

            if (!movementResult.Succeeded)
            {
                return CardDrawResult.Fail(
                    CardDrawFailure.MovementFailed,
                    movementResult.Failure
                );
            }

            return CardDrawResult.Success(topCard);
        }

        public InitialHandResult DrawInitialHand(
            DeckRuntime deck,
            HandRuntime hand,
            int requestedCardCount
        )
        {
            if (deck == null)
            {
                throw new ArgumentNullException(
                    nameof(deck)
                );
            }

            if (hand == null)
            {
                throw new ArgumentNullException(
                    nameof(hand)
                );
            }

            if (requestedCardCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedCardCount),
                    requestedCardCount,
                    "Initial hand size must be greater than zero."
                );
            }

            if (!hand.IsEmpty)
            {
                throw new InvalidOperationException(
                    "The initial hand can only be drawn into an empty hand."
                );
            }

            int targetCardCount = Math.Min(
                requestedCardCount,
                hand.Capacity
            );

            int drawnCardCount = 0;
            bool stoppedBecauseDeckWasEmpty = false;

            while (drawnCardCount < targetCardCount)
            {
                CardDrawResult drawResult =
                    TryDraw(deck, hand);

                if (drawResult.Succeeded)
                {
                    drawnCardCount++;
                    continue;
                }

                if (drawResult.Failure ==
                    CardDrawFailure.DeckEmpty)
                {
                    stoppedBecauseDeckWasEmpty = true;
                    break;
                }

                throw new InvalidOperationException(
                    $"Initial hand drawing failed unexpectedly with '{drawResult.Failure}'."
                );
            }

            return new InitialHandResult(
                requestedCardCount,
                targetCardCount,
                drawnCardCount,
                stoppedBecauseDeckWasEmpty
            );
        }
    }
}