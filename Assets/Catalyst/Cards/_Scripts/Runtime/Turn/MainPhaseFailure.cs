namespace Catalyst.Cards.Runtime.Turn
{
    public enum MainPhaseEndFailure
    {
        None = 0,
        NullTurn = 1,
        NullHand = 2,
        TurnNotStarted = 3,
        NotInMainPhase = 4,
        HandFull = 5
    }
}