using System;

namespace Catalyst.Cards.Runtime.Zones
{
    public sealed class ReactionTableRuntime :
        CardZoneRuntime
    {
        public const int DefaultCapacity = 12;

        public ReactionTableRuntime()
            : this(DefaultCapacity)
        {
        }

        public ReactionTableRuntime(
            int capacity
        )
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    capacity,
                    "Reaction table capacity must be greater than zero."
                );
            }

            Capacity = capacity;
        }

        public int Capacity { get; }

        public bool IsFull =>
            Count >= Capacity;

        internal override bool CanAdd(
            CardInstance card
        )
        {
            return !IsFull
                && base.CanAdd(card);
        }
    }
}