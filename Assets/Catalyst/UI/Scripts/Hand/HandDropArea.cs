using System;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Interaction;
using Catalyst.UI.Presentation.ReactionTable;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Hand
{
    public sealed class HandDropArea :
        MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Drag Source")]
        [SerializeField]
        private HandCardDragPresenter dragPresenter;

        [Header("Visual Refresh")]
        [SerializeField]
        private InitialHandPresenter handPresenter;

        [SerializeField]
        private ReactionTablePresenter
            reactionTablePresenter;

        [SerializeField]
        private ReactionAvailabilityPresenter
            reactionAvailabilityPresenter;

        private readonly CardMovementService
            movementService =
                new CardMovementService();

        public void OnPointerEnter(
            PointerEventData eventData
        )
        {
            ValidateReferences();

            bool isCardComingFromReactionTable =
                dragPresenter.IsDraggingFrom(
                    CardDragOrigin.ReactionTable
                );

            dragPresenter.SetInteractionAvailable(
                isCardComingFromReactionTable
            );
        }

        public void OnPointerExit(
            PointerEventData eventData
        )
        {
            if (dragPresenter == null)
            {
                return;
            }

            dragPresenter.SetInteractionAvailable(false);
        }

        public void OnDrop(
            PointerEventData eventData
        )
        {
            ValidateReferences();

            dragPresenter.SetInteractionAvailable(false);

            if (!dragPresenter.IsDraggingFrom(
                    CardDragOrigin.ReactionTable
                ))
            {
                return;
            }

            if (!dragPresenter.TryGetDraggedCard(
                    out CardInstance card
                ))
            {
                return;
            }

            GameSession session = bootstrap.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    "cannot process a drop because the game " +
                    "session has not been initialized."
                );
            }

            CardMovementResult movementResult =
                movementService.TryMove(
                    card,
                    session.ReactionTable,
                    session.Hand
                );

            if (!movementResult.Succeeded)
            {
                Debug.LogWarning(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    "could not return the dragged card to " +
                    "the hand. Failure: " +
                    $"{movementResult.Failure}.",
                    this
                );

                return;
            }

            handPresenter.PresentInitialHand();
            reactionTablePresenter.Refresh();
            reactionAvailabilityPresenter.Refresh();
        }

        private void OnDisable()
        {
            if (dragPresenter != null)
            {
                dragPresenter.SetInteractionAvailable(false);
            }
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    $"has no {nameof(GameSessionBootstrap)} " +
                    "assigned."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    $"has no {nameof(HandCardDragPresenter)} " +
                    "assigned."
                );
            }

            if (handPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    $"has no {nameof(InitialHandPresenter)} " +
                    "assigned."
                );
            }

            if (reactionTablePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    $"has no {nameof(ReactionTablePresenter)} " +
                    "assigned."
                );
            }

            if (reactionAvailabilityPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandDropArea)} on '{name}' " +
                    $"has no " +
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    "assigned."
                );
            }
        }
    }
}