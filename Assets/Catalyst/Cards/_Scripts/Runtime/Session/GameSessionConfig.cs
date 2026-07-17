using System;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSessionConfig
    {
        public GameSessionConfig(
            int initialHandSize,
            int maxHandSize,
            int initialHeat = 0,
            int initialElectricity = 0
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

            if (initialHeat < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialHeat),
                    initialHeat,
                    "Initial heat cannot be negative."
                );
            }

            if (initialElectricity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialElectricity),
                    initialElectricity,
                    "Initial electricity cannot be negative."
                );
            }

            InitialHandSize = initialHandSize;
            MaxHandSize = maxHandSize;
            InitialHeat = initialHeat;
            InitialElectricity = initialElectricity;
        }

        public int InitialHandSize { get; }

        public int MaxHandSize { get; }

        public int InitialHeat { get; }

        public int InitialElectricity { get; }
    }
}