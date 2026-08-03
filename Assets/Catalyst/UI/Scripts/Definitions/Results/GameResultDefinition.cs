using Catalyst.Cards.Runtime.Session;
using UnityEngine;

namespace Catalyst.UI.Definitions.Results
{
    [CreateAssetMenu(
        fileName = "GameResultDefinition",
        menuName =
            "Catalyst/UI/Game Result Definition"
    )]
    public sealed class GameResultDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private GameSessionEndReason endReason;

        [SerializeField]
        private GameResultOutcome outcome;

        [Header("Content")]
        [SerializeField]
        private Sprite icon;

        [SerializeField]
        private string title;

        [SerializeField]
        [TextArea]
        private string message;

        public GameSessionEndReason EndReason =>
            endReason;

        public GameResultOutcome Outcome =>
            outcome;

        public Sprite Icon =>
            icon;

        public string Title =>
            title;

        public string Message =>
            message;
    }
}