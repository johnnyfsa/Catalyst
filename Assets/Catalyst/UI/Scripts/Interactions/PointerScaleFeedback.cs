using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Interaction
{
    public sealed class PointerScaleFeedback :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Scale Feedback")]
        [SerializeField]
        [Min(1f)]
        private float hoverScaleMultiplier = 1.05f;

        private Vector3 originalScale;
        private bool isInitialized;

        private void Awake()
        {
            CacheOriginalScale();
        }

        public void OnPointerEnter(
            PointerEventData eventData
        )
        {
            CacheOriginalScale();

            transform.localScale =
                originalScale * hoverScaleMultiplier;
        }

        public void OnPointerExit(
            PointerEventData eventData
        )
        {
            CacheOriginalScale();

            transform.localScale =
                originalScale;
        }

        private void OnDisable()
        {
            if (isInitialized)
            {
                transform.localScale =
                    originalScale;
            }
        }

        private void CacheOriginalScale()
        {
            if (isInitialized)
            {
                return;
            }

            originalScale = transform.localScale;
            isInitialized = true;
        }
    }
}