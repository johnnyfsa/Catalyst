using System;
using Catalyst.Cards.Presentation;
using Catalyst.Cards.Runtime;
using UnityEngine;

namespace Catalyst.Game.Presentation
{
    public sealed class HandCardView : MonoBehaviour
    {
        [Header("Card Presentation")]
        [SerializeField]
        private ChemicalCardView chemicalCardView;

        private CardInstance boundCard;

        public CardInstance BoundCard => boundCard;

        public bool HasBoundCard => boundCard != null;

        public Guid? BoundInstanceId =>
            boundCard != null
                ? boundCard.InstanceId
                : null;

        /// <summary>
        /// Associates this visual hand card with a specific
        /// runtime card instance.
        /// </summary>
        public void Bind(CardInstance card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (chemicalCardView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(HandCardView)} on '{name}' has no " +
                    $"{nameof(ChemicalCardView)} assigned."
                );
            }

            boundCard = card;

            chemicalCardView.Bind(
                card.Definition
            );
        }

        /// <summary>
        /// Removes the runtime binding and clears the visual content.
        /// </summary>
        public void Clear()
        {
            boundCard = null;

            if (chemicalCardView != null)
            {
                chemicalCardView.Clear();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (chemicalCardView == null)
            {
                chemicalCardView =
                    GetComponent<ChemicalCardView>();
            }
        }
#endif
    }
}