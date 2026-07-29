using System;
using System.Collections;
using Catalyst.UI.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation
{
    [ExecuteAlways]
    public sealed class ResourceEntryStyleView :
        MonoBehaviour
    {
        public enum OutlineState
        {
            Off = 0,
            Steady = 1,
            Pulsing = 2
        }

        [Header("Editor Preview")]
        [Tooltip(
            "Style used for Editor preview. Runtime bindings may " +
            "replace it when the session UI is initialized."
        )]
        [SerializeField]
        private ResourceEntryStyleAsset initialStyle;

        [SerializeField]
        [Min(0)]
        private int previewAmount;

        [SerializeField]
        private OutlineState previewOutlineState =
            OutlineState.Steady;

        [Header("Visual References")]
        [SerializeField]
        private Image outlineImage;

        [SerializeField]
        private TMP_Text resourceNameText;

        [SerializeField]
        private TMP_Text resourceAmountText;

        [SerializeField]
        private Image resourceIconImage;

        [Header("Outline Pulse")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumPulseAlpha = 0.35f;

        [SerializeField]
        [Min(0.1f)]
        private float pulseCycleDuration = 1f;

        private ResourceEntryStyleAsset boundStyle;

        private Color outlineBaseColor =
            Color.white;

        private Coroutine pulseRoutine;

        public ResourceEntryStyleAsset BoundStyle =>
            boundStyle;

        public OutlineState CurrentOutlineState
        {
            get;
            private set;
        } = OutlineState.Off;

        public void Bind(
            ResourceEntryStyleAsset style,
            int amount
        )
        {
            if (style == null)
            {
                throw new ArgumentNullException(
                    nameof(style)
                );
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Resource amount cannot be negative."
                );
            }

            boundStyle = style;

            ApplyStyle(style);
            SetAmount(amount);

            ApplyCurrentOutlineState();
        }

        public void SetAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Resource amount cannot be negative."
                );
            }

            if (resourceAmountText != null)
            {
                resourceAmountText.text =
                    amount.ToString();
            }
        }

        public void SetOutlineState(
            OutlineState state
        )
        {
            StopPulse();

            CurrentOutlineState = state;

            switch (state)
            {
                case OutlineState.Off:
                    SetOutlineIntensity(0f);
                    break;

                case OutlineState.Steady:
                    SetOutlineIntensity(1f);
                    break;

                case OutlineState.Pulsing:
                    BeginPulse();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(state),
                        state,
                        "Unsupported resource outline state."
                    );
            }
        }

        public void Clear()
        {
            StopPulse();

            boundStyle = null;
            CurrentOutlineState =
                OutlineState.Off;

            SetText(
                resourceNameText,
                string.Empty
            );

            SetText(
                resourceAmountText,
                string.Empty
            );

            SetOutlineIntensity(0f);

            if (resourceIconImage != null)
            {
                resourceIconImage.sprite = null;
                resourceIconImage.enabled = false;
            }
        }

        [ContextMenu("Refresh Preview")]
        public void RefreshPreview()
        {
            if (
                Application.isPlaying
                || initialStyle == null
            )
            {
                return;
            }

            StopPulse();

            ApplyStyle(initialStyle);
            SetAmount(previewAmount);

            ApplyEditorOutlinePreview();
        }

        private void ApplyStyle(
            ResourceEntryStyleAsset style
        )
        {
            SetText(
                resourceNameText,
                style.DisplayName
            );

            outlineBaseColor =
                style.OutlineColor;

            SetColor(
                resourceNameText,
                style.NameTextColor
            );

            SetColor(
                resourceAmountText,
                style.AmountTextColor
            );

            if (resourceIconImage != null)
            {
                resourceIconImage.sprite =
                    style.Icon;

                resourceIconImage.color =
                    style.IconColor;

                resourceIconImage.enabled =
                    style.Icon != null;
            }
        }

        private void ApplyCurrentOutlineState()
        {
            SetOutlineState(
                CurrentOutlineState
            );
        }

        private void BeginPulse()
        {
            if (!Application.isPlaying)
            {
                SetOutlineIntensity(
                    minimumPulseAlpha
                );

                return;
            }

            SetOutlineIntensity(
                minimumPulseAlpha
            );

            pulseRoutine = StartCoroutine(
                PulseOutline()
            );
        }

        private IEnumerator PulseOutline()
        {
            float elapsed = 0f;

            while (true)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float normalizedTime =
                    Mathf.Repeat(
                        elapsed
                        / pulseCycleDuration,
                        1f
                    );

                float wave =
                    0.5f
                    - 0.5f
                    * Mathf.Cos(
                        normalizedTime
                        * Mathf.PI
                        * 2f
                    );

                float intensity =
                    Mathf.Lerp(
                        minimumPulseAlpha,
                        1f,
                        wave
                    );

                SetOutlineIntensity(
                    intensity
                );

                yield return null;
            }
        }

        private void SetOutlineIntensity(
            float intensity
        )
        {
            if (outlineImage == null)
            {
                return;
            }

            Color presentedColor =
                outlineBaseColor;

            presentedColor.a *=
                Mathf.Clamp01(intensity);

            outlineImage.color =
                presentedColor;
        }

        private void StopPulse()
        {
            if (pulseRoutine == null)
            {
                return;
            }

            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        private void ApplyEditorOutlinePreview()
        {
            switch (previewOutlineState)
            {
                case OutlineState.Off:
                    SetOutlineIntensity(0f);
                    break;

                case OutlineState.Steady:
                    SetOutlineIntensity(1f);
                    break;

                case OutlineState.Pulsing:
                    SetOutlineIntensity(
                        minimumPulseAlpha
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SetText(
            TMP_Text textComponent,
            string value
        )
        {
            if (textComponent != null)
            {
                textComponent.text =
                    value ?? string.Empty;
            }
        }

        private static void SetColor(
            Graphic graphic,
            Color color
        )
        {
            if (graphic != null)
            {
                graphic.color = color;
            }
        }

        private void OnDisable()
        {
            StopPulse();
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                RefreshPreview();
            }
        }

        private void OnValidate()
        {
            minimumPulseAlpha =
                Mathf.Clamp01(
                    minimumPulseAlpha
                );

            pulseCycleDuration =
                Mathf.Max(
                    0.1f,
                    pulseCycleDuration
                );

            if (!Application.isPlaying)
            {
                RefreshPreview();
            }
        }
#endif
    }
}