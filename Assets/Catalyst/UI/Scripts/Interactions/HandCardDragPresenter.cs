using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Hand
{
    public sealed class HandCardDragPresenter : MonoBehaviour
    {
        [Header("Drag Layer")]
        [SerializeField]
        private RectTransform dragLayer;

        [Header("Drag Proxy")]
        [SerializeField]
        private RectTransform dragProxyTransform;

        [SerializeField]
        private HandCardView dragProxyView;

        private HandCardView draggedCardView;

        public bool IsDragging =>
            draggedCardView != null;

        public void BeginDrag(
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

            ValidateReferences();

            if (!sourceCardView.HasBoundCard)
            {
                return;
            }

            CancelCurrentDrag();

            draggedCardView = sourceCardView;

            dragProxyView.Bind(
                sourceCardView.BoundCard
            );

            dragProxyTransform.gameObject.SetActive(
                true
            );

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
        }

        private void OnDisable()
        {
            CancelCurrentDrag();
        }
    }
}