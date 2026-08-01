namespace Catalyst.Cards.Runtime.Turn
{
    public readonly struct TurnAdvanceResult
    {
        private TurnAdvanceResult(
            bool succeeded,
            TurnAdvanceFailure failure,
            MainPhaseEndFailure mainPhaseFailure,
            EndPhaseFailure endPhaseFailure,
            int completedTurnNumber,
            int startedTurnNumber
        )
        {
            Succeeded = succeeded;
            Failure = failure;
            MainPhaseFailure = mainPhaseFailure;
            EndPhaseFailure = endPhaseFailure;
            CompletedTurnNumber = completedTurnNumber;
            StartedTurnNumber = startedTurnNumber;
        }

        public bool Succeeded { get; }

        public TurnAdvanceFailure Failure { get; }

        public MainPhaseEndFailure MainPhaseFailure
        {
            get;
        }

        public EndPhaseFailure EndPhaseFailure
        {
            get;
        }

        public int CompletedTurnNumber { get; }

        public int StartedTurnNumber { get; }

        public static TurnAdvanceResult Success(
            int completedTurnNumber,
            int startedTurnNumber
        )
        {
            return new TurnAdvanceResult(
                succeeded: true,
                failure: TurnAdvanceFailure.None,
                mainPhaseFailure:
                    MainPhaseEndFailure.None,
                endPhaseFailure:
                    EndPhaseFailure.None,
                completedTurnNumber:
                    completedTurnNumber,
                startedTurnNumber:
                    startedTurnNumber
            );
        }

        public static TurnAdvanceResult MainPhaseFailed(
            MainPhaseEndFailure failure
        )
        {
            return new TurnAdvanceResult(
                succeeded: false,
                failure:
                    TurnAdvanceFailure
                        .MainPhaseCouldNotEnd,
                mainPhaseFailure: failure,
                endPhaseFailure:
                    EndPhaseFailure.None,
                completedTurnNumber: 0,
                startedTurnNumber: 0
            );
        }

        public static TurnAdvanceResult EndPhaseFailed(
            EndPhaseFailure failure
        )
        {
            return new TurnAdvanceResult(
                succeeded: false,
                failure:
                    TurnAdvanceFailure
                        .EndPhaseCouldNotResolve,
                mainPhaseFailure:
                    MainPhaseEndFailure.None,
                endPhaseFailure: failure,
                completedTurnNumber: 0,
                startedTurnNumber: 0
            );
        }
    }
}