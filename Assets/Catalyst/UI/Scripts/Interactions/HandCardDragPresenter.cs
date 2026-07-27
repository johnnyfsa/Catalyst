using System;
using Catalyst.Cards.Runtime;
using Catalyst.UI.Presentation.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Hand
{
    public sealed class HandCardDragPresenter :
        MonoBehaviour
    {
        [Header("Drag Layer")]
        [SerializeField]
        private RectTransform dragLayer;

        [Header("Drag Proxy")]
        [SerializeField]
        private RectTransform dragProxyTransform;

        [SerializeField]
        private HandCardView dragProxyView;

        [SerializeField]
        private GameObject interactionOutline;

        private HandCardView draggedCardView;

        private CardDragOrigin dragOrigin =
            CardDragOrigin.None;

        public bool IsDragging =>
            draggedCardView != null;

        public CardDragOrigin DragOrigin =>
            IsDragging
                ? dragOrigin
                : CardDragOrigin.None;

        public bool TryGetDraggedCard(
            out CardInstance card
        )
        {
            card = null;

            if (
                draggedCardView == null
                || !draggedCardView.HasBoundCard
            )
            {
                return false;
            }

            card = draggedCardView.BoundCard;
            return true;
        }

        public bool IsDraggingFrom(
            CardDragOrigin origin
        )
        {
            return IsDragging
                && dragOrigin == origin;
        }

        public void SetInteractionAvailable(
            bool isAvailable
        )
        {
            if (interactionOutline == null)
            {
                return;
            }

            bool shouldShow =
                isAvailable && IsDragging;

            interactionOutline.SetActive(
                shouldShow
            );
        }

        public void BeginDrag(
            HandCardView sourceCardView,
            PointerEventData eventData,
            CardDragOrigin origin
        )
        {
            if (sourceCardView == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceCardView)
                );
            }

            if (eventData == null)
            {
                throw new ArgumentNullException(
                    nameof(eventData)
                );
            }

            if (origin == CardDragOrigin.None)
            {
                throw new ArgumentException(
                    "A card drag must have a valid origin.",
                    nameof(origin)
                );
            }

            ValidateReferences();

            if (!sourceCardView.HasBoundCard)
            {
                return;
            }

            CancelCurrentDrag();

            draggedCardView = sourceCardView;
            dragOrigin = origin;

            dragProxyView.Bind(
                sourceCardView.BoundCard
            );

            dragProxyTransform.gameObject.SetActive(
                true
            );

            interactionOutline.SetActive(false);

            UpdateProxyPosition(
                eventData
            );
        }

        public void ContinueDrag(
            HandCardView sourceCardView,
            PointerEventData eventData
        )
        {
            if (sourceCardView == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceCardView)
                );
            }

            if (eventData == null)
            {
                throw new ArgumentNullException(
                    nameof(eventData)
                );
            }

            if (!ReferenceEquals(
                draggedCardView,
                sourceCardView
            ))
            {
                return;
            }

            UpdateProxyPosition(
                eventData
            );
        }

        public void EndDrag(
            HandCardView sourceCardView
        )
        {
            if (sourceCardView == null)
            {
                return;
            }

            if (!ReferenceEquals(
                draggedCardView,
                sourceCardView
            ))
            {
                return;
            }

            CancelCurrentDrag();
        }

        public void CancelDrag(
            HandCardView sourceCardView
        )
        {
            if (sourceCardView == null)
            {
                return;
            }

            if (!ReferenceEquals(
                draggedCardView,
                sourceCardView
            ))
            {
                return;
            }

            CancelCurrentDrag();
        }

        private void UpdateProxyPosition(
            PointerEventData eventData
        )
        {
            if (!RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    dragLayer,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPosition
                ))
            {
                return;
            }

            dragProxyTransform.anchoredPosition =
                localPosition;
        }

        private void CancelCurrentDrag()
        {
            draggedCardView = null;
            dragOrigin = CardDragOrigin.None;

            if (interactionOutline != null)
            {
                interactionOutline.SetActive(false);
            }

            if (dragProxyView != null)
            {
                dragProxyView.Clear();
            }

            if (dragProxyTransform != null)
            {
                dragProxyTransform.gameObject.SetActive(
                    false
                );
            }
        }

        private void ValidateReferences()
        {
            if (dragLayer == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    "has no drag layer assigned."
                );
            }

            if (dragProxyTransform == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    "has no drag proxy transform assigned."
                );
            }

            if (dragProxyView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    $"has no {nameof(HandCardView)} assigned " +
                    "for the drag proxy."
                );
            }

            if (interactionOutline == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    "has no interaction outline assigned."
                );
            }

            bool proxyViewBelongsToProxy =
                dragProxyView.transform
                    == dragProxyTransform
                || dragProxyView.transform.IsChildOf(
                    dragProxyTransform
                );

            if (!proxyViewBelongsToProxy)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    $"requires its {nameof(HandCardView)} to be " +
                    "on the drag proxy or one of its children."
                );
            }

            bool outlineBelongsToProxy =
                interactionOutline.transform
                    == dragProxyTransform
                || interactionOutline.transform.IsChildOf(
                    dragProxyTransform
                );

            if (!outlineBelongsToProxy)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    "requires its interaction outline to be " +
                    "inside the drag proxy hierarchy."
                );
            }
        }

        private void OnDisable()
        {
            CancelCurrentDrag();
        }
    }
}