using System;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Reactions.Definitions;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionFlowService
    {
        private readonly ReactionResolutionPlanner planner;
        private readonly ReactionExecutionService executionService;

        public ReactionFlowService(
            ReactionResolutionPlanner planner,
            ReactionExecutionService executionService
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
        }

        public ReactionFlowResult TryResolve(
            GameSession session,
            ReactionDefinition reaction
        )
        {
            if (session == null)
            {
                return ReactionFlowResult.Fail(
                    ReactionResolutionFailure.SessionIsNull
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
    }
}