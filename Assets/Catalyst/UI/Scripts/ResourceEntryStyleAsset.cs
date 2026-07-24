using UnityEngine;

namespace Catalyst.UI.Definitions
{
    [CreateAssetMenu(
        fileName = "ResourceEntryStyle",
        menuName = "Catalyst/UI/Resource Entry Style"
    )]
    public sealed class ResourceEntryStyleAsset : ScriptableObject
    {
        [Header("Content")]
        [SerializeField]
        private string displayName;

        [SerializeField]
        private Sprite icon;

        [Header("Colors")]
        [SerializeField]
        private Color outlineColor = Color.white;

        [SerializeField]
        private Color nameTextColor = Color.white;

        [SerializeField]
        private Color amountTextColor = Color.white;

        [SerializeField]
        private Color iconColor = Color.white;

        public string DisplayName => displayName;

        public Sprite Icon => icon;

        public Color OutlineColor => outlineColor;

        public Color NameTextColor => nameTextColor;

        public Color AmountTextColor => amountTextColor;

        public Color IconColor => iconColor;
    }
}