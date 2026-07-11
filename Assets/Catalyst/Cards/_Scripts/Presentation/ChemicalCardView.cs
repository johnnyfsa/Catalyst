using System;
using System.Text;
using Catalyst.Cards.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.Cards.Presentation
{
    [ExecuteAlways]
    public sealed class ChemicalCardView : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CardStyleMap styleMap;

        [Tooltip(
    "Definition used for Editor preview and as a fallback " +
    "when the card starts without a runtime binding."
)]
        [SerializeField] private CardDefinition initialDefinition;

        [Header("Card Images")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image artworkImage;

        [Header("Title")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image titleBackgroundImage;

        [Header("Formula")]
        [SerializeField] private TMP_Text formulaText;
        [SerializeField] private Image formulaBackgroundImage;

        [Header("Tags")]
        [Tooltip("Temporary text-based tag presentation for the prototype.")]
        [SerializeField] private TMP_Text tagsText;

        private CardDefinition boundDefinition;

        public CardDefinition BoundDefinition => boundDefinition;

        private void Start()
        {
            if (boundDefinition != null)
            {
                Render(boundDefinition);
                return;
            }

            if (initialDefinition != null)
            {
                Render(initialDefinition);
            }
        }

        /// <summary>
        /// Associates a runtime card definition with this view.
        /// </summary>
        public void Bind(CardDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            boundDefinition = definition;
            Render(definition);
        }

        /// <summary>
        /// Removes the currently displayed runtime card.
        /// </summary>
        public void Clear()
        {
            boundDefinition = null;

            SetText(titleText, string.Empty);
            SetText(formulaText, string.Empty);
            SetText(tagsText, string.Empty);

            if (artworkImage != null)
            {
                artworkImage.sprite = null;
                artworkImage.enabled = false;
            }
        }

        [ContextMenu("Refresh Preview")]
        public void RefreshPreview()
        {
            if (Application.isPlaying || initialDefinition == null)
            {
                return;
            }

            Render(initialDefinition);
        }

        private void Render(CardDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            ApplyContent(definition);
            ApplyStyle(definition.CardType);
        }

        private void ApplyContent(CardDefinition definition)
        {
            SetText(titleText, definition.DisplayName);
            SetText(formulaText, definition.Formula);
            SetText(tagsText, BuildTagsText(definition));

            if (artworkImage != null)
            {
                artworkImage.sprite = definition.Artwork;
                artworkImage.enabled = definition.Artwork != null;
            }
        }

        private void ApplyStyle(CardType cardType)
        {
            if (styleMap == null)
            {
                Debug.LogWarning(
                    $"{nameof(ChemicalCardView)} on '{name}' has no " +
                    $"{nameof(CardStyleMap)} assigned.",
                    this
                );

                return;
            }

            if (!styleMap.TryGetStyle(cardType, out CardStyle style))
            {
                Debug.LogError(
                    $"No style was found for card type '{cardType}'.",
                    styleMap
                );

                return;
            }

            SetColor(frameImage, style.FrameColor);
            SetColor(backgroundImage, style.BackgroundColor);

            SetColor(titleText, style.TitleTextColor);
            SetColor(
                titleBackgroundImage,
                style.TitleTextBackgroundColor
            );

            SetColor(formulaText, style.FormulaTextColor);
            SetColor(
                formulaBackgroundImage,
                style.FormulaTextBackgroundColor
            );

            SetColor(tagsText, style.TagTextColor);
        }

        private static string BuildTagsText(CardDefinition definition)
        {
            if (definition.Tags == null || definition.Tags.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            for (int index = 0; index < definition.Tags.Count; index++)
            {
                CardTagDefinition tag = definition.Tags[index];

                if (tag == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(" • ");
                }

                builder.Append(tag.DisplayName);
            }

            return builder.ToString();
        }

        private static void SetText(TMP_Text textComponent, string value)
        {
            if (textComponent != null)
            {
                textComponent.text = value ?? string.Empty;
            }
        }

        private static void SetColor(Graphic graphic, Color color)
        {
            if (graphic != null)
            {
                graphic.color = color;
            }
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                RefreshPreview();
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                RefreshPreview();
            }
        }
#endif
    }
}