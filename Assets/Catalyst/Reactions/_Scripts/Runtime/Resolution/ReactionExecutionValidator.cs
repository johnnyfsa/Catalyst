using System.Collections.Generic;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Session;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionExecutionValidator
    {
        public ReactionExecutionValidationResult Validate(
            GameSession session,
            ReactionResolutionPlan plan
        )
        {
            if (session == null)
            {
                return ReactionExecutionValidationResult.Invalid(
                    ReactionResolutionFailure.SessionIsNull
                );
            }

            if (plan == null)
            {
                return ReactionExecutionValidationResult.Invalid(
                    ReactionResolutionFailure.PlanIsNull
                );
            }

            ReactionExecutionValidationResult reactantValidation =
                ValidateReactants(
                    session,
                    plan
                );

            if (!reactantValidation.IsValid)
            {
                return reactantValidation;
            }

            ReactionExecutionValidationResult resourceValidation =
                ValidateResources(
                    session,
                    plan
                );

            if (!resourceValidation.IsValid)
            {
                return resourceValidation;
            }

            if (!HasProductCapacity(
                    session,
                    plan.TotalProductCount
                ))
            {
                return ReactionExecutionValidationResult.Invalid(
                    ReactionResolutionFailure
                        .InsufficientProductCapacity
                );
            }

            return ReactionExecutionValidationResult.Valid();
        }

        private static ReactionExecutionValidationResult
            ValidateReactants(
                GameSession session,
                ReactionResolutionPlan plan
            )
        {
            var reactantIds =
                new HashSet<System.Guid>();

            foreach (
                CardInstance reactant
                in plan.ConsumedReactants
            )
            {
                if (!reactantIds.Add(reactant.InstanceId))
                {
                    return ReactionExecutionValidationResult.Invalid(
                        ReactionResolutionFailure
                            .DuplicateReactantInstance
                    );
                }

                if (!session.TryGetCard(
                        reactant.InstanceId,
                        out CardInstance registeredCard
                    ))
                {
                    return ReactionExecutionValidationResult.Invalid(
                        ReactionResolutionFailure
                            .ReactantDoesNotBelongToSession
                    );
                }

                if (!ReferenceEquals(
                        reactant,
                        registeredCard
                    ))
                {
                    return ReactionExecutionValidationResult.Invalid(
                        ReactionResolutionFailure
                            .ReactantInstanceDoesNotMatchSessionCard
                    );
                }

                if (!session.ReactionTable.Contains(reactant))
                {
                    return ReactionExecutionValidationResult.Invalid(
                        ReactionResolutionFailure
                            .ReactantIsNotOnReactionTable
                    );
                }
            }

            return ReactionExecutionValidationResult.Valid();
        }

        private static ReactionExecutionValidationResult
            ValidateResources(
                GameSession session,
                ReactionResolutionPlan plan
            )
        {
            if (!session.Heat.CanConsume(
                    plan.RequiredHeat
                ))
            {
                return ReactionExecutionValidationResult.Invalid(
                    ReactionResolutionFailure
                        .InsufficientHeat
                );
            }

            if (!session.Electricity.CanConsume(
                    plan.RequiredElectricity
                ))
            {
                return ReactionExecutionValidationResult.Invalid(
                    ReactionResolutionFailure
                        .InsufficientElectricity
                );
            }

            return ReactionExecutionValidationResult.Valid();
        }

        private static bool HasProductCapacity(
            GameSession session,
            int productCount
        )
        {
            int remainingCapacity =
                session.Hand.Capacity
                - session.Hand.Count;

            return productCount <= remainingCapacity;
        }
    }
}