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

        [Header("Drag Proxy Appearance")]
        [SerializeField]
        private CanvasGroup dragProxyCanvasGroup;

        [SerializeField]
        [Range(0f, 1f)]
        private float defaultProxyAlpha = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float discardHoverProxyAlpha = 0.35f;

        private HandCardView draggedCardView;

        private CardDragOrigin dragOrigin =
            CardDragOrigin.None;

        public bool IsDragging =>
            draggedCardView != null;

        public CardDragOrigin DragOrigin =>
            IsDragging
                ? dragOrigin
                : CardDragOrigin.None;

        private bool interactionLocked;

        public bool InteractionLocked =>
            interactionLocked;

        public void SetInteractionLocked(
            bool locked
        )
        {
            interactionLocked = locked;

            if (locked)
            {
                CancelCurrentDrag();
            }
        }

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

            if (interactionLocked)
            {
                interactionOutline?.SetActive(false);
                return;
            }
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

        public void SetDiscardHoverVisual(
    bool isHoveringDiscard
)
        {
            if (dragProxyCanvasGroup == null)
            {
                return;
            }

            dragProxyCanvasGroup.alpha =
                isHoveringDiscard
                    ? discardHoverProxyAlpha
                    : defaultProxyAlpha;
        }

        public void BeginDrag(
            HandCardView sourceCardView,
            PointerEventData eventData,
            CardDragOrigin origin
        )
        {
            if (interactionLocked)
            {
                return;
            }

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

            if (dragProxyCanvasGroup != null)
            {
                dragProxyCanvasGroup.alpha =
                    defaultProxyAlpha;
            }

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

            if (interactionLocked)
            {
                return;
            }
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

            if (dragProxyCanvasGroup == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardDragPresenter)} on '{name}' " +
                    "has no drag proxy CanvasGroup assigned."
                );
            }
        }

        private void OnDisable()
        {
            CancelCurrentDrag();
        }
    }
}