using System;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Turn
{
    public sealed class DrawPhaseService
    {
        private readonly CardDrawService drawService;

        public DrawPhaseService(
            CardDrawService drawService
        )
        {
            this.drawService = drawService
                ?? throw new ArgumentNullException(
                    nameof(drawService)
                );
        }

        public DrawPhaseResult Resolve(
            TurnRuntime turn,
            DeckRuntime deck,
            HandRuntime hand
        )
        {
            if (turn == null)
            {
                throw new ArgumentNullException(
                    nameof(turn)
                );
            }

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

            if (!turn.HasStarted)
            {
                throw new InvalidOperationException(
                    "The turn runtime must be started before resolving the draw phase."
                );
            }

            if (turn.CurrentPhase != GamePhase.Draw)
            {
                throw new InvalidOperationException(
                    $"The draw phase cannot be resolved while the current phase is '{turn.CurrentPhase}'."
                );
            }

            int drawnCardCount = 0;

            while (!hand.IsFull)
            {
                CardDrawResult drawResult =
                    drawService.TryDraw(
                        deck,
                        hand
                    );

                switch (drawResult.Failure)
                {
                    case CardDrawFailure.None:
                        drawnCardCount++;
                        break;

                    case CardDrawFailure.DeckEmpty:
                        return DrawPhaseResult.DeckOut(
                            drawnCardCount
                        );

                    case CardDrawFailure.HandFull:
                        /*
                         * Defensive handling.
                         *
                         * The while condition should normally
                         * prevent this result, but the service
                         * still treats it as successful phase
                         * completion if the hand reports full.
                         */
                        if (!hand.IsFull)
                        {
                            throw new InvalidOperationException(
                                "The card draw service reported a full hand, but the hand runtime is not full."
                            );
                        }

                        turn.AdvancePhase();

                        return DrawPhaseResult.HandFull(
                            drawnCardCount
                        );

                    default:
                        throw new InvalidOperationException(
                            $"The draw phase failed unexpectedly with '{drawResult.Failure}'."
                        );
                }
            }

            turn.AdvancePhase();

            return DrawPhaseResult.HandFull(
                drawnCardCount
            );
        }
    }
}