namespace Catalyst.Cards.Runtime.Turn
{
    public sealed class EndPhaseService
    {
        public EndPhaseResult Resolve(
            TurnRuntime turn
        )
        {
            if (turn == null)
            {
                return EndPhaseResult.Fail(
                    EndPhaseFailure.NullTurn
                );
            }

            if (!turn.HasStarted)
            {
                return EndPhaseResult.Fail(
                    EndPhaseFailure.TurnNotStarted
                );
            }

            if (turn.CurrentPhase != GamePhase.End)
            {
                return EndPhaseResult.Fail(
                    EndPhaseFailure.NotInEndPhase
                );
            }

            int completedTurnNumber =
                turn.TurnNumber;

            turn.AdvancePhase();

            return EndPhaseResult.Success(
                completedTurnNumber,
                turn.TurnNumber
            );
        }
    }
}