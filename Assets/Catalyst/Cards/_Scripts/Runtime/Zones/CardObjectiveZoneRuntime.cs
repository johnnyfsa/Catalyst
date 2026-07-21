using System;
using Catalyst.Cards.Definitions;

namespace Catalyst.Cards.Runtime.Zones
{
    public sealed class CardObjectiveZoneRuntime
        : CardZoneRuntime
    {
        public CardObjectiveZoneRuntime(
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

        public int CurrentAmount => Count;

        public bool IsCompleted =>
            CurrentAmount >= RequiredAmount;

        internal override bool CanAdd(
            CardInstance card
        )
        {
            return card != null
                && ReferenceEquals(
                    card.Definition,
                    AcceptedDefinition
                )
                && base.CanAdd(card);
        }

        protected override bool AllowsRemoval => false;
    }
}