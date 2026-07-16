using System;
using Catalyst.Cards.Definitions;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionProductPlanEntry
    {
        public ReactionProductPlanEntry(
            CardDefinition definition,
            int quantity
        )
        {
            Definition = definition
                ?? throw new ArgumentNullException(
                    nameof(definition)
                );

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "Product quantity must be positive."
                );
            }

            Quantity = quantity;
        }

        public CardDefinition Definition { get; }

        public int Quantity { get; }
    }
}