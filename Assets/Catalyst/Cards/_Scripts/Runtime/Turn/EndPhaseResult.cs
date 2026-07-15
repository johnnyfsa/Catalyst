namespace Catalyst.Cards.Runtime.Turn
{
    public readonly struct EndPhaseResult
    {
        private EndPhaseResult(
            bool succeeded,
            EndPhaseFailure failure,
            int completedTurnNumber,
            int startedTurnNumber
        )
        {
            Succeeded = succeeded;
            Failure = failure;
            CompletedTurnNumber = completedTurnNumber;
            StartedTurnNumber = startedTurnNumber;
        }

        public bool Succeeded { get; }

        public EndPhaseFailure Failure { get; }

        public int CompletedTurnNumber { get; }

        public int StartedTurnNumber { get; }

        public static EndPhaseResult Success(
            int completedTurnNumber,
            int startedTurnNumber
        )
        {
            return new EndPhaseResult(
                true,
                EndPhaseFailure.None,
                completedTurnNumber,
                startedTurnNumber
            );
        }

        public static EndPhaseResult Fail(
            EndPhaseFailure failure
        )
        {
            return new EndPhaseResult(
                false,
                failure,
                completedTurnNumber: 0,
                startedTurnNumber: 0
            );
        }
    }
}