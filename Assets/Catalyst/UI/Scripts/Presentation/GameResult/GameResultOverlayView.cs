using System;
using Catalyst.UI.Definitions.Results;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.GameResult
{
    public sealed class GameResultOverlayView :
        MonoBehaviour
    {
        [Header("Visibility")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Outcome Visuals")]
        [SerializeField]
        private GameObject victoryVisual;

        [SerializeField]
        private GameObject defeatVisual;

        [Header("Content")]
        [SerializeField]
        private Image resultIcon;

        [SerializeField]
        private TMP_Text resultTitleText;

        [SerializeField]
        private TMP_Text resultMessageText;

        [SerializeField]
        private TMP_Text resultSummaryText;

        private void Awake()
        {
            ValidateReferences();
            Hide();
        }

        public void Present(
            GameResultDefinition definition,
            string summary
        )
        {
            ValidateReferences();

            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            ApplyOutcome(
                definition.Outcome
            );

            ApplyIcon(
                definition.Icon
            );

            resultTitleText.text =
                definition.Title;

            resultMessageText.text =
                definition.Message;

            resultSummaryText.text =
                summary ?? string.Empty;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            ValidateReferences();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void ApplyOutcome(
            GameResultOutcome outcome
        )
        {
            switch (outcome)
            {
                case GameResultOutcome.Victory:
                    victoryVisual.SetActive(true);
                    defeatVisual.SetActive(false);
                    break;

                case GameResultOutcome.Defeat:
                    victoryVisual.SetActive(false);
                    defeatVisual.SetActive(true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(outcome),
                        outcome,
                        "Unsupported game result outcome."
                    );
            }
        }

        private void ApplyIcon(
            Sprite icon
        )
        {
            bool hasIcon =
                icon != null;

            resultIcon.sprite =
                icon;

            resultIcon.enabled =
                hasIcon;
        }

        private void ValidateReferences()
        {
            if (canvasGroup == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no " +
                    $"{nameof(CanvasGroup)} assigned."
                );
            }

            if (victoryVisual == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no victory visual assigned."
                );
            }

            if (defeatVisual == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no defeat visual assigned."
                );
            }

            if (ReferenceEquals(
                victoryVisual,
                defeatVisual
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' cannot use the same GameObject " +
                    "for victory and defeat visuals."
                );
            }

            if (resultIcon == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no result icon assigned."
                );
            }

            if (resultTitleText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no result title text assigned."
                );
            }

            if (resultMessageText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no result message text assigned."
                );
            }

            if (resultSummaryText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' has no result summary text assigned."
                );
            }

        }
    }
}