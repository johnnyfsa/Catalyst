namespace Catalyst.Cards.Runtime.Movement
{
    public enum CardMovementFailure
    {
        None = 0,
        NullCard = 1,
        NullSource = 2,
        NullDestination = 3,
        SameZone = 4,
        CardNotInSource = 5,
        SourceDoesNotAllowRemoval = 6,
        DestinationAlreadyContainsCard = 7,
        DestinationCannotReceiveCard = 8,
        DestinationRejectedCard = 9
    }
}