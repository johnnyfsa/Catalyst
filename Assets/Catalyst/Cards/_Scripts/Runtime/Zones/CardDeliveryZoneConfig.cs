using System;
using Catalyst.Cards.Definitions;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class CardDeliveryZoneConfig
    {
        public CardDeliveryZoneConfig(
            CardDefinition acceptedDefinition,
            int requiredAmount
        )
        {
            AcceptedDefinition = acceptedDefinition
                ?? throw new ArgumentNullException(
                    nameof(acceptedDefinition)
                );

            if (requiredAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredAmount),
                    requiredAmount,
                    "Required amount must be greater than zero."
                );
            }

            RequiredAmount = requiredAmount;
        }

        public CardDefinition AcceptedDefinition { get; }

        public int RequiredAmount { get; }
    }
}