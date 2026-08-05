using System;
using Catalyst.Game.Bootstrap;
using Catalyst.Game.Launch;
using Catalyst.UI.Presentation.ReactionTable;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.GameResult
{
    public sealed class GameResultActionsPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Retry")]
        [SerializeField]
        private Button retryButton;

        [SerializeField]
        private ActionButtonVisual retryButtonVisual;

        [Header("Return to Title")]
        [SerializeField]
        private Button returnToTitleButton;

        [SerializeField]
        private ActionButtonVisual
            returnToTitleButtonVisual;

        [SerializeField]
        private string titleSceneName =
            "MainMenu";

        private bool navigationRequested;

        private void Awake()
        {
            ValidateReferences();
            PresentActionsEnabled();
        }

        private void OnEnable()
        {
            ValidateReferences();

            retryButton.onClick.AddListener(
                Retry
            );

            returnToTitleButton.onClick.AddListener(
                ReturnToTitle
            );
        }

        private void OnDisable()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(
                    Retry
                );
            }

            if (returnToTitleButton != null)
            {
                returnToTitleButton.onClick
                    .RemoveListener(
                        ReturnToTitle
                    );
            }
        }

        private void Retry()
        {
            if (navigationRequested)
            {
                return;
            }

            navigationRequested = true;
            PresentActionsDisabled();

            int previousSeed =
                bootstrap.SessionSeed;

            int newSeed =
                SessionSeedGenerator
                    .GenerateDifferentFrom(
                        previousSeed
                    );

            var request =
                new GameLaunchRequest(
                    newSeed,
                    StageEntryMode.SkipBriefing
                );

            GameLaunchContext.Prepare(request);

            Debug.Log(
                $"Retry requested. " +
                $"Previous seed: {previousSeed}. " +
                $"New seed: {newSeed}. " +
                $"Entry mode: " +
                $"{StageEntryMode.SkipBriefing}.",
                this
            );

            Scene activeScene =
                SceneManager.GetActiveScene();

            SceneManager.LoadScene(
                activeScene.buildIndex,
                LoadSceneMode.Single
            );
        }

        private void ReturnToTitle()
        {
            if (navigationRequested)
            {
                return;
            }

            navigationRequested = true;
            PresentActionsDisabled();

            GameLaunchContext.Clear();

            Debug.Log(
                $"Returning to title. " +
                $"Launch context cleared. " +
                $"Scene: {titleSceneName}.",
                this
            );

            SceneManager.LoadScene(
                titleSceneName,
                LoadSceneMode.Single
            );
        }

        private void PresentActionsEnabled()
        {
            navigationRequested = false;

            retryButton.interactable = true;
            returnToTitleButton.interactable = true;

            retryButtonVisual.SetActive();
            returnToTitleButtonVisual.SetActive();
        }

        private void PresentActionsDisabled()
        {
            retryButton.interactable = false;
            returnToTitleButton.interactable =
                false;

            retryButtonVisual.SetInactive();
            returnToTitleButtonVisual
                .SetInactive();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw MissingReference(
                    nameof(GameSessionBootstrap)
                );
            }

            if (retryButton == null)
            {
                throw MissingReference(
                    "Retry Button"
                );
            }

            if (retryButtonVisual == null)
            {
                throw MissingReference(
                    "Retry Action Button Visual"
                );
            }

            if (returnToTitleButton == null)
            {
                throw MissingReference(
                    "Return To Title Button"
                );
            }

            if (returnToTitleButtonVisual == null)
            {
                throw MissingReference(
                    "Return To Title Action Button Visual"
                );
            }

            if (ReferenceEquals(
                retryButton,
                returnToTitleButton
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultActionsPresenter)} " +
                    $"on '{name}' cannot use the same " +
                    "Button for Retry and Return to Title."
                );
            }

            if (string.IsNullOrWhiteSpace(
                titleSceneName
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultActionsPresenter)} " +
                    $"on '{name}' has no title scene " +
                    "name configured."
                );
            }
        }

        private InvalidOperationException
            MissingReference(
                string referenceName
            )
        {
            return new InvalidOperationException(
                $"{nameof(GameResultActionsPresenter)} " +
                $"on '{name}' has no " +
                $"{referenceName} assigned."
            );
        }
    }
}