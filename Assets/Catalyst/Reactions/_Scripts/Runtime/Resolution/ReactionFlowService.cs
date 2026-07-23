using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Reactions.Definitions;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionFlowService
    {
        private readonly ReactionResolutionPlanner planner;
        private readonly ReactionExecutionService executionService;
        private readonly ReadOnlyCollection
     <ReactionDefinition> availableReactions;

        public IReadOnlyList<ReactionDefinition> AvailableReactions => availableReactions;
        public ReactionFlowService(
    ReactionResolutionPlanner planner,
    ReactionExecutionService executionService,
    IEnumerable<ReactionDefinition> availableReactions
)
        {
            this.planner = planner
                ?? throw new ArgumentNullException(
                    nameof(planner)
                );

            this.executionService = executionService
                ?? throw new ArgumentNullException(
                    nameof(executionService)
                );

            this.availableReactions =
                CopyAvailableReactions(
                    availableReactions
                );
        }

        public ReactionFlowResult TryResolve(
            GameSession session,
            ReactionDefinition reaction
        )
        {
            if (session == null)
            {
                return ReactionFlowResult.Fail(
                    ReactionResolutionFailure
                        .SessionIsNull
                );
            }

            if (!ContainsReaction(reaction))
            {
                return ReactionFlowResult.Fail(
                    ReactionResolutionFailure
                        .ReactionUnavailable
                );
            }

            if (!ContainsReaction(reaction))
            {
                return ReactionFlowResult.Fail(
                    ReactionResolutionFailure
                        .ReactionUnavailable
                );
            }

            ReactionResolutionPlanResult planResult =
                planner.TryCreatePlan(
                    reaction,
                    session.ReactionTable.Cards
                );

            if (!planResult.Succeeded)
            {
                return ReactionFlowResult.Fail(
                    planResult.Failure
                );
            }

            ReactionExecutionResult executionResult =
                executionService.Execute(
                    session,
                    planResult.Plan
                );

            if (!executionResult.Succeeded)
            {
                return ReactionFlowResult.Fail(
                    executionResult.Failure
                );
            }

            return ReactionFlowResult.Success(
                executionResult.CreatedProducts
            );
        }

        #region helpers
        private bool ContainsReaction(
    ReactionDefinition reaction
)
        {
            if (reaction == null)
            {
                return false;
            }

            foreach (
                ReactionDefinition availableReaction
                in AvailableReactions
            )
            {
                if (ReferenceEquals(
                    availableReaction,
                    reaction
                ))
                {
                    return true;
                }
            }

            return false;
        }

        private static ReadOnlyCollection<ReactionDefinition> CopyAvailableReactions(
        IEnumerable<ReactionDefinition> source
    )
        {
            if (source == null)
            {
                throw new ArgumentNullException(
                    nameof(source)
                );
            }

            var result =
                new List<ReactionDefinition>();

            foreach (
                ReactionDefinition reaction
                in source
            )
            {
                if (reaction == null)
                {
                    throw new ArgumentException(
                        "Available reaction collection cannot contain null entries.",
                        nameof(source)
                    );
                }

                result.Add(reaction);
            }

            return result.AsReadOnly();
        }

        #endregion
    }
}