namespace Catalyst.Cards.Runtime.Randomness
{
    public interface IRandomSource
    {
        int Next(int minInclusive, int maxExclusive);
    }
}