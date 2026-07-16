using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Definitions;

namespace Catalyst.Reactions.Runtime
{
    public sealed class ReactionMatcherService
    {
        public ReactionMatchResult Match(
            ReactionDefinition reaction,
            IEnumerable<CardInstance> cards)
        {
            if (reaction == null)
            {
                return ReactionMatchResult.Fail(
                    ReactionMatchFailure.ReactionIsNull);
            }

            ReactionDefinitionValidationResult validation =
                ReactionDefinitionValidator.Validate(reaction);

            if (!validation.IsValid)
            {
                return ReactionMatchResult.InvalidDefinition(
                    validation.Failure);
            }

            if (cards == null)
            {
                return ReactionMatchResult.Fail(
                    ReactionMatchFailure.CardCollectionIsNull);
            }

            Dictionary<CardDefinition, int> actualComposition =
                BuildEmptyComposition(reaction);

            int actualCardCount = 0;

            foreach (CardInstance card in cards)
            {
                if (card == null)
                {
                    return ReactionMatchResult.Fail(
                        ReactionMatchFailure.TableContainsNullCard);
                }

                CardDefinition cardDefinition = card.Definition;

                if (cardDefinition == null)
                {
                    return ReactionMatchResult.Fail(
                        ReactionMatchFailure
                            .TableContainsCardWithoutDefinition);
                }

                actualCardCount++;

                if (!actualComposition.TryGetValue(
                        cardDefinition,
                        out int currentQuantity))
                {
                    return ReactionMatchResult.Fail(
                        ReactionMatchFailure.CompositionDoesNotMatch);
                }

                actualComposition[cardDefinition] =
                    currentQuantity + 1;
            }

            int expectedCardCount = 0;

            foreach (ReactionCardAmount reactant in reaction.Reactants)
            {
                expectedCardCount += reactant.Quantity;

                int actualQuantity =
                    actualComposition[reactant.CardDefinition];

                if (actualQuantity != reactant.Quantity)
                {
                    return ReactionMatchResult.Fail(
                        ReactionMatchFailure.CompositionDoesNotMatch);
                }
            }

            if (actualCardCount != expectedCardCount)
            {
                return ReactionMatchResult.Fail(
                    ReactionMatchFailure.CompositionDoesNotMatch);
            }

            return ReactionMatchResult.Match();
        }

        private static Dictionary<CardDefinition, int>
            BuildEmptyComposition(ReactionDefinition reaction)
        {
            var composition =
                new Dictionary<CardDefinition, int>();

            foreach (ReactionCardAmount reactant in reaction.Reactants)
            {
                composition.Add(
                    reactant.CardDefinition,
                    0);
            }

            return composition;
        }
    }
}