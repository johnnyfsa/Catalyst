using System;
using Catalyst.Reactions.Runtime;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public readonly struct ReactionResolutionPlanResult
    {
        private ReactionResolutionPlanResult(
            ReactionResolutionPlan plan,
            ReactionResolutionFailure failure,
            ReactionDefinitionValidationFailure definitionFailure
        )
        {
            Plan = plan;
            Failure = failure;
            DefinitionFailure = definitionFailure;
        }

        public bool Succeeded =>
            Failure == ReactionResolutionFailure.None;

        public ReactionResolutionPlan Plan { get; }

        public ReactionResolutionFailure Failure { get; }

        public ReactionDefinitionValidationFailure DefinitionFailure
        {
            get;
        }

        public static ReactionResolutionPlanResult Success(
            ReactionResolutionPlan plan
        )
        {
            if (plan == null)
            {
                throw new ArgumentNullException(
                    nameof(plan)
                );
            }

            return new ReactionResolutionPlanResult(
                plan,
                ReactionResolutionFailure.None,
                ReactionDefinitionValidationFailure.None
            );
        }

        public static ReactionResolutionPlanResult Fail(
            ReactionResolutionFailure failure
        )
        {
            if (failure == ReactionResolutionFailure.None)
            {
                throw new ArgumentException(
                    "A failed plan must have a failure reason.",
                    nameof(failure)
                );
            }

            if (
                failure
                == ReactionResolutionFailure
                    .ReactionDefinitionIsInvalid
            )
            {
                throw new ArgumentException(
                    "Use InvalidDefinition for invalid definitions.",
                    nameof(failure)
                );
            }

            return new ReactionResolutionPlanResult(
                null,
                failure,
                ReactionDefinitionValidationFailure.None
            );
        }

        public static ReactionResolutionPlanResult
            InvalidDefinition(
                ReactionDefinitionValidationFailure
                    definitionFailure
            )
        {
            if (
                definitionFailure
                == ReactionDefinitionValidationFailure.None
            )
            {
                throw new ArgumentException(
                    "An invalid definition must preserve its validation failure.",
                    nameof(definitionFailure)
                );
            }

            return new ReactionResolutionPlanResult(
                null,
                ReactionResolutionFailure
                    .ReactionDefinitionIsInvalid,
                definitionFailure
            );
        }
    }
}