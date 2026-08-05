using UnityEngine;

namespace Catalyst.UI.Definitions.SessionBriefing
{
    [CreateAssetMenu(
        fileName = "SessionBriefingDefinition",
        menuName =
            "Catalyst/UI/Session Briefing Definition"
    )]
    public sealed class SessionBriefingDefinition :
        ScriptableObject
    {
        [Header("Content")]
        [SerializeField]
        private string stageTitle;

        [SerializeField]
        [TextArea(4, 10)]
        private string briefingText;

        [Header("Visuals")]
        [SerializeField]
        private Sprite stageBackground;

        public string StageTitle =>
            stageTitle;

        public string BriefingText =>
            briefingText;

        public Sprite StageBackground =>
            stageBackground;
    }
}