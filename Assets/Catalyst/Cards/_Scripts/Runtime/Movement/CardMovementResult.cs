namespace Catalyst.Cards.Runtime.Movement
{
    public readonly struct CardMovementResult
    {
        private CardMovementResult(
            bool succeeded,
            CardMovementFailure failure
        )
        {
            Succeeded = succeeded;
            Failure = failure;
        }

        public bool Succeeded { get; }
        public CardMovementFailure Failure { get; }

        public static CardMovementResult Success()
        {
            return new CardMovementResult(
                true,
                CardMovementFailure.None
            );
        }

        public static CardMovementResult Fail(
            CardMovementFailure failure
        )
        {
            return new CardMovementResult(false, failure);
        }
    }
}