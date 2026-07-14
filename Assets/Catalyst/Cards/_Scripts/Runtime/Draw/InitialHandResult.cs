namespace Catalyst.Cards.Runtime.Drawing
{
    public readonly struct InitialHandResult
    {
        public InitialHandResult(
            int requestedCardCount,
            int targetCardCount,
            int drawnCardCount,
            bool stoppedBecauseDeckWasEmpty
        )
        {
            RequestedCardCount = requestedCardCount;
            TargetCardCount = targetCardCount;
            DrawnCardCount = drawnCardCount;
            StoppedBecauseDeckWasEmpty =
                stoppedBecauseDeckWasEmpty;
        }

        public int RequestedCardCount { get; }

        public int TargetCardCount { get; }

        public int DrawnCardCount { get; }

        public bool StoppedBecauseDeckWasEmpty { get; }

        public bool Completed =>
            DrawnCardCount == TargetCardCount;
    }
}