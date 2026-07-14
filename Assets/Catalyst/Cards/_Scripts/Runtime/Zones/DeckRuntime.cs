using System;
using Catalyst.Cards.Runtime.Randomness;

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

        public void Shuffle(IRandomSource randomSource)
        {
            if (randomSource == null)
            {
                throw new ArgumentNullException(
                    nameof(randomSource)
                );
            }

            for (int currentIndex = Count - 1;
                 currentIndex > 0;
                 currentIndex--)
            {
                int swapIndex = randomSource.Next(
                    0,
                    currentIndex + 1
                );

                SwapCards(currentIndex, swapIndex);
            }
        }
    }
}