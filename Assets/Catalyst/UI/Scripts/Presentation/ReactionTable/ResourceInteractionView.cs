using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ResourceEntryInteractionView :
        MonoBehaviour,
        IPointerClickHandler
    {
        [Header("Selection")]
        [SerializeField]
        private ReactionResourceSelection resource;

        [Header("Receiver")]
        [SerializeField]
        private ReactionAvailabilityPresenter
            reactionAvailabilityPresenter;

        public ReactionResourceSelection Resource =>
            resource;

        public void OnPointerClick(
            PointerEventData eventData
        )
        {
            ValidateReferences();

            if (
                eventData.button
                != PointerEventData.InputButton.Left
            )
            {
                return;
            }

            reactionAvailabilityPresenter
                .SelectResource(resource);
        }

        private void ValidateReferences()
        {
            if (
                resource
                == ReactionResourceSelection.None
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(ResourceEntryInteractionView)} " +
                    $"on '{name}' has no selectable resource " +
                    "configured."
                );
            }

            if (
                reactionAvailabilityPresenter
                == null
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(ResourceEntryInteractionView)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    "assigned."
                );
            }
        }
    }
}