namespace Catalyst.Cards.Runtime.Turn
{
    public readonly struct MainPhaseEndResult
    {
        private MainPhaseEndResult(
            bool succeeded,
            MainPhaseEndFailure failure
        )
        {
            Succeeded = succeeded;
            Failure = failure;
        }

        public bool Succeeded { get; }

        public MainPhaseEndFailure Failure { get; }

        public static MainPhaseEndResult Success()
        {
            return new MainPhaseEndResult(
                true,
                MainPhaseEndFailure.None
            );
        }

        public static MainPhaseEndResult Fail(
            MainPhaseEndFailure failure
        )
        {
            return new MainPhaseEndResult(
                false,
                failure
            );
        }
    }
}