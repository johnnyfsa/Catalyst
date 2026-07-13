namespace Catalyst.Cards.Runtime.Zones
{
    public sealed class DeckRuntime : CardZoneRuntime
    {
        public bool TryPeekTop(out CardInstance card)
        {
            if (IsEmpty)
            {
                card = null;
                return false;
            }

            card = GetCardAt(Count - 1);
            return true;
        }
    }
}