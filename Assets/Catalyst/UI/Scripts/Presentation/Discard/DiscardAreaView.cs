using System;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.Discard
{
    public sealed class DiscardAreaView :
        MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField]
        private Image frameProgress;

        [SerializeField]
        private Image discardSymbol;

        [Header("Symbol Opacity")]
        [SerializeField]
        [Range(0f, 1f)]
        private float idleSymbolAlpha = 0.35f;

        [SerializeField]
        [Range(0f, 1f)]
        private float readySymbolAlpha = 1f;

        private void Awake()
        {
            ShowIdle();
        }

        public void ShowIdle()
        {
            ValidateReferences();

            frameProgress.fillAmount = 0f;

            SetSymbolAlpha(
                idleSymbolAlpha
            );
        }

        public void ShowCharging(
            float progress
        )
        {
            ValidateReferences();

            frameProgress.fillAmount =
                Mathf.Clamp01(progress);

            SetSymbolAlpha(
                idleSymbolAlpha
            );
        }

        public void ShowReady()
        {
            ValidateReferences();

            frameProgress.fillAmount = 1f;

            SetSymbolAlpha(
                readySymbolAlpha
            );
        }

        private void SetSymbolAlpha(
            float alpha
        )
        {
            Color color =
                discardSymbol.color;

            color.a = Mathf.Clamp01(alpha);

            discardSymbol.color = color;
        }

        private void ValidateReferences()
        {
            if (frameProgress == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardAreaView)} on " +
                    $"'{name}' has no progress frame assigned."
                );
            }

            if (discardSymbol == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardAreaView)} on " +
                    $"'{name}' has no discard symbol assigned."
                );
            }

            if (ReferenceEquals(
                    frameProgress,
                    discardSymbol
                ))
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardAreaView)} on " +
                    $"'{name}' requires different Images " +
                    "for progress and discard symbol."
                );
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Preview Idle")]
        private void PreviewIdle()
        {
            ShowIdle();
        }

        [ContextMenu("Preview Charging 25%")]
        private void PreviewChargingQuarter()
        {
            ShowCharging(0.25f);
        }

        [ContextMenu("Preview Charging 50%")]
        private void PreviewChargingHalf()
        {
            ShowCharging(0.5f);
        }

        [ContextMenu("Preview Charging 75%")]
        private void PreviewChargingThreeQuarters()
        {
            ShowCharging(0.75f);
        }

        [ContextMenu("Preview Ready")]
        private void PreviewReady()
        {
            ShowReady();
        }
#endif
    }
}