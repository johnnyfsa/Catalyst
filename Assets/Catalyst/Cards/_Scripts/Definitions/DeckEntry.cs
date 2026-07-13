using System;
using UnityEngine;

namespace Catalyst.Cards.Definitions
{
    [Serializable]
    public sealed class DeckEntry
    {
        [SerializeField]
        private CardDefinition cardDefinition;

        [SerializeField]
        [Min(1)]
        private int quantity = 1;

        public CardDefinition CardDefinition => cardDefinition;
        public int Quantity => quantity;

        public DeckEntry(CardDefinition cardDefinition, int quantity)
        {
            if (cardDefinition == null)
            {
                throw new ArgumentNullException(nameof(cardDefinition));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    quantity,
                    "Deck entry quantity must be greater than zero."
                );
            }

            this.cardDefinition = cardDefinition;
            this.quantity = quantity;
        }
    }
}