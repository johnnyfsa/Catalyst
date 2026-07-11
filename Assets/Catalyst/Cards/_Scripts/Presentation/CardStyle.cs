using System;
using UnityEngine;

namespace Catalyst.Cards.Presentation
{
    [Serializable]
    public sealed class CardStyle
    {
        [Header("Card")]
        [SerializeField] private Color frameColor = Color.white;
        [SerializeField] private Color backgroundColor = Color.white;

        [Header("Title")]
        [SerializeField] private Color titleTextColor = Color.black;
        [SerializeField] private Color titleTextBackgroundColor = Color.white;

        [Header("Formula")]
        [SerializeField] private Color formulaTextColor = Color.black;
        [SerializeField] private Color formulaTextBackgroundColor = Color.white;

        [Header("Tags")]
        [SerializeField] private Color tagTextColor = Color.black;

        public Color FrameColor => frameColor;
        public Color BackgroundColor => backgroundColor;

        public Color TitleTextColor => titleTextColor;
        public Color TitleTextBackgroundColor => titleTextBackgroundColor;

        public Color FormulaTextColor => formulaTextColor;
        public Color FormulaTextBackgroundColor => formulaTextBackgroundColor;

        public Color TagTextColor => tagTextColor;
    }
}