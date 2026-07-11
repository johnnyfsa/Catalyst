using UnityEngine;

namespace Catalyst.Cards.Definitions
{
    [CreateAssetMenu(
        fileName = "CardTag",
        menuName = "Catalyst/Cards/Card Tag"
    )]
    public sealed class CardTagDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [TextArea(2, 4)]
        [SerializeField] private string shortDescription;

        [TextArea(4, 10)]
        [SerializeField] private string fullDescription;

        [SerializeField] private Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public string ShortDescription => shortDescription;
        public string FullDescription => fullDescription;
        public Sprite Icon => icon;
    }
}