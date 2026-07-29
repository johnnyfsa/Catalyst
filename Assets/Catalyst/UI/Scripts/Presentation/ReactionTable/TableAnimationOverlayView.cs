using System;
using UnityEngine;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class TableAnimationOverlayView : MonoBehaviour
    {
        private static readonly int ShowTrigger = Animator.StringToHash("Show");
        private static readonly int HideTrigger = Animator.StringToHash("Hide");

        [SerializeField] private Animator animator;
        [SerializeField] private ScreenReactionFlashView screenReactionFlashView;

        public event Action ReactionMomentReached;
        public event Action SequenceCompleted;

        public void PlayFadeIn()
        {
            animator.ResetTrigger(HideTrigger);
            animator.SetTrigger(ShowTrigger);
        }

        public void PlayFadeOut()
        {
            animator.ResetTrigger(ShowTrigger);
            animator.SetTrigger(HideTrigger);

            screenReactionFlashView.PlayFadeOut();
        }

        // Chamado por Animation Event no FadeIn da mesa.
        public void OnScreenFlashMoment()
        {
            screenReactionFlashView.PlayFadeIn();
        }

        // Chamado por Animation Event no final do FadeIn da mesa.
        public void OnReactionMoment()
        {
            ReactionMomentReached?.Invoke();
        }

        // Chamado por Animation Event no final do FadeOut da mesa.
        public void OnSequenceCompleted()
        {
            SequenceCompleted?.Invoke();
        }
    }
}