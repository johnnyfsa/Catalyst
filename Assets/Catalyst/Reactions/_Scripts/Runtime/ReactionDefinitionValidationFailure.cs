namespace Catalyst.Reactions.Runtime
{
    public enum ReactionDefinitionValidationFailure
    {
        None = 0,

        DefinitionIsNull,
        ReactionIdIsEmpty,

        ReactantCollectionIsNull,
        ProductCollectionIsNull,

        NoReactants,
        NoProducts,

        ReactantEntryIsNull,
        ProductEntryIsNull,

        ReactantDefinitionIsNull,
        ProductDefinitionIsNull,

        ReactantQuantityIsNotPositive,
        ProductQuantityIsNotPositive,

        DuplicateReactantDefinition,
        DuplicateProductDefinition,

        RequiredHeatIsNegative,
        ProducedHeatIsNegative,

        RequiredElectricityIsNegative,
        ProducedElectricityIsNegative
    }
}