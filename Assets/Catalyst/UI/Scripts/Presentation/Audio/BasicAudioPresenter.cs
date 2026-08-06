using Catalyst.UI.Definitions.Audio;
using UnityEngine;

namespace Catalyst.UI.Presentation.Audio
{
    public sealed class BasicAudioPresenter :
        MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField]
        private BasicAudioDefinition definition;

        [Header("Playback")]
        [SerializeField]
        private AudioSource audioSource;

        public void PlayCardClick()
        {
            Play(
                definition != null
                    ? definition.CardClick
                    : null
            );
        }


        public void PlayCardPlaced()
        {
            Play(
                definition != null
                    ? definition.CardPlaced
                    : null
            );
        }

        public void PlayCardDiscarded()
        {
            Play(
                definition != null
                    ? definition.CardDiscarded
                    : null
            );
        }

        public void PlayTurnPassed()
        {
            Play(
                definition != null
                    ? definition.TurnPassed
                    : null
            );
        }

        public void PlayReactionCharge()
        {
            Play(
                definition != null
                    ? definition.ReactionCharge
                    : null
            );
        }

        public void PlayReactionExecuted()
        {
            Play(
                definition != null
                    ? definition.ReactionExecuted
                    : null
            );
        }

        public void PlayVictory()
        {
            Play(
                definition != null
                    ? definition.Victory
                    : null
            );
        }

        public void PlayDefeat()
        {
            Play(
                definition != null
                    ? definition.Defeat
                    : null
            );
        }

        public void PlayButtonClick()
        {
            Play(
                definition != null
                    ? definition.ButtonClick
                    : null
            );
        }

        public void PlayWaterDelivered()
        {
            Play(
                definition != null
                    ? definition.WaterDelivered
                    : null
            );
        }

        private void Play(
            AudioClip clip
        )
        {
            if (
                audioSource == null
                || clip == null
            )
            {
                return;
            }

            audioSource.PlayOneShot(
                clip
            );
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (audioSource == null)
            {
                audioSource =
                    GetComponent<AudioSource>();
            }
        }
        [ContextMenu("Preview Card Click")]
        private void PreviewCardClick()
        {
            PlayCardClick();
        }

        [ContextMenu("Preview Reaction Charge")]
        private void PreviewReactionCharge()
        {
            PlayReactionCharge();
        }

        [ContextMenu("Preview Reaction Executed")]
        private void PreviewReactionExecuted()
        {
            PlayReactionExecuted();
        }
#endif
    }
}