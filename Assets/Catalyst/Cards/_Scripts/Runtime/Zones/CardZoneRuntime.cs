using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Catalyst.Cards.Runtime.Zones
{
    public abstract class CardZoneRuntime
    {
        private readonly List<CardInstance> cards;
        private readonly ReadOnlyCollection<CardInstance> readOnlyCards;

        protected CardZoneRuntime()
        {
            cards = new List<CardInstance>();
            readOnlyCards = cards.AsReadOnly();
        }

        public IReadOnlyList<CardInstance> Cards => readOnlyCards;
        public int Count => cards.Count;
        public bool IsEmpty => cards.Count == 0;

        public bool Contains(CardInstance card)
        {
            if (card == null)
            {
                return false;
            }

            return FindIndex(card.InstanceId) >= 0;
        }

        internal virtual bool CanAdd(CardInstance card)
        {
            return card != null && !Contains(card);
        }

        internal bool CanRemove(CardInstance card)
        {
            return card != null
                && AllowsRemoval
                && Contains(card);
        }

        internal bool TryAdd(CardInstance card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (!CanAdd(card))
            {
                return false;
            }

            cards.Add(card);
            return true;
        }

        internal bool TryRemove(CardInstance card)
        {
            return TryRemove(card, out _);
        }

        internal bool TryRemove(
            CardInstance card,
            out int removedIndex
        )
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            removedIndex = -1;

            if (!AllowsRemoval)
            {
                return false;
            }

            int index = FindIndex(card.InstanceId);

            if (index < 0)
            {
                return false;
            }

            cards.RemoveAt(index);
            removedIndex = index;

            return true;
        }

        internal bool TryInsertAt(
            CardInstance card,
            int index
        )
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (!CanAdd(card))
            {
                return false;
            }

            if (index < 0 || index > cards.Count)
            {
                return false;
            }

            cards.Insert(index, card);
            return true;
        }

        protected virtual bool AllowsRemoval => true;

        protected CardInstance GetCardAt(int index)
        {
            return cards[index];
        }

        private int FindIndex(Guid instanceId)
        {
            for (int index = 0; index < cards.Count; index++)
            {
                if (cards[index].InstanceId == instanceId)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}