using System;
using Catalyst.Cards.Definitions;

namespace Catalyst.Cards.Runtime.Creation
{
    public sealed class CardInstanceFactory
    {
        private readonly ICardInstanceIdSource idSource;

        public CardInstanceFactory(
            ICardInstanceIdSource idSource
        )
        {
            this.idSource = idSource
                ?? throw new ArgumentNullException(
                    nameof(idSource)
                );
        }

        public CardInstance Create(
            CardDefinition definition
        )
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            Guid instanceId = idSource.NextId();

            if (instanceId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "The card instance ID source returned an empty identifier."
                );
            }

            return new CardInstance(
                instanceId,
                definition
            );
        }
    }
}