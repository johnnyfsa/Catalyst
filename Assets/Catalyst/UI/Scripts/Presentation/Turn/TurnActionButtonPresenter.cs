using System;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Hand;
using Catalyst.UI.Presentation.ReactionTable;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.Turn
{
    public sealed class TurnActionButtonPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Action")]
        [SerializeField]
        private Button turnButton;

        [SerializeField]
        private ActionButtonVisual buttonVisual;

        [Header("Visual Refresh")]
        [SerializeField]
        private InitialHandPresenter handPresenter;

        [SerializeField]
        private ReactionTablePresenter
            reactionTablePresenter;

        [SerializeField]
        private ReactionAvailabilityPresenter
            reactionAvailabilityPresenter;

        [SerializeField]
        private InitialTurnCounterPresenter
            turnCounterPresenter;

        [SerializeField]
        private InitialRemainingTurnsPresenter
            remainingTurnsPresenter;

        private bool interactionLocked;

        public bool InteractionLocked =>
            interactionLocked;

        private void OnEnable()
        {
            ValidateReferences();

            turnButton.onClick.AddListener(
                TryAdvanceTurn
            );
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (turnButton != null)
            {
                turnButton.onClick.RemoveListener(
                    TryAdvanceTurn
                );

                turnButton.interactable = false;
            }

            if (buttonVisual != null)
            {
                buttonVisual.SetInactive();
            }
        }

        public void SetInteractionLocked(
            bool locked
        )
        {
            interactionLocked = locked;
            Refresh();
        }

        [ContextMenu("Refresh Turn Action")]
        public void Refresh()
        {
            ValidateReferences();

            bool canAdvanceTurn =
                CanAdvanceTurn();

            turnButton.interactable =
                canAdvanceTurn;

            if (canAdvanceTurn)
            {
                buttonVisual.SetActive();
            }
            else
            {
                buttonVisual.SetInactive();
            }
        }

        public void TryAdvanceTurn()
        {
            ValidateReferences();

            if (!CanAdvanceTurn())
            {
                Refresh();
                return;
            }

            GameSession session =
                bootstrap.Session;

            TurnAdvanceResult result =
                bootstrap.SessionFlow
                    .TryAdvanceTurn(
                        session
                    );

            if (!result.Succeeded)
            {
                Debug.LogWarning(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' could not advance the turn. " +
                    $"Failure: {result.Failure}. " +
                    $"Main phase failure: " +
                    $"{result.MainPhaseFailure}. " +
                    $"End phase failure: " +
                    $"{result.EndPhaseFailure}.",
                    this
                );

                Refresh();
                return;
            }

            RefreshAfterSuccess();
        }

        private bool CanAdvanceTurn()
        {
            if (interactionLocked)
            {
                return false;
            }

            if (
                bootstrap == null
                || bootstrap.Session == null
                || bootstrap.SessionFlow == null
            )
            {
                return false;
            }

            GameSession session =
                bootstrap.Session;

            if (!session.IsRunning)
            {
                return false;
            }

            if (
                session.Turn.CurrentPhase
                != GamePhase.Main
            )
            {
                return false;
            }

            int projectedHandCount =
                session.Hand.Count
                + session.ReactionTable.Count;

            return projectedHandCount
                < session.Hand.Capacity;
        }

        private void RefreshAfterSuccess()
        {
            handPresenter.PresentInitialHand();

            reactionTablePresenter.Refresh();

            reactionAvailabilityPresenter.Refresh();

            turnCounterPresenter
                .PresentInitialTurn();

            remainingTurnsPresenter
                .PresentInitialRemainingTurns();

            Refresh();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (turnButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no Button assigned."
                );
            }

            if (buttonVisual == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ActionButtonVisual)} assigned."
                );
            }

            if (handPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(InitialHandPresenter)} assigned."
                );
            }

            if (reactionTablePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionTablePresenter)} assigned."
                );
            }

            if (reactionAvailabilityPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    "assigned."
                );
            }

            if (turnCounterPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(InitialTurnCounterPresenter)} " +
                    "assigned."
                );
            }

            if (remainingTurnsPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnActionButtonPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(InitialRemainingTurnsPresenter)} " +
                    "assigned."
                );
            }
        }
    }
}