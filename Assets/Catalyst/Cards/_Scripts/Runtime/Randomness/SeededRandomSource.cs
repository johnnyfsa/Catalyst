using System;

namespace Catalyst.Cards.Runtime.Randomness
{
    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random random;

        public SeededRandomSource(int seed)
        {
            random = new Random(seed);
        }

        public int Next(
            int minInclusive,
            int maxExclusive
        )
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive),
                    maxExclusive,
                    "Maximum value must be greater than minimum value."
                );
            }

            return random.Next(
                minInclusive,
                maxExclusive
            );
        }
    }
}