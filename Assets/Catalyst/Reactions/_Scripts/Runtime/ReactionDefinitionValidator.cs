using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Reactions.Definitions;

namespace Catalyst.Reactions.Runtime
{
    public static class ReactionDefinitionValidator
    {
        public static ReactionDefinitionValidationResult Validate(
            ReactionDefinition definition)
        {
            if (definition == null)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure.DefinitionIsNull);
            }

            if (string.IsNullOrWhiteSpace(definition.ReactionId))
            {
                return Invalid(
                    ReactionDefinitionValidationFailure.ReactionIdIsEmpty);
            }

            if (definition.Reactants == null)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure
                        .ReactantCollectionIsNull);
            }

            if (definition.Products == null)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure
                        .ProductCollectionIsNull);
            }

            if (definition.Reactants.Count == 0)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure.NoReactants);
            }

            if (definition.Products.Count == 0)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure.NoProducts);
            }

            ReactionDefinitionValidationResult reactantResult =
                ValidateEntries(
                    definition.Reactants,
                    ReactionDefinitionValidationFailure.ReactantEntryIsNull,
                    ReactionDefinitionValidationFailure
                        .ReactantDefinitionIsNull,
                    ReactionDefinitionValidationFailure
                        .ReactantQuantityIsNotPositive,
                    ReactionDefinitionValidationFailure
                        .DuplicateReactantDefinition);

            if (!reactantResult.IsValid)
            {
                return reactantResult;
            }

            ReactionDefinitionValidationResult productResult =
                ValidateEntries(
                    definition.Products,
                    ReactionDefinitionValidationFailure.ProductEntryIsNull,
                    ReactionDefinitionValidationFailure
                        .ProductDefinitionIsNull,
                    ReactionDefinitionValidationFailure
                        .ProductQuantityIsNotPositive,
                    ReactionDefinitionValidationFailure
                        .DuplicateProductDefinition);

            if (!productResult.IsValid)
            {
                return productResult;
            }

            if (definition.RequiredHeat < 0)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure
                        .RequiredHeatIsNegative);
            }

            if (definition.RequiredElectricity < 0)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure
                        .RequiredElectricityIsNegative);
            }

            if (definition.ProducedHeat < 0)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure
                        .ProducedHeatIsNegative);
            }

            if (definition.ProducedElectricity < 0)
            {
                return Invalid(
                    ReactionDefinitionValidationFailure
                        .ProducedElectricityIsNegative);
            }

            return ReactionDefinitionValidationResult.Valid();
        }

        private static ReactionDefinitionValidationResult ValidateEntries(
            IReadOnlyList<ReactionCardAmount> entries,
            ReactionDefinitionValidationFailure nullEntryFailure,
            ReactionDefinitionValidationFailure nullDefinitionFailure,
            ReactionDefinitionValidationFailure quantityFailure,
            ReactionDefinitionValidationFailure duplicateFailure)
        {
            var definitions = new HashSet<CardDefinition>();

            foreach (ReactionCardAmount entry in entries)
            {
                if (entry == null)
                {
                    return Invalid(nullEntryFailure);
                }

                if (entry.CardDefinition == null)
                {
                    return Invalid(nullDefinitionFailure);
                }

                if (entry.Quantity <= 0)
                {
                    return Invalid(quantityFailure);
                }

                if (!definitions.Add(entry.CardDefinition))
                {
                    return Invalid(duplicateFailure);
                }
            }

            return ReactionDefinitionValidationResult.Valid();
        }

        private static ReactionDefinitionValidationResult Invalid(
            ReactionDefinitionValidationFailure failure)
        {
            return ReactionDefinitionValidationResult.Invalid(failure);
        }
    }
}