using Catalyst.UI.Presentation.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ActionButtonVisual :
        MonoBehaviour
    {
        public enum VisualState
        {
            Inactive,
            Active
        }

        [Header("Interaction")]
        [SerializeField]
        private Button button;

        [SerializeField]
        private BasicAudioPresenter audioPresenter;

        [Header("Optional illuminated objects")]
        [SerializeField]
        private GameObject[] activeOnlyObjects;

        [Header("Content")]
        [SerializeField]
        private TMP_Text label;

        [SerializeField]
        private Image icon;

        [Header("Inactive")]
        [SerializeField]
        private Color inactiveLabelColor =
            new(0.45f, 0.48f, 0.5f, 1f);

        [SerializeField]
        private Color inactiveIconColor =
            new(0.45f, 0.48f, 0.5f, 1f);

        [Header("Active")]
        [SerializeField]
        private Color activeLabelColor =
            Color.white;

        [SerializeField]
        private Color activeIconColor =
            Color.white;

        [Header("Editor Preview")]
        [SerializeField]
        private VisualState previewState =
            VisualState.Inactive;

        public VisualState CurrentState
        {
            get;
            private set;
        }

        private void Awake()
        {
            ApplyState(
                previewState
            );
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(
                    PlayButtonClick
                );
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(
                    PlayButtonClick
                );
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (button == null)
            {
                button =
                    GetComponent<Button>();
            }

            ApplyState(
                previewState
            );
        }
#endif

        public void SetInactive()
        {
            ApplyState(
                VisualState.Inactive
            );
        }

        public void SetActive()
        {
            ApplyState(
                VisualState.Active
            );
        }

        public void ApplyState(
            VisualState state
        )
        {
            CurrentState = state;

            bool isActive =
                state == VisualState.Active;

            if (activeOnlyObjects != null)
            {
                foreach (
                    GameObject visual
                    in activeOnlyObjects
                )
                {
                    if (
                        visual != null
                        && visual.activeSelf
                            != isActive
                    )
                    {
                        visual.SetActive(
                            isActive
                        );
                    }
                }
            }

            if (label != null)
            {
                label.color =
                    isActive
                        ? activeLabelColor
                        : inactiveLabelColor;
            }

            if (icon != null)
            {
                icon.color =
                    isActive
                        ? activeIconColor
                        : inactiveIconColor;
            }
        }

        private void PlayButtonClick()
        {
            audioPresenter?
                .PlayButtonClick();
        }
    }
}