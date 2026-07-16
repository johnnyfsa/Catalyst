namespace Catalyst.Reactions.Runtime
{
    public readonly struct ReactionDefinitionValidationResult
    {
        private ReactionDefinitionValidationResult(
            ReactionDefinitionValidationFailure failure)
        {
            Failure = failure;
        }

        public bool IsValid =>
            Failure == ReactionDefinitionValidationFailure.None;

        public ReactionDefinitionValidationFailure Failure { get; }

        public static ReactionDefinitionValidationResult Valid()
        {
            return new ReactionDefinitionValidationResult(
                ReactionDefinitionValidationFailure.None);
        }

        public static ReactionDefinitionValidationResult Invalid(
            ReactionDefinitionValidationFailure failure)
        {
            return new ReactionDefinitionValidationResult(failure);
        }
    }
}