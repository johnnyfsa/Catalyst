namespace Catalyst.Reactions.Runtime
{
    public enum ReactionMatchFailure
    {
        None = 0,

        ReactionIsNull,
        ReactionDefinitionIsInvalid,
        CardCollectionIsNull,
        TableContainsNullCard,
        TableContainsCardWithoutDefinition,
        CompositionDoesNotMatch
    }
}