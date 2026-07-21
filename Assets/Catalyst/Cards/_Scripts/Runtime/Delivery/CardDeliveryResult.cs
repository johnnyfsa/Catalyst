using System;

namespace Catalyst.Cards.Runtime.Delivery
{
    public sealed class CardDeliveryResult
    {
        private CardDeliveryResult(
            bool succeeded,
            CardDeliveryFailure failure
        )
        {
            Succeeded = succeeded;
            Failure = failure;
        }

        public bool Succeeded { get; }

        public CardDeliveryFailure Failure { get; }

        public static CardDeliveryResult Success()
        {
            return new CardDeliveryResult(
                succeeded: true,
                CardDeliveryFailure.None
            );
        }

        public static CardDeliveryResult Fail(
            CardDeliveryFailure failure
        )
        {
            if (failure == CardDeliveryFailure.None)
            {
                throw new ArgumentException(
                    "A failed delivery must contain a failure.",
                    nameof(failure)
                );
            }

            return new CardDeliveryResult(
                succeeded: false,
                failure
            );
        }
    }
}