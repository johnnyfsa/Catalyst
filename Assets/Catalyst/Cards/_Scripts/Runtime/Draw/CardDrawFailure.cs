namespace Catalyst.Cards.Runtime.Drawing
{
    public enum CardDrawFailure
    {
        None = 0,
        NullDeck = 1,
        NullHand = 2,
        DeckEmpty = 3,
        HandFull = 4,
        MovementFailed = 5
    }
}