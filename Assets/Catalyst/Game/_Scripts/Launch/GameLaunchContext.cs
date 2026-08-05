using System;

namespace Catalyst.Game.Launch
{
    public static class GameLaunchContext
    {
        private static GameLaunchRequest?
            pendingRequest;

        public static bool HasPendingRequest =>
            pendingRequest.HasValue;

        public static void Prepare(
            GameLaunchRequest request
        )
        {
            pendingRequest = request;
        }

        public static bool TryConsume(
            out GameLaunchRequest request
        )
        {
            if (!pendingRequest.HasValue)
            {
                request = default;
                return false;
            }

            request = pendingRequest.Value;
            pendingRequest = null;

            return true;
        }

        public static void Clear()
        {
            pendingRequest = null;
        }
    }
}