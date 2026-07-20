using System;
using System.Linq;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Turn
{
    public sealed class MainPhaseService
    {
        private readonly CardMovementService movementService;

        public MainPhaseService(
            CardMovementService movementService
        )
        {
            this.movementService = movementService
                ?? throw new ArgumentNullException(
                    nameof(movementService)
                );
        }

        public MainPhaseEndResult TryEnd(
            TurnRuntime turn,
            HandRuntime hand,
            ReactionTableRuntime reactionTable
        )
        {
            if (turn == null)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.NullTurn
                );
            }

            if (hand == null)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.NullHand
                );
            }

            if (reactionTable == null)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.NullReactionTable
                );
            }

            if (!turn.HasStarted)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.TurnNotStarted
                );
            }

            if (turn.CurrentPhase != GamePhase.Main)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.NotInMainPhase
                );
            }

            int projectedHandCount =
                hand.Count + reactionTable.Count;

            if (projectedHandCount >= hand.Capacity)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.HandFull
                );
            }

            ReturnTableCardsToHand(
                reactionTable,
                hand
            );

            turn.AdvancePhase();

            return MainPhaseEndResult.Success();
        }

        private void ReturnTableCardsToHand(
            ReactionTableRuntime reactionTable,
            HandRuntime hand
        )
        {
            CardInstance[] cardsToReturn =
                reactionTable.Cards.ToArray();

            foreach (CardInstance card in cardsToReturn)
            {
                CardMovementResult movementResult =
                    movementService.TryMove(
                        card,
                        reactionTable,
                        hand
                    );

                if (!movementResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "A validated reaction table card could not be returned to the hand. " +
                        $"Failure: '{movementResult.Failure}'."
                    );
                }
            }
        }
    }
}