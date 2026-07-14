using System;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSessionConfig
    {
        public GameSessionConfig(
            int initialHandSize,
            int maxHandSize
        )
        {
            if (initialHandSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialHandSize),
                    initialHandSize,
                    "Initial hand size must be greater than zero."
                );
            }

            if (maxHandSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHandSize),
                    maxHandSize,
                    "Maximum hand size must be greater than zero."
                );
            }

            InitialHandSize = initialHandSize;
            MaxHandSize = maxHandSize;
        }

        public int InitialHandSize { get; }

        public int MaxHandSize { get; }
    }
}