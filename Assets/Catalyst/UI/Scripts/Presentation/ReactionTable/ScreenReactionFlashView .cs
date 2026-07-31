using UnityEngine;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ScreenReactionFlashView : MonoBehaviour
    {
        private static readonly int ShowTrigger = Animator.StringToHash("Show");
        private static readonly int HideTrigger = Animator.StringToHash("Hide");

        [SerializeField] private Animator animator;

        public void PlayFadeIn()
        {
            animator.SetTrigger(ShowTrigger);
        }

        public void PlayFadeOut()
        {
            animator.SetTrigger(HideTrigger);
        }
    }
}