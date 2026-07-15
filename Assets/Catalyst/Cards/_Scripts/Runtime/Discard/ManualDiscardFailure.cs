namespace Catalyst.Cards.Runtime.Discard
{
    public enum ManualDiscardFailure
    {
        None = 0,
        NullCard = 1,
        NullHand = 2,
        NullDiscardPile = 3,
        CardNotInHand = 4,
        MovementFailed = 5
    }
}