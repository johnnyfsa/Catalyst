using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.Objectives
{
    public sealed class ObjectiveEntryView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField]
        private TMP_Text objectiveTitleText;

        [SerializeField]
        private TMP_Text objectiveDescriptionText;

        [SerializeField]
        private TMP_Text objectiveProgressText;

        [Header("Icon")]
        [SerializeField]
        private Image objectiveIconImage;

        public void Bind(
            string title,
            string description,
            Sprite icon,
            int currentAmount,
            int requiredAmount
        )
        {
            SetContent(
                title,
                description,
                icon
            );

            SetProgress(
                currentAmount,
                requiredAmount
            );
        }

        public void SetProgress(
            int currentAmount,
            int requiredAmount
        )
        {
            if (currentAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentAmount),
                    currentAmount,
                    "Current objective amount cannot be negative."
                );
            }

            if (requiredAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredAmount),
                    requiredAmount,
                    "Required objective amount must be greater than zero."
                );
            }

            if (objectiveProgressText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ObjectiveEntryView)} on '{name}' has no " +
                    "objective progress text assigned."
                );
            }

            objectiveProgressText.text =
                $"{currentAmount} / {requiredAmount}";
        }

        private void SetContent(
            string title,
            string description,
            Sprite icon
        )
        {
            ValidateContentReferences();

            objectiveTitleText.text =
                title ?? string.Empty;

            objectiveDescriptionText.text =
                description ?? string.Empty;

            objectiveIconImage.sprite = icon;
            objectiveIconImage.enabled = icon != null;
        }

        private void ValidateContentReferences()
        {
            if (objectiveTitleText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ObjectiveEntryView)} on '{name}' has no " +
                    "objective title text assigned."
                );
            }

            if (objectiveDescriptionText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ObjectiveEntryView)} on '{name}' has no " +
                    "objective description text assigned."
                );
            }

            if (objectiveProgressText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ObjectiveEntryView)} on '{name}' has no " +
                    "objective progress text assigned."
                );
            }

            if (objectiveIconImage == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ObjectiveEntryView)} on '{name}' has no " +
                    "objective icon image assigned."
                );
            }
        }
    }
}