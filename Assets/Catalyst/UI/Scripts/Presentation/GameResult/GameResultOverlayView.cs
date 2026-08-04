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

        [Header("Outcome Backgrounds")]
        [SerializeField]
        private Image victoryBackground;

        [SerializeField]
        private Image defeatBackground;

        [Header("Outcome Colors")]
        [SerializeField]
        private Color victoryAccentColor =
            Color.white;

        [SerializeField]
        private Color defeatAccentColor =
            Color.white;

        [Header("Primary Content")]
        [SerializeField]
        private Image resultIcon;

        [SerializeField]
        private TMP_Text resultTitleText;

        [SerializeField]
        private TMP_Text resultMessageText;

        [Header("Objective Summary")]
        [SerializeField]
        private TMP_Text objectiveSummaryLabelText;

        [SerializeField]
        private TMP_Text objectiveSummaryValueText;

        [Header("Session Summary")]
        [SerializeField]
        private TMP_Text sessionSummaryLabelText;

        [SerializeField]
        private TMP_Text sessionSummaryValueText;

        private void Awake()
        {
            ValidateReferences();
            Hide();
        }

        public void Present(
    GameResultDefinition definition,
    GameResultSummary summary
)
        {
            ValidateReferences();

            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            if (summary == null)
            {
                throw new ArgumentNullException(
                    nameof(summary)
                );
            }

            ApplyOutcome(
                definition.Outcome
            );

            ApplyBackground(
                definition.Outcome,
                definition.Background
            );

            ApplyIcon(
                definition.Icon
            );

            resultTitleText.text =
                definition.Title
                ?? string.Empty;

            resultMessageText.text =
                definition.Message
                ?? string.Empty;

            ApplySummary(summary);
            Show();
        }
        public void Hide()
        {
            ValidateReferences();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
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

                    resultIcon.color =
                        victoryAccentColor;
                    break;

                case GameResultOutcome.Defeat:
                    victoryVisual.SetActive(false);
                    defeatVisual.SetActive(true);

                    resultIcon.color =
                        defeatAccentColor;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(outcome),
                        outcome,
                        "Unsupported game result outcome."
                    );
            }
        }

        private void ApplyBackground(
            GameResultOutcome outcome,
            Sprite background
        )
        {
            Image activeBackground;

            switch (outcome)
            {
                case GameResultOutcome.Victory:
                    activeBackground =
                        victoryBackground;
                    break;

                case GameResultOutcome.Defeat:
                    activeBackground =
                        defeatBackground;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(outcome),
                        outcome,
                        "Unsupported game result outcome."
                    );
            }

            activeBackground.sprite =
                background;

            activeBackground.enabled =
                background != null;
        }

        private void ApplyIcon(
            Sprite icon
        )
        {
            resultIcon.sprite =
                icon;

            resultIcon.enabled =
                icon != null;
        }

        private void ApplySummary(
    GameResultSummary summary
)
        {
            objectiveSummaryLabelText.text =
                summary.ObjectiveLabel;

            objectiveSummaryValueText.text =
                summary.ObjectiveValue;

            sessionSummaryLabelText.text =
                summary.SessionLabel;

            sessionSummaryValueText.text =
                summary.SessionValue;
        }

        private void ValidateReferences()
        {
            if (canvasGroup == null)
            {
                throw MissingReference(
                    nameof(CanvasGroup)
                );
            }

            if (victoryVisual == null)
            {
                throw MissingReference(
                    "victory visual"
                );
            }

            if (defeatVisual == null)
            {
                throw MissingReference(
                    "defeat visual"
                );
            }

            if (ReferenceEquals(
                victoryVisual,
                defeatVisual
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' cannot use the same " +
                    "GameObject for victory and defeat visuals."
                );
            }

            if (victoryBackground == null)
            {
                throw MissingReference(
                    "victory background"
                );
            }

            if (defeatBackground == null)
            {
                throw MissingReference(
                    "defeat background"
                );
            }

            if (ReferenceEquals(
                victoryBackground,
                defeatBackground
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultOverlayView)} " +
                    $"on '{name}' cannot use the same Image " +
                    "for victory and defeat backgrounds."
                );
            }

            if (resultIcon == null)
            {
                throw MissingReference(
                    "result icon"
                );
            }

            if (resultTitleText == null)
            {
                throw MissingReference(
                    "result title text"
                );
            }

            if (resultMessageText == null)
            {
                throw MissingReference(
                    "result message text"
                );
            }

            if (objectiveSummaryLabelText == null)
            {
                throw MissingReference(
                    "objective summary label text"
                );
            }

            if (objectiveSummaryValueText == null)
            {
                throw MissingReference(
                    "objective summary value text"
                );
            }

            if (sessionSummaryLabelText == null)
            {
                throw MissingReference(
                    "session summary label text"
                );
            }

            if (sessionSummaryValueText == null)
            {
                throw MissingReference(
                    "session summary value text"
                );
            }
        }

        private InvalidOperationException
            MissingReference(
                string referenceName
            )
        {
            return new InvalidOperationException(
                $"{nameof(GameResultOverlayView)} " +
                $"on '{name}' has no " +
                $"{referenceName} assigned."
            );
        }
    }
}