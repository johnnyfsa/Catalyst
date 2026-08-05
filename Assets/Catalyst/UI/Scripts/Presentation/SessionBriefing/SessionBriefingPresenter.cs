using System;
using Catalyst.Game.Bootstrap;
using Catalyst.Game.Launch;
using Catalyst.UI.Definitions.SessionBriefing;
using Catalyst.UI.Presentation.ReactionTable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.SessionBriefing
{
    public sealed class SessionBriefingPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Definition")]
        [SerializeField]
        private SessionBriefingDefinition definition;

        [Header("Overlay")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Content")]
        [SerializeField]
        private TMP_Text stageTitleText;

        [SerializeField]
        private TMP_Text briefingText;

        [SerializeField]
        private Image stageBackgroundImage;

        [Header("Start Mission Action")]
        [SerializeField]
        private Button startMissionButton;

        [SerializeField]
        private ActionButtonVisual
            startMissionButtonVisual;

        private bool startRequested;

        private void Awake()
        {
            ValidateReferences();
            Hide();
        }

        private void OnEnable()
        {
            ValidateReferences();

            startMissionButton.onClick.AddListener(
                StartMission
            );
        }

        private void Start()
        {
            ApplyDefinition();
            PresentEntryMode();
        }

        private void OnDisable()
        {
            if (startMissionButton != null)
            {
                startMissionButton.onClick.RemoveListener(
                    StartMission
                );
            }
        }

        private void ApplyDefinition()
        {
            stageTitleText.text =
                definition.StageTitle
                ?? string.Empty;

            briefingText.text =
                definition.BriefingText
                ?? string.Empty;

            stageBackgroundImage.sprite =
                definition.StageBackground;

            stageBackgroundImage.enabled =
                definition.StageBackground != null;
        }

        private void PresentEntryMode()
        {
            switch (bootstrap.EntryMode)
            {
                case StageEntryMode.ShowBriefing:
                    Show();
                    break;

                case StageEntryMode.SkipBriefing:
                    Hide();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(bootstrap.EntryMode),
                        bootstrap.EntryMode,
                        "Unsupported stage entry mode."
                    );
            }
        }

        private void StartMission()
        {
            if (startRequested)
            {
                return;
            }

            startRequested = true;
            PresentButtonEnabled(false);

            bool sessionStarted =
                bootstrap.StartSession();

            if (!sessionStarted)
            {
                Debug.LogWarning(
                    $"{nameof(SessionBriefingPresenter)} " +
                    $"on '{name}' received a start request, " +
                    "but the game session was not in a " +
                    "startable state.",
                    this
                );

                Hide();
                return;
            }

            Hide();

            Debug.Log(
                $"Briefing confirmed. " +
                $"Session started. " +
                $"Seed: {bootstrap.SessionSeed}. " +
                $"Entry mode: {bootstrap.EntryMode}.",
                this
            );
        }

        private void Show()
        {
            startRequested = false;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            PresentButtonEnabled(true);
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            PresentButtonEnabled(false);
        }

        private void PresentButtonEnabled(
            bool enabled
        )
        {
            startMissionButton.interactable =
                enabled;

            if (enabled)
            {
                startMissionButtonVisual.SetActive();
            }
            else
            {
                startMissionButtonVisual.SetInactive();
            }
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw MissingReference(
                    nameof(GameSessionBootstrap)
                );
            }

            if (definition == null)
            {
                throw MissingReference(
                    nameof(SessionBriefingDefinition)
                );
            }

            if (canvasGroup == null)
            {
                throw MissingReference(
                    nameof(CanvasGroup)
                );
            }

            if (stageTitleText == null)
            {
                throw MissingReference(
                    "Stage Title Text"
                );
            }

            if (briefingText == null)
            {
                throw MissingReference(
                    "Briefing Text"
                );
            }

            if (stageBackgroundImage == null)
            {
                throw MissingReference(
                    "Stage Background Image"
                );
            }

            if (startMissionButton == null)
            {
                throw MissingReference(
                    "Start Mission Button"
                );
            }

            if (startMissionButtonVisual == null)
            {
                throw MissingReference(
                    nameof(ActionButtonVisual)
                );
            }
        }

        private InvalidOperationException
            MissingReference(
                string referenceName
            )
        {
            return new InvalidOperationException(
                $"{nameof(SessionBriefingPresenter)} " +
                $"on '{name}' has no " +
                $"{referenceName} assigned."
            );
        }
    }
}