using System;

namespace Catalyst.Game.Launch
{
    public readonly struct GameLaunchRequest
    {
        public GameLaunchRequest(
            int seed,
            StageEntryMode entryMode
        )
        {
            if (!Enum.IsDefined(
                typeof(StageEntryMode),
                entryMode
            ))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(entryMode),
                    entryMode,
                    "Unsupported stage entry mode."
                );
            }

            Seed = seed;
            EntryMode = entryMode;
        }

        public int Seed { get; }

        public StageEntryMode EntryMode { get; }
    }
}