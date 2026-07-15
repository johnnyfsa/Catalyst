using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Turn
{
    public sealed class MainPhaseService
    {
        public MainPhaseEndResult TryEnd(
            TurnRuntime turn,
            HandRuntime hand
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

            if (hand.IsFull)
            {
                return MainPhaseEndResult.Fail(
                    MainPhaseEndFailure.HandFull
                );
            }

            turn.AdvancePhase();

            return MainPhaseEndResult.Success();
        }
    }
}