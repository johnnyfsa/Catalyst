namespace Catalyst.UI.Presentation.GameResult
{
    public sealed class GameResultSummary
    {
        public GameResultSummary(
            string objectiveLabel,
            string objectiveValue,
            string sessionLabel,
            string sessionValue
        )
        {
            ObjectiveLabel =
                objectiveLabel ?? string.Empty;

            ObjectiveValue =
                objectiveValue ?? string.Empty;

            SessionLabel =
                sessionLabel ?? string.Empty;

            SessionValue =
                sessionValue ?? string.Empty;
        }

        public string ObjectiveLabel { get; }

        public string ObjectiveValue { get; }

        public string SessionLabel { get; }

        public string SessionValue { get; }
    }
}