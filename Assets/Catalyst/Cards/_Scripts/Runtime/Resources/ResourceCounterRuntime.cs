using System;

namespace Catalyst.Cards.Runtime.Resources
{
    public sealed class ResourceCounterRuntime
    {
        public ResourceCounterRuntime(
            int initialAmount = 0
        )
        {
            if (initialAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialAmount),
                    initialAmount,
                    "Initial resource amount cannot be negative."
                );
            }

            Amount = initialAmount;
        }

        public int Amount { get; private set; }

        public bool CanConsume(int amount)
        {
            return amount >= 0
                && Amount >= amount;
        }

        internal bool TryConsume(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount)
                );
            }

            if (!CanConsume(amount))
            {
                return false;
            }

            Amount -= amount;
            return true;
        }

        internal void Add(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount)
                );
            }

            Amount = checked(Amount + amount);
        }
    }
}