using System;
using Catalyst.Cards.Runtime.Creation;

namespace Catalyst.Tests.EditMode.Common.Doubles
{
    internal sealed class SequentialCardInstanceIdSource
        : ICardInstanceIdSource
    {
        private int nextValue = 1;

        public Guid NextId()
        {
            byte[] bytes = new byte[16];

            BitConverter
                .GetBytes(nextValue++)
                .CopyTo(bytes, 0);

            return new Guid(bytes);
        }
    }
}