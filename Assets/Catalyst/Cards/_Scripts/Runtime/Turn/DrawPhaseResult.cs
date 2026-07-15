namespace Catalyst.Cards.Runtime.Turn
{
    public readonly struct DrawPhaseResult
    {
        private DrawPhaseResult(
            DrawPhaseOutcome outcome,
            CardInstance drawnCard
        )
        {
            Outcome = outcome;
            DrawnCard = drawnCard;
        }

        public DrawPhaseOutcome Outcome { get; }

        public CardInstance DrawnCard { get; }

        public bool CanContinueTurn =>
            Outcome != DrawPhaseOutcome.DeckOut;

        public static DrawPhaseResult CardDrawn(
            CardInstance card
        )
        {
            return new DrawPhaseResult(
                DrawPhaseOutcome.CardDrawn,
                card
            );
        }

        public static DrawPhaseResult HandFull()
        {
            return new DrawPhaseResult(
                DrawPhaseOutcome.HandFull,
                null
            );
        }

        public static DrawPhaseResult DeckOut()
        {
            return new DrawPhaseResult(
                DrawPhaseOutcome.DeckOut,
                null
            );
        }
    }
}