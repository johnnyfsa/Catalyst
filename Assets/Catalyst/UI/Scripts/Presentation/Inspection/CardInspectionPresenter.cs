using System;
using Catalyst.Cards.Presentation;
using Catalyst.Cards.Runtime;
using UnityEngine;

namespace Catalyst.UI.Presentation.Inspection
{
    public sealed class CardInspectionPresenter : MonoBehaviour
    {
        [Header("Overlay")]
        [Tooltip(
            "Root GameObject of the complete card inspection overlay. " +
            "It starts inactive and is activated when a card is inspected."
        )]
        [SerializeField]
        private GameObject overlayRoot;

        [Header("Expanded Card")]
        [Tooltip(
            "ChemicalCardView belonging to the expanded Card prefab."
        )]
        [SerializeField]
        private ChemicalCardView inspectedCardView;

        private GameObject selectedOutline;
        private CardInstance inspectedCard;

        public bool IsOpen =>
            overlayRoot != null &&
            overlayRoot.activeSelf;

        public CardInstance InspectedCard =>
            inspectedCard;

        /// <summary>
        /// Opens the expanded inspection for the specified runtime card
        /// and marks its originating mini-card as selected.
        /// </summary>
        public void Open(
            CardInstance card,
            GameObject selectionOutline
        )
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            ValidateReferences();

            ClearCurrentSelection();

            inspectedCard = card;
            selectedOutline = selectionOutline;

            if (selectedOutline != null)
            {
                selectedOutline.SetActive(true);
            }

            overlayRoot.SetActive(true);

            inspectedCardView.Bind(
                card.Definition
            );
        }

        /// <summary>
        /// Closes the inspection and removes the selection state from
        /// the originating mini-card.
        /// </summary>
        public void Close()
        {
            if (inspectedCardView != null)
            {
                inspectedCardView.Clear();
            }

            ClearCurrentSelection();

            inspectedCard = null;

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void ClearCurrentSelection()
        {
            if (selectedOutline != null)
            {
                selectedOutline.SetActive(false);
            }

            selectedOutline = null;
        }

        private void ValidateReferences()
        {
            if (overlayRoot == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CardInspectionPresenter)} on '{name}' " +
                    "has no overlay root assigned."
                );
            }

            if (inspectedCardView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CardInspectionPresenter)} on '{name}' " +
                    $"has no {nameof(ChemicalCardView)} assigned."
                );
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (overlayRoot == null)
            {
                overlayRoot = gameObject;
            }
        }
#endif
    }
}