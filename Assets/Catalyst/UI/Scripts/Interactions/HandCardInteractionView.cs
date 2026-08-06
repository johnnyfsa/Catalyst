using System;
using Catalyst.Cards.Runtime;
using Catalyst.UI.Presentation.Inspection;
using Catalyst.UI.Presentation.Interaction;
using Catalyst.UI.Presentation.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Hand
{
    public sealed class HandCardInteractionView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Local Card")]
        [SerializeField]
        private HandCardView handCardView;

        [SerializeField]
        private GameObject selectionOutline;

        private BasicAudioPresenter audioPresenter;

        private CardInspectionPresenter inspectionPresenter;
        private HandCardDragPresenter dragPresenter;
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
                    handCardView
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

            if (!handCardView.HasBoundCard)
            {
                return;
            }

            CardInstance card =
                handCardView.BoundCard;

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

            if (!handCardView.HasBoundCard)
            {
                return;
            }

            dragStarted = true;

            inspectionPresenter.Close();

            dragPresenter.BeginDrag(
                handCardView,
                eventData,
                CardDragOrigin.Hand
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
                handCardView,
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
                handCardView
            );

            dragStarted = false;
        }

        private void OnDisable()
        {
            dragStarted = false;

            if (dragPresenter != null)
            {
                dragPresenter.CancelDrag(
                    handCardView
                );
            }

            if (selectionOutline != null)
            {
                selectionOutline.SetActive(false);
            }
        }

        private void ValidateReferences()
        {
            if (handCardView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardInteractionView)} on '{name}' " +
                    $"has no {nameof(HandCardView)} assigned."
                );
            }

            if (inspectionPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardInteractionView)} on '{name}' " +
                    "has not been initialized with a " +
                    $"{nameof(CardInspectionPresenter)}."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardInteractionView)} on '{name}' " +
                    "has not been initialized with a " +
                    $"{nameof(HandCardDragPresenter)}."
                );
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (handCardView == null)
            {
                handCardView =
                    GetComponent<HandCardView>();
            }
        }
#endif
    }
}