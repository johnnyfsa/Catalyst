using System;
using Catalyst.Cards.Runtime;
using Catalyst.UI.Presentation.Inspection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Hand
{
    public sealed class HandCardInteractionView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        [Header("Local Card")]
        [SerializeField]
        private HandCardView handCardView;

        [SerializeField]
        private GameObject selectionOutline;

        private CardInspectionPresenter inspectionPresenter;
        private bool dragStarted;

        public bool IsInitialized =>
            inspectionPresenter != null;

        public void Initialize(
            CardInspectionPresenter presenter
        )
        {
            inspectionPresenter = presenter
                ?? throw new ArgumentNullException(
                    nameof(presenter)
                );
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
        }

        public void OnBeginDrag(
            PointerEventData eventData
        )
        {
            dragStarted = true;

            if (inspectionPresenter != null)
            {
                inspectionPresenter.Close();
            }
        }

        public void OnEndDrag(
            PointerEventData eventData
        )
        {
            dragStarted = false;
        }

        private void OnDisable()
        {
            dragStarted = false;

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