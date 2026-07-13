namespace Catalyst.Cards.Runtime.Zones
{
    public sealed class DiscardPileRuntime : CardZoneRuntime
    {
        protected override bool AllowsRemoval => false;
    }
}