using System;
using Catalyst.UI.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation
{
    [ExecuteAlways]
    public sealed class ResourceEntryStyleView : MonoBehaviour
    {
        [Header("Editor Preview")]
        [Tooltip(
            "Style used for Editor preview. Runtime bindings may " +
            "replace it when the session UI is initialized."
        )]
        [SerializeField]
        private ResourceEntryStyleAsset initialStyle;

        [SerializeField]
        [Min(0)]
        private int previewAmount;

        [Header("Visual References")]
        [SerializeField]
        private Image outlineImage;

        [SerializeField]
        private TMP_Text resourceNameText;

        [SerializeField]
        private TMP_Text resourceAmountText;

        [SerializeField]
        private Image resourceIconImage;

        private ResourceEntryStyleAsset boundStyle;

        public ResourceEntryStyleAsset BoundStyle =>
            boundStyle;

        public void Bind(
            ResourceEntryStyleAsset style,
            int amount
        )
        {
            if (style == null)
            {
                throw new ArgumentNullException(
                    nameof(style)
                );
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Resource amount cannot be negative."
                );
            }

            boundStyle = style;

            ApplyStyle(style);
            SetAmount(amount);
        }

        public void SetAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Resource amount cannot be negative."
                );
            }

            if (resourceAmountText != null)
            {
                resourceAmountText.text =
                    amount.ToString();
            }
        }

        public void Clear()
        {
            boundStyle = null;

            SetText(
                resourceNameText,
                string.Empty
            );

            SetText(
                resourceAmountText,
                string.Empty
            );

            if (resourceIconImage != null)
            {
                resourceIconImage.sprite = null;
                resourceIconImage.enabled = false;
            }
        }

        [ContextMenu("Refresh Preview")]
        public void RefreshPreview()
        {
            if (
                Application.isPlaying ||
                initialStyle == null
            )
            {
                return;
            }

            ApplyStyle(initialStyle);
            SetAmount(previewAmount);
        }

        private void ApplyStyle(
            ResourceEntryStyleAsset style
        )
        {
            SetText(
                resourceNameText,
                style.DisplayName
            );

            SetColor(
                outlineImage,
                style.OutlineColor
            );

            SetColor(
                resourceNameText,
                style.NameTextColor
            );

            SetColor(
                resourceAmountText,
                style.AmountTextColor
            );

            if (resourceIconImage != null)
            {
                resourceIconImage.sprite =
                    style.Icon;

                resourceIconImage.color =
                    style.IconColor;

                resourceIconImage.enabled =
                    style.Icon != null;
            }
        }

        private static void SetText(
            TMP_Text textComponent,
            string value
        )
        {
            if (textComponent != null)
            {
                textComponent.text =
                    value ?? string.Empty;
            }
        }

        private static void SetColor(
            Graphic graphic,
            Color color
        )
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