using System;
using Catalyst.Cards.Definitions;

namespace Catalyst.Cards.Runtime
{
    public sealed class CardInstance
    {
        public Guid InstanceId { get; }
        public CardDefinition Definition { get; }

        public CardInstance(
            Guid instanceId,
            CardDefinition definition
        )
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A card instance must have a non-empty identifier.",
                    nameof(instanceId)
                );
            }

            Definition = definition
                ?? throw new ArgumentNullException(nameof(definition));

            InstanceId = instanceId;
        }
    }
}