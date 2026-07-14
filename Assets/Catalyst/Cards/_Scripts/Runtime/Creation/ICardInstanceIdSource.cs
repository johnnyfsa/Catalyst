using System;

namespace Catalyst.Cards.Runtime.Creation
{
    public interface ICardInstanceIdSource
    {
        Guid NextId();
    }
}