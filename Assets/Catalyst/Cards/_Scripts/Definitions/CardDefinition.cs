using System.Collections.Generic;
using UnityEngine;

namespace Catalyst.Cards.Definitions
{
    [CreateAssetMenu(
        fileName = "CardDefinition",
        menuName = "Catalyst/Cards/Card Definition"
    )]
    public sealed class CardDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private CardType cardType;

        [Header("Chemical presentation")]
        [Tooltip("TextMesh Pro rich text is supported, such as H<sub>2</sub>O.")]
        [SerializeField] private string formula;

        [SerializeField] private Sprite artwork;

        [Header("Tags")]
        [SerializeField]
        private List<CardTagDefinition> tags =
            new List<CardTagDefinition>();

        public string Id => id;
        public string DisplayName => displayName;
        public CardType CardType => cardType;
        public string Formula => formula;
        public Sprite Artwork => artwork;
        public IReadOnlyList<CardTagDefinition> Tags => tags;
    }
}