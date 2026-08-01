namespace Catalyst.Cards.Runtime.Turn
{
    public readonly struct DrawPhaseResult
    {
        private DrawPhaseResult(
            DrawPhaseOutcome outcome,
            CardInstance drawnCard,
            int drawnCardCount
        )
        {
            if (drawnCardCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(drawnCardCount),
                    drawnCardCount,
                    "Drawn card count cannot be negative."
                );
            }

            Outcome = outcome;
            DrawnCard = drawnCard;
            DrawnCardCount = drawnCardCount;
        }

        public DrawPhaseOutcome Outcome { get; }

        /// <summary>
        /// Preserved for compatibility with the previous
        /// unit-draw result contract.
        ///
        /// Normal draw-phase resolution now terminates with
        /// HandFull or DeckOut, so this property will normally
        /// be null for DrawPhaseService results.
        /// </summary>
        public CardInstance DrawnCard { get; }

        public int DrawnCardCount { get; }

        public bool CanContinueTurn =>
            Outcome != DrawPhaseOutcome.DeckOut;

        public static DrawPhaseResult CardDrawn(
            CardInstance card
        )
        {
            if (card == null)
            {
                throw new System.ArgumentNullException(
                    nameof(card)
                );
            }

            return new DrawPhaseResult(
                DrawPhaseOutcome.CardDrawn,
                card,
                drawnCardCount: 1
            );
        }

        public static DrawPhaseResult HandFull(
            int drawnCardCount = 0
        )
        {
            return new DrawPhaseResult(
                DrawPhaseOutcome.HandFull,
                drawnCard: null,
                drawnCardCount
            );
        }

        public static DrawPhaseResult DeckOut(
            int drawnCardCount = 0
        )
        {
            return new DrawPhaseResult(
                DrawPhaseOutcome.DeckOut,
                drawnCard: null,
                drawnCardCount
            );
        }
    }
}