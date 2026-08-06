using System;

namespace Catalyst.Game.Launch
{
    public static class SessionSeedGenerator
    {
        public static int Generate()
        {
            return Guid.NewGuid().GetHashCode();
        }

        public static int GenerateDifferentFrom(
            int previousSeed
        )
        {
            int generatedSeed = Generate();

            if (generatedSeed == previousSeed)
            {
                generatedSeed =
                    unchecked(previousSeed + 1);
            }

            return generatedSeed;
        }
    }
}