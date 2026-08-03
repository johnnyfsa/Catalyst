namespace Catalyst.Cards.Runtime.Delivery
{
    public enum CardDeliveryFailure
    {
        None = 0,
        NullCard = 1,
        NullSource = 2,
        NullDeliveryZone = 3,
        CardNotInSource = 4,
        DeliveryZoneRejectedCard = 5,
        MovementFailed = 6,
        DeliveryZoneDoesNotBelongToSession = 7,
        UnsupportedSource = 8
    }
}