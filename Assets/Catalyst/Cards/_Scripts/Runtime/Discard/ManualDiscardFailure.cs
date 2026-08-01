namespace Catalyst.Cards.Runtime.Discard
{
    public enum ManualDiscardFailure
    {
        None = 0,
        NullCard = 1,
        NullSource = 2,
        NullDiscardPile = 3,
        CardNotInSource = 4,
        MovementFailed = 5,
        UnsupportedSource = 6
    }
}