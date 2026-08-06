using System;
using Catalyst.Game.Launch;
using Catalyst.UI.Presentation.ReactionTable;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.MainMenu
{
    public sealed class MainMenuPresenter :
        MonoBehaviour
    {
        [Header("New Game")]
        [SerializeField]
        private Button newGameButton;

        [SerializeField]
        private ActionButtonVisual
            newGameButtonVisual;

        [Header("Navigation")]
        [SerializeField]
        private string gameplaySceneName =
            "Phase1";

        private bool navigationRequested;

        private void Awake()
        {
            ValidateReferences();
            PresentNewGameEnabled();
        }

        private void OnEnable()
        {
            ValidateReferences();

            newGameButton.onClick.AddListener(
                StartNewGame
            );
        }

        private void OnDisable()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(
                    StartNewGame
                );
            }
        }

        private void StartNewGame()
        {
            if (navigationRequested)
            {
                return;
            }

            navigationRequested = true;
            PresentNewGameDisabled();

            int seed =
                SessionSeedGenerator.Generate();

            var request =
                new GameLaunchRequest(
                    seed,
                    StageEntryMode.ShowBriefing
                );

            GameLaunchContext.Prepare(request);

            Debug.Log(
                $"New game requested. " +
                $"Seed: {seed}. " +
                $"Entry mode: " +
                $"{StageEntryMode.ShowBriefing}. " +
                $"Scene: {gameplaySceneName}.",
                this
            );

            SceneManager.LoadScene(
                gameplaySceneName,
                LoadSceneMode.Single
            );
        }

        private void PresentNewGameEnabled()
        {
            navigationRequested = false;
            newGameButton.interactable = true;
            newGameButtonVisual.SetActive();
        }

        private void PresentNewGameDisabled()
        {
            newGameButton.interactable = false;
            newGameButtonVisual.SetInactive();
        }

        private void ValidateReferences()
        {
            if (newGameButton == null)
            {
                throw MissingReference(
                    "New Game Button"
                );
            }

            if (newGameButtonVisual == null)
            {
                throw MissingReference(
                    "New Game Action Button Visual"
                );
            }

            if (string.IsNullOrWhiteSpace(
                gameplaySceneName
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(MainMenuPresenter)} " +
                    $"on '{name}' has no gameplay " +
                    "scene name configured."
                );
            }
        }

        private InvalidOperationException
            MissingReference(
                string referenceName
            )
        {
            return new InvalidOperationException(
                $"{nameof(MainMenuPresenter)} " +
                $"on '{name}' has no " +
                $"{referenceName} assigned."
            );
        }
    }
}