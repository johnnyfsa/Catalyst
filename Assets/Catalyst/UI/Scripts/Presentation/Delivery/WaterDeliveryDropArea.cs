using System;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Delivery;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Audio;
using Catalyst.Game.UI;
using Catalyst.UI.Presentation.Hand;
using Catalyst.UI.Presentation.Interaction;
using Catalyst.UI.Presentation.Objectives;
using Catalyst.UI.Presentation.Session;
using Catalyst.UI.Presentation.ReactionTable;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Delivery
{
    public sealed class WaterDeliveryDropArea :
        MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Drag Source")]
        [SerializeField]
        private HandCardDragPresenter dragPresenter;

        [Header("Objective")]
        [SerializeField]
        private SessionObjectivePresenter
            objectivePresenter;

        [Header("Visual Refresh")]
        [SerializeField]
        private InitialHandPresenter handPresenter;

        [SerializeField]
        private ReactionTablePresenter reactionTablePresenter;

        [SerializeField]
        private WaterTankPresenter waterTankPresenter;

        [SerializeField]
        private GameSessionEndPresenter
            sessionEndPresenter;

        [Header("Audio")]
        [SerializeField]
        private BasicAudioPresenter audioPresenter;

        private bool interactionLocked;

        public void SetInteractionLocked(
            bool locked
        )
        {
            interactionLocked = locked;

            if (
                locked
                && dragPresenter != null
            )
            {
                dragPresenter.SetInteractionAvailable(
                    false
                );
            }
        }

        public void OnPointerEnter(
            PointerEventData eventData
        )
        {
            ValidateReferences();

            bool canDeliver =
                !interactionLocked
                && TryGetDeliverableCard(
                    out _
                );

            dragPresenter.SetInteractionAvailable(
                canDeliver
            );
        }

        public void OnPointerExit(
            PointerEventData eventData
        )
        {
            if (dragPresenter == null)
            {
                return;
            }

            dragPresenter.SetInteractionAvailable(
                false
            );
        }

        public void OnDrop(
            PointerEventData eventData
        )
        {
            ValidateReferences();

            dragPresenter.SetInteractionAvailable(
                false
            );

            if (interactionLocked)
            {
                return;
            }

            if (!TryGetDeliverableCard(
                out CardInstance card
            ))
            {
                return;
            }

            GameSession session =
                bootstrap.Session;

            CardDeliveryZoneRuntime deliveryZone =
                objectivePresenter.DeliveryObjective;

            CardDeliveryResult result =
                bootstrap.SessionFlow.TryDeliverCard(
                    session,
                    card,
                    deliveryZone
                );

            if (!result.Succeeded)
            {
                Debug.LogWarning(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' could not deliver " +
                    $"the dragged card. Failure: " +
                    $"{result.Failure}.",
                    this
                );

                return;
            }

            audioPresenter?.PlayWaterDelivered();
            RefreshAfterDelivery();

            sessionEndPresenter
                .ApplyIfSessionEnded();
        }

        private bool TryGetDeliverableCard(
    out CardInstance card
)
        {
            card = null;

            if (
                bootstrap == null
                || bootstrap.Session == null
                || bootstrap.SessionFlow == null
            )
            {
                return false;
            }

            GameSession session =
                bootstrap.Session;

            if (!session.IsRunning)
            {
                return false;
            }

            if (
                session.Turn.CurrentPhase
                != GamePhase.Main
            )
            {
                return false;
            }

            if (!dragPresenter.TryGetDraggedCard(
                    out card
                ))
            {
                return false;
            }

            if (!HasSupportedDragOrigin())
            {
                return false;
            }

            CardDeliveryZoneRuntime deliveryZone =
                objectivePresenter.DeliveryObjective;

            if (deliveryZone == null)
            {
                return false;
            }

            return ReferenceEquals(
                card.Definition,
                deliveryZone.AcceptedDefinition
            );
        }

        private bool HasSupportedDragOrigin()
        {
            return
                dragPresenter.DragOrigin
                    == CardDragOrigin.Hand
                || dragPresenter.DragOrigin
                    == CardDragOrigin.ReactionTable;
        }

        private void RefreshAfterDelivery()
        {
            handPresenter.PresentInitialHand();

            reactionTablePresenter.Refresh();

            waterTankPresenter.Refresh();

            objectivePresenter.Refresh();
        }

        private void OnDisable()
        {
            if (dragPresenter != null)
            {
                dragPresenter.SetInteractionAvailable(
                    false
                );
            }
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(HandCardDragPresenter)} assigned."
                );
            }

            if (objectivePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(SessionObjectivePresenter)} assigned."
                );
            }

            if (handPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(InitialHandPresenter)} assigned."
                );
            }

            if (waterTankPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(WaterTankPresenter)} assigned."
                );
            }

            if (sessionEndPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionEndPresenter)} assigned."
                );
            }
            if (reactionTablePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterDeliveryDropArea)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ReactionTablePresenter)} assigned."
                );
            }
        }

    }
}