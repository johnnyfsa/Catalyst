using System;

namespace Catalyst.Cards.Runtime.Creation
{
    public sealed class GuidCardInstanceIdSource
        : ICardInstanceIdSource
    {
        public Guid NextId()
        {
            return Guid.NewGuid();
        }
    }
}