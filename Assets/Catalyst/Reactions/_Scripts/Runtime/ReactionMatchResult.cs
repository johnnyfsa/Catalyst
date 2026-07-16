namespace Catalyst.Reactions.Runtime
{
    public readonly struct ReactionMatchResult
    {
        private ReactionMatchResult(
            ReactionMatchFailure failure,
            ReactionDefinitionValidationFailure definitionFailure)
        {
            Failure = failure;
            DefinitionFailure = definitionFailure;
        }

        public bool Succeeded =>
            Failure == ReactionMatchFailure.None;

        public ReactionMatchFailure Failure { get; }

        public ReactionDefinitionValidationFailure DefinitionFailure
        {
            get;
        }

        public static ReactionMatchResult Match()
        {
            return new ReactionMatchResult(
                ReactionMatchFailure.None,
                ReactionDefinitionValidationFailure.None);
        }

        public static ReactionMatchResult Fail(
            ReactionMatchFailure failure)
        {
            return new ReactionMatchResult(
                failure,
                ReactionDefinitionValidationFailure.None);
        }

        public static ReactionMatchResult InvalidDefinition(
            ReactionDefinitionValidationFailure definitionFailure)
        {
            return new ReactionMatchResult(
                ReactionMatchFailure.ReactionDefinitionIsInvalid,
                definitionFailure);
        }
    }
}