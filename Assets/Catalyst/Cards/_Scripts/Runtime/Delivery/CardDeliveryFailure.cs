namespace Catalyst.Cards.Runtime.Delivery
{
    public enum CardDeliveryFailure
    {
        None = 0,
        NullCard = 1,
        NullHand = 2,
        NullDeliveryZone = 3,
        CardNotInHand = 4,
        DeliveryZoneRejectedCard = 5,
        MovementFailed = 6
    }
}