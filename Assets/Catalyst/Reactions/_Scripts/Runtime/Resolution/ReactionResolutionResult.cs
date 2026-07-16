using System;
using Catalyst.Reactions.Runtime;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public readonly struct ReactionResolutionResult
    {
        private ReactionResolutionResult(
            ReactionResolutionFailure failure,
            ReactionDefinitionValidationFailure definitionFailure
        )
        {
            Failure = failure;
            DefinitionFailure = definitionFailure;
        }

        public bool Succeeded =>
            Failure == ReactionResolutionFailure.None;

        public ReactionResolutionFailure Failure { get; }

        public ReactionDefinitionValidationFailure DefinitionFailure
        {
            get;
        }

        public static ReactionResolutionResult Success()
        {
            return new ReactionResolutionResult(
                ReactionResolutionFailure.None,
                ReactionDefinitionValidationFailure.None
            );
        }

        public static ReactionResolutionResult Fail(
            ReactionResolutionFailure failure
        )
        {
            if (failure == ReactionResolutionFailure.None)
            {
                throw new ArgumentException(
                    "A failed reaction resolution must have a failure reason.",
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
                    "Use InvalidDefinition when the reaction definition is invalid.",
                    nameof(failure)
                );
            }

            return new ReactionResolutionResult(
                failure,
                ReactionDefinitionValidationFailure.None
            );
        }

        public static ReactionResolutionResult InvalidDefinition(
            ReactionDefinitionValidationFailure definitionFailure
        )
        {
            if (
                definitionFailure
                == ReactionDefinitionValidationFailure.None
            )
            {
                throw new ArgumentException(
                    "An invalid reaction definition must have a validation failure.",
                    nameof(definitionFailure)
                );
            }

            return new ReactionResolutionResult(
                ReactionResolutionFailure
                    .ReactionDefinitionIsInvalid,
                definitionFailure
            );
        }
    }
}