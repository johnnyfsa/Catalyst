using System;

namespace Catalyst.Game.Launch
{
    public static class SessionSeedGenerator
    {
        public static int GenerateDifferentFrom(
            int previousSeed
        )
        {
            int generatedSeed =
                Guid.NewGuid().GetHashCode();

            if (generatedSeed == previousSeed)
            {
                generatedSeed =
                    unchecked(previousSeed + 1);
            }

            return generatedSeed;
        }
    }
}