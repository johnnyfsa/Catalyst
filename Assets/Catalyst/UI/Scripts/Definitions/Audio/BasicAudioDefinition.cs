using UnityEngine;

namespace Catalyst.UI.Definitions.Audio
{
    [CreateAssetMenu(
        fileName = "BasicAudioDefinition",
        menuName = "Catalyst/UI/Audio/Basic Audio Definition"
    )]
    public sealed class BasicAudioDefinition :
        ScriptableObject
    {
        [Header("Cards")]
        [SerializeField]
        private AudioClip cardClick;

        [SerializeField]
        private AudioClip cardPlaced;

        [SerializeField]
        private AudioClip cardDiscarded;

        [Header("Turn")]
        [SerializeField]
        private AudioClip turnPassed;

        [Header("Reaction")]
        [SerializeField]
        private AudioClip reactionCharge;

        [SerializeField]
        private AudioClip reactionExecuted;

        [Header("Result")]
        [SerializeField]
        private AudioClip victory;

        [SerializeField]
        private AudioClip defeat;

        [Header("Interface")]
        [SerializeField]
        private AudioClip buttonClick;

        public AudioClip CardClick =>
            cardClick;

        public AudioClip CardPlaced =>
            cardPlaced;

        public AudioClip CardDiscarded =>
            cardDiscarded;

        public AudioClip TurnPassed =>
            turnPassed;

        public AudioClip ReactionCharge =>
            reactionCharge;

        public AudioClip ReactionExecuted =>
            reactionExecuted;

        public AudioClip Victory =>
            victory;

        public AudioClip Defeat =>
            defeat;

        public AudioClip ButtonClick =>
            buttonClick;
        [SerializeField]
        private AudioClip waterDelivered;

        public AudioClip WaterDelivered =>
            waterDelivered;
    }
}