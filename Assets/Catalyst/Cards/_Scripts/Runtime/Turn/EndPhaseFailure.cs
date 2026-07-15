namespace Catalyst.Cards.Runtime.Turn
{
    public enum EndPhaseFailure
    {
        None = 0,
        NullTurn = 1,
        TurnNotStarted = 2,
        NotInEndPhase = 3
    }
}