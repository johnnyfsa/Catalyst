using System;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public readonly struct ReactionExecutionValidationResult
    {
        private ReactionExecutionValidationResult(
            ReactionResolutionFailure failure
        )
        {
            Failure = failure;
        }

        public bool IsValid =>
            Failure == ReactionResolutionFailure.None;

        public ReactionResolutionFailure Failure { get; }

        public static ReactionExecutionValidationResult Valid()
        {
            return new ReactionExecutionValidationResult(
                ReactionResolutionFailure.None
            );
        }

        public static ReactionExecutionValidationResult Invalid(
            ReactionResolutionFailure failure
        )
        {
            if (failure == ReactionResolutionFailure.None)
            {
                throw new ArgumentException(
                    "An invalid reaction execution must have a failure reason.",
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
                    "Reaction definition validation belongs to reaction planning.",
                    nameof(failure)
                );
            }

            if (
                failure
                == ReactionResolutionFailure.ReactionIsNull
                || failure
                == ReactionResolutionFailure.TableDoesNotMatch
            )
            {
                throw new ArgumentException(
                    "Reaction matching failures belong to reaction planning.",
                    nameof(failure)
                );
            }

            return new ReactionExecutionValidationResult(
                failure
            );
        }
    }
}