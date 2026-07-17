namespace Catalyst.Reactions.Runtime.Resolution
{
    public enum ReactionResolutionFailure
    {
        None = 0,

        SessionIsNull,
        PlanIsNull,

        ReactionIsNull,
        ReactionDefinitionIsInvalid,
        TableDoesNotMatch,

        DuplicateReactantInstance,
        ReactantDoesNotBelongToSession,
        ReactantInstanceDoesNotMatchSessionCard,
        ReactantIsNotOnReactionTable,

        InsufficientHeat,
        InsufficientProductCapacity
    }
}