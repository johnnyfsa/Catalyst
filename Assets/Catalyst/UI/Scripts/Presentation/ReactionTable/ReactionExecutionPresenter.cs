using System;
using Catalyst.Game.Bootstrap;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime.Resolution;
using Catalyst.UI.Presentation.Session;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ReactionExecutionPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Reaction State")]
        [SerializeField]
        private ReactionAvailabilityPresenter
            reactionAvailabilityPresenter;

        [Header("Visual Refresh")]
        [SerializeField]
        private ReactionTablePresenter
            reactionTablePresenter;

        [SerializeField]
        private InitialSessionCountersPresenter
            countersPresenter;

        [Header("Reaction Action")]
        [SerializeField]
        private Button reactionButton;

        [Header("Animation")]
        [SerializeField]
        private TableAnimationOverlayView
            tableAnimationOverlay;

        private bool isExecuting;

        private ReactionDefinition pendingReaction;

        public bool IsExecuting =>
            isExecuting;

        private void OnEnable()
        {
            ValidateReferences();

            reactionButton.onClick.AddListener(
                TryExecuteReaction
            );

            tableAnimationOverlay
                .ReactionMomentReached +=
                    HandleReactionMomentReached;

            tableAnimationOverlay
                .SequenceCompleted +=
                    HandleSequenceCompleted;
        }

        private void OnDisable()
        {
            if (reactionButton != null)
            {
                reactionButton.onClick.RemoveListener(
                    TryExecuteReaction
                );
            }

            if (tableAnimationOverlay != null)
            {
                tableAnimationOverlay
                    .ReactionMomentReached -=
                        HandleReactionMomentReached;

                tableAnimationOverlay
                    .SequenceCompleted -=
                        HandleSequenceCompleted;
            }

            pendingReaction = null;
            isExecuting = false;
        }

        public void TryExecuteReaction()
        {
            if (isExecuting)
            {
                return;
            }

            ValidateReferences();

            ReactionDefinition resolvedReaction =
                reactionAvailabilityPresenter
                    .ResolvedReaction;

            if (
                resolvedReaction == null
                || !reactionAvailabilityPresenter
                    .CanReact
            )
            {
                reactionAvailabilityPresenter
                    .Refresh();

                return;
            }

            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' cannot execute a reaction " +
                    "because the game session has not been " +
                    "initialized."
                );
            }

            if (bootstrap.ReactionFlow == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' cannot execute a reaction " +
                    "because the reaction flow has not been " +
                    "initialized."
                );
            }

            pendingReaction = resolvedReaction;
            isExecuting = true;

            reactionButton.interactable = false;

            tableAnimationOverlay.PlayFadeIn();
        }

        private void HandleReactionMomentReached()
        {

            Debug.Log(
       "ReactionExecutionPresenter: HandleReactionMomentReached",
       this
   );
            if (!isExecuting)
            {
                Debug.LogWarning(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' received a reaction moment " +
                    "without an active execution.",
                    this
                );

                tableAnimationOverlay.PlayFadeOut();
                return;
            }

            if (pendingReaction == null)
            {
                Debug.LogError(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' reached the reaction moment " +
                    "without a pending reaction.",
                    this
                );

                RefreshAffectedPresenters();
                tableAnimationOverlay.PlayFadeOut();
                return;
            }

            try
            {
                ReactionFlowResult result =
                    bootstrap.ReactionFlow.TryResolve(
                        bootstrap.Session,
                        pendingReaction
                    );

                if (!result.Succeeded)
                {
                    Debug.LogWarning(
                        $"{nameof(ReactionExecutionPresenter)} " +
                        $"on '{name}' could not execute " +
                        $"reaction '{pendingReaction.name}'. " +
                        $"Failure: {result.Failure}.",
                        this
                    );
                }

                RefreshAffectedPresenters();
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this
                );

                RefreshAffectedPresenters();
            }

            tableAnimationOverlay.PlayFadeOut();
        }

        private void HandleSequenceCompleted()
        {
            if (!isExecuting)
            {
                return;
            }

            pendingReaction = null;
            isExecuting = false;

            reactionAvailabilityPresenter.Refresh();
        }

        private void RefreshAffectedPresenters()
        {
            reactionTablePresenter.Refresh();

            countersPresenter
                .PresentInitialCounters();

            reactionAvailabilityPresenter
                .Refresh();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (reactionAvailabilityPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    "assigned."
                );
            }

            if (reactionTablePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionTablePresenter)} assigned."
                );
            }

            if (countersPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    "assigned."
                );
            }

            if (reactionButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' has no reaction Button " +
                    "assigned."
                );
            }

            if (tableAnimationOverlay == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionExecutionPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(TableAnimationOverlayView)} " +
                    "assigned."
                );
            }
        }
    }
}