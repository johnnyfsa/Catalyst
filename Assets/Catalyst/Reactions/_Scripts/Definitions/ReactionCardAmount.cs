using System;
using Catalyst.Cards.Definitions;
using UnityEngine;

namespace Catalyst.Reactions.Definitions
{
    [Serializable]
    public sealed class ReactionCardAmount
    {
        [SerializeField]
        private CardDefinition cardDefinition;

        [SerializeField, Min(1)]
        private int quantity = 1;

        public CardDefinition CardDefinition => cardDefinition;

        public int Quantity => quantity;

        public ReactionCardAmount(
            CardDefinition cardDefinition,
            int quantity)
        {
            this.cardDefinition = cardDefinition;
            this.quantity = quantity;
        }
    }
}