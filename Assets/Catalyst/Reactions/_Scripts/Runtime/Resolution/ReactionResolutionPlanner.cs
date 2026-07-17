using System;
using System.Collections.Generic;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Definitions;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionResolutionPlanner
    {
        private readonly ReactionMatcherService matcher;

        public ReactionResolutionPlanner(
            ReactionMatcherService matcher
        )
        {
            this.matcher = matcher
                ?? throw new ArgumentNullException(
                    nameof(matcher)
                );
        }

        public ReactionResolutionPlanResult TryCreatePlan(
            ReactionDefinition reaction,
            IEnumerable<CardInstance> tableCards
        )
        {
            if (reaction == null)
            {
                return ReactionResolutionPlanResult.Fail(
                    ReactionResolutionFailure.ReactionIsNull
                );
            }

            ReactionDefinitionValidationResult validation =
                ReactionDefinitionValidator.Validate(
                    reaction
                );

            if (!validation.IsValid)
            {
                return ReactionResolutionPlanResult
                    .InvalidDefinition(
                        validation.Failure
                    );
            }

            if (tableCards == null)
            {
                return ReactionResolutionPlanResult.Fail(
                    ReactionResolutionFailure
                        .TableDoesNotMatch
                );
            }

            CardInstance[] reactantSnapshot =
                CreateReactantSnapshot(
                    tableCards
                );

            ReactionMatchResult matchResult =
                matcher.Match(
                    reaction,
                    reactantSnapshot
                );

            if (!matchResult.Succeeded)
            {
                return ConvertMatchFailure(matchResult);
            }

            ReactionProductPlanEntry[] products =
                CreateProductSnapshot(
                    reaction.Products
                );

            var plan =
                new ReactionResolutionPlan(
                    reactantSnapshot,
                    products,
                    reaction.RequiredHeat,
                    reaction.ProducedHeat,
                    reaction.RequiredElectricity,
                    reaction.ProducedElectricity
                );

            return ReactionResolutionPlanResult
                .Success(plan);
        }

        private static CardInstance[] CreateReactantSnapshot(
            IEnumerable<CardInstance> cards
        )
        {
            var snapshot =
                new List<CardInstance>();

            foreach (CardInstance card in cards)
            {
                snapshot.Add(card);
            }

            return snapshot.ToArray();
        }

        private static ReactionProductPlanEntry[]
            CreateProductSnapshot(
                IReadOnlyList<ReactionCardAmount> products
            )
        {
            var snapshot =
                new ReactionProductPlanEntry[
                    products.Count
                ];

            for (int index = 0;
                 index < products.Count;
                 index++)
            {
                ReactionCardAmount product =
                    products[index];

                snapshot[index] =
                    new ReactionProductPlanEntry(
                        product.CardDefinition,
                        product.Quantity
                    );
            }

            return snapshot;
        }

        private static ReactionResolutionPlanResult
            ConvertMatchFailure(
                ReactionMatchResult matchResult
            )
        {
            if (
                matchResult.Failure
                == ReactionMatchFailure
                    .ReactionDefinitionIsInvalid
            )
            {
                return ReactionResolutionPlanResult
                    .InvalidDefinition(
                        matchResult.DefinitionFailure
                    );
            }

            return ReactionResolutionPlanResult.Fail(
                ReactionResolutionFailure
                    .TableDoesNotMatch
            );
        }
    }
}