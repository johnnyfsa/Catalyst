using System;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Discard;
using Catalyst.UI.Presentation.Hand;
using Catalyst.UI.Presentation.Inspection;
using Catalyst.UI.Presentation.ReactionTable;
using Catalyst.UI.Presentation.Turn;
using UnityEngine;

namespace Catalyst.UI.Presentation.Session
{
    public sealed class GameSessionEndPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Inspection")]
        [SerializeField]
        private CardInspectionPresenter
            inspectionPresenter;

        [Header("Card Interaction")]
        [SerializeField]
        private HandCardDragPresenter
            dragPresenter;

        [SerializeField]
        private InitialHandPresenter
            handPresenter;

        [SerializeField]
        private ReactionTablePresenter
            reactionTablePresenter;

        [Header("Drop Areas")]
        [SerializeField]
        private HandDropArea handDropArea;

        [SerializeField]
        private ReactionTableDropArea
            reactionTableDropArea;

        [SerializeField]
        private DiscardDropArea
            discardDropArea;

        [Header("Actions")]
        [SerializeField]
        private ReactionAvailabilityPresenter
            reactionAvailabilityPresenter;

        [SerializeField]
        private TurnActionButtonPresenter
            turnActionButtonPresenter;

        public bool HasAppliedSessionEnd
        {
            get;
            private set;
        }

        private void Start()
        {
            ApplyIfSessionEnded();
        }

        public bool ApplyIfSessionEnded()
        {
            ValidateReferences();

            if (
                bootstrap.Session == null
                || !bootstrap.Session.HasEnded
            )
            {
                return false;
            }

            inspectionPresenter.Close();

            dragPresenter.SetInteractionLocked(true);

            handPresenter.SetInteractionLocked(true);

            reactionTablePresenter
                .SetInteractionLocked(true);

            handDropArea.SetInteractionLocked(true);

            reactionTableDropArea
                .SetInteractionLocked(true);

            discardDropArea
                .SetInteractionLocked(true);

            reactionAvailabilityPresenter
                .SetInteractionLocked(true);

            turnActionButtonPresenter
                .SetInteractionLocked(true);

            HasAppliedSessionEnd = true;

            Debug.Log(
                $"Session ended. " +
                $"Reason: {bootstrap.Session.EndReason}. " +
                $"Turn: {bootstrap.Session.Turn.TurnNumber}. " +
                $"Maximum turns: " +
                $"{FormatMaximumTurns()}. " +
                $"Phase: " +
                $"{bootstrap.Session.Turn.CurrentPhase}. " +
                $"Hand: {bootstrap.Session.Hand.Count}/" +
                $"{bootstrap.Session.Hand.Capacity}. " +
                $"Deck: {bootstrap.Session.Deck.Count}. " +
                "Interactions locked: true.",
                this
            );

            return true;
        }

        private string FormatMaximumTurns()
        {
            return bootstrap.Session.HasTurnLimit
                ? bootstrap.Session.MaximumTurns.Value
                    .ToString()
                : "None";
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (inspectionPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(CardInspectionPresenter)} assigned."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(HandCardDragPresenter)} assigned."
                );
            }

            if (handPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(InitialHandPresenter)} assigned."
                );
            }

            if (reactionTablePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionTablePresenter)} assigned."
                );
            }

            if (handDropArea == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(HandDropArea)} assigned."
                );
            }

            if (reactionTableDropArea == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionTableDropArea)} assigned."
                );
            }

            if (discardDropArea == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(DiscardDropArea)} assigned."
                );
            }

            if (reactionAvailabilityPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    "assigned."
                );
            }

            if (turnActionButtonPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameSessionEndPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(TurnActionButtonPresenter)} " +
                    "assigned."
                );
            }
        }
    }
}