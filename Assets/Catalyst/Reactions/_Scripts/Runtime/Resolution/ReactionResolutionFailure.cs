namespace Catalyst.Reactions.Runtime.Resolution
{
    public enum ReactionResolutionFailure
    {
        None = 0,

        SessionIsNull,
        ReactionIsNull,
        ReactionDefinitionIsInvalid,
        TableDoesNotMatch,
        InsufficientHeat,
        InsufficientProductCapacity
    }
}