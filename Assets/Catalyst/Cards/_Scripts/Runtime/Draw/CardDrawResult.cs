using Catalyst.Cards.Runtime.Movement;

namespace Catalyst.Cards.Runtime.Drawing
{
    public readonly struct CardDrawResult
    {
        private CardDrawResult(
            bool succeeded,
            CardInstance drawnCard,
            CardDrawFailure failure,
            CardMovementFailure movementFailure
        )
        {
            Succeeded = succeeded;
            DrawnCard = drawnCard;
            Failure = failure;
            MovementFailure = movementFailure;
        }

        public bool Succeeded { get; }

        public CardInstance DrawnCard { get; }

        public CardDrawFailure Failure { get; }

        public CardMovementFailure MovementFailure { get; }

        public static CardDrawResult Success(
            CardInstance drawnCard
        )
        {
            return new CardDrawResult(
                true,
                drawnCard,
                CardDrawFailure.None,
                CardMovementFailure.None
            );
        }

        public static CardDrawResult Fail(
            CardDrawFailure failure,
            CardMovementFailure movementFailure =
                CardMovementFailure.None
        )
        {
            return new CardDrawResult(
                false,
                null,
                failure,
                movementFailure
            );
        }
    }
}