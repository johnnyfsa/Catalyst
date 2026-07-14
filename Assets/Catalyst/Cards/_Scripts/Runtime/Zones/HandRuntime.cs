namespace Catalyst.Cards.Runtime.Zones
{
    public sealed class HandRuntime : CardZoneRuntime
    {
        public const int DefaultCapacity = 8;

        public HandRuntime()
            : this(DefaultCapacity)
        {
        }

        public HandRuntime(int capacity)
        {
            if (capacity <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(capacity),
                    capacity,
                    "Hand capacity must be greater than zero."
                );
            }

            Capacity = capacity;
        }

        public int Capacity { get; }
        public bool IsFull => Count >= Capacity;

        internal override bool CanAdd(CardInstance card)
        {
            return !IsFull && base.CanAdd(card);
        }
    }
}