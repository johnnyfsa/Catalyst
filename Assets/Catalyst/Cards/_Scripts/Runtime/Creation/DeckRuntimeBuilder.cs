using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Creation
{
    public sealed class DeckRuntimeBuilder
    {
        private readonly CardInstanceFactory instanceFactory;

        public DeckRuntimeBuilder(
            CardInstanceFactory instanceFactory
        )
        {
            this.instanceFactory = instanceFactory
                ?? throw new ArgumentNullException(
                    nameof(instanceFactory)
                );
        }

        public DeckRuntime Build(
            IEnumerable<DeckEntry> entries
        )
        {
            if (entries == null)
            {
                throw new ArgumentNullException(
                    nameof(entries)
                );
            }

            DeckRuntime deck = new DeckRuntime();
            HashSet<Guid> generatedIds = new HashSet<Guid>();

            foreach (DeckEntry entry in entries)
            {
                ValidateEntry(entry);

                for (int copyIndex = 0;
                     copyIndex < entry.Quantity;
                     copyIndex++)
                {
                    CardInstance card =
                        instanceFactory.Create(
                            entry.CardDefinition
                        );

                    if (!generatedIds.Add(card.InstanceId))
                    {
                        throw new InvalidOperationException(
                            $"The ID source generated the duplicate card instance ID '{card.InstanceId}'."
                        );
                    }

                    bool added = deck.TryAdd(card);

                    if (!added)
                    {
                        throw new InvalidOperationException(
                            $"The created card instance '{card.InstanceId}' could not be added to the deck."
                        );
                    }
                }
            }

            return deck;
        }

        private static void ValidateEntry(DeckEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentException(
                    "The deck entry collection cannot contain null entries.",
                    nameof(entry)
                );
            }

            if (entry.CardDefinition == null)
            {
                throw new ArgumentException(
                    "Every deck entry must reference a card definition.",
                    nameof(entry)
                );
            }

            if (entry.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Every deck entry must have a quantity greater than zero.",
                    nameof(entry)
                );
            }
        }
    }
}