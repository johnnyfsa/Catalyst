using System;
using Catalyst.Cards.Runtime;
using Catalyst.UI.Presentation.Hand;
using Catalyst.UI.Presentation.Inspection;
using Catalyst.UI.Presentation.Interaction;
using Catalyst.UI.Presentation.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ReactionTableCardInteractionView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Local Card")]
        [SerializeField]
        private HandCardView cardView;

        [SerializeField]
        private GameObject selectionOutline;

        private CardInspectionPresenter
            inspectionPresenter;

        private HandCardDragPresenter
            dragPresenter;

        private BasicAudioPresenter
            audioPresenter;

        private bool dragStarted;

        private bool interactionLocked;

        public void SetInteractionLocked(
            bool locked
        )
        {
            interactionLocked = locked;

            if (!locked)
            {
                return;
            }

            dragStarted = false;

            if (dragPresenter != null)
            {
                dragPresenter.CancelDrag(
                    cardView
                );
            }

            if (selectionOutline != null)
            {
                selectionOutline.SetActive(false);
            }
        }

        public bool IsInitialized =>
            inspectionPresenter != null
            && dragPresenter != null;

        public void Initialize(
    CardInspectionPresenter presenter,
    HandCardDragPresenter cardDragPresenter,
    BasicAudioPresenter basicAudioPresenter
    )
        {
            inspectionPresenter = presenter
                ?? throw new ArgumentNullException(
                    nameof(presenter)
                );

            dragPresenter = cardDragPresenter
                ?? throw new ArgumentNullException(
                    nameof(cardDragPresenter)
                );

            audioPresenter = basicAudioPresenter;
        }

        public void OnPointerClick(
            PointerEventData eventData
        )
        {

            if (dragStarted)
            {
                return;
            }

            ValidateReferences();
            if (interactionLocked)
            {
                return;
            }

            if (!cardView.HasBoundCard)
            {
                return;
            }

            CardInstance card =
                cardView.BoundCard;

            inspectionPresenter.Open(
                card,
                selectionOutline
            );
            audioPresenter?.PlayCardClick();
        }

        public void OnBeginDrag(
            PointerEventData eventData
        )
        {
            if (interactionLocked)
            {
                return;
            }
            ValidateReferences();

            if (!cardView.HasBoundCard)
            {
                return;
            }

            dragStarted = true;

            inspectionPresenter.Close();

            dragPresenter.BeginDrag(
                cardView,
                eventData,
                CardDragOrigin.ReactionTable
            );
            audioPresenter?.PlayCardClick();
        }

        public void OnDrag(
            PointerEventData eventData
        )
        {
            if (interactionLocked)
            {
                return;
            }
            if (!dragStarted)
            {
                return;
            }

            dragPresenter.ContinueDrag(
                cardView,
                eventData
            );
        }

        public void OnEndDrag(
            PointerEventData eventData
        )
        {
            if (!dragStarted)
            {
                return;
            }

            dragPresenter.EndDrag(
                cardView
            );

            dragStarted = false;
        }

        private void OnDisable()
        {
            dragStarted = false;

            if (dragPresenter != null)
            {
                dragPresenter.CancelDrag(
                    cardView
                );
            }

            if (selectionOutline != null)
            {
                selectionOutline.SetActive(false);
            }
        }

        private void ValidateReferences()
        {
            if (cardView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTableCardInteractionView)} " +
                    $"on '{name}' has no " +
                    $"{nameof(HandCardView)} assigned."
                );
            }

            if (selectionOutline == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTableCardInteractionView)} " +
                    $"on '{name}' has no selection outline " +
                    "assigned."
                );
            }

            if (inspectionPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTableCardInteractionView)} " +
                    $"on '{name}' has not been initialized " +
                    $"with a " +
                    $"{nameof(CardInspectionPresenter)}."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTableCardInteractionView)} " +
                    $"on '{name}' has not been initialized " +
                    $"with a " +
                    $"{nameof(HandCardDragPresenter)}."
                );
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (cardView == null)
            {
                cardView =
                    GetComponent<HandCardView>();
            }
        }
#endif
    }
}