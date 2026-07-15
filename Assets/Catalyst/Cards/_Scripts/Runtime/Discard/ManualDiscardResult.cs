using Catalyst.Cards.Runtime.Movement;

namespace Catalyst.Cards.Runtime.Discard
{
    public readonly struct ManualDiscardResult
    {
        private ManualDiscardResult(
            bool succeeded,
            CardInstance discardedCard,
            ManualDiscardFailure failure,
            CardMovementFailure movementFailure
        )
        {
            Succeeded = succeeded;
            DiscardedCard = discardedCard;
            Failure = failure;
            MovementFailure = movementFailure;
        }

        public bool Succeeded { get; }

        public CardInstance DiscardedCard { get; }

        public ManualDiscardFailure Failure { get; }

        public CardMovementFailure MovementFailure { get; }

        public static ManualDiscardResult Success(
            CardInstance discardedCard
        )
        {
            return new ManualDiscardResult(
                true,
                discardedCard,
                ManualDiscardFailure.None,
                CardMovementFailure.None
            );
        }

        public static ManualDiscardResult Fail(
            ManualDiscardFailure failure,
            CardMovementFailure movementFailure =
                CardMovementFailure.None
        )
        {
            return new ManualDiscardResult(
                false,
                null,
                failure,
                movementFailure
            );
        }
    }
}