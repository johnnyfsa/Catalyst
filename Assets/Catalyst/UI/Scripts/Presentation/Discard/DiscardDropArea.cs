using System;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Hand;
using Catalyst.UI.Presentation.Interaction;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.UI.Presentation.ReactionTable;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Catalyst.UI.Presentation.Discard
{
    public sealed class DiscardDropArea :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IDropHandler
    {
        private enum ChargeState
        {
            Idle = 0,
            Charging = 1,
            Ready = 2
        }

        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Drag Source")]
        [SerializeField]
        private HandCardDragPresenter dragPresenter;

        [Header("Visual")]
        [SerializeField]
        private DiscardAreaView discardAreaView;

        [Header("Timing")]
        [SerializeField]
        [Min(0.01f)]
        private float chargeDuration = 0.75f;

        [Header("Visual Refresh")]
        [SerializeField]
        private InitialHandPresenter handPresenter;

        [SerializeField]
        private ReactionTablePresenter
            reactionTablePresenter;

        [SerializeField]
        private ReactionAvailabilityPresenter
            reactionAvailabilityPresenter;

        private CardInstance trackedCard;

        private CardDragOrigin trackedOrigin =
            CardDragOrigin.None;

        private ChargeState state =
            ChargeState.Idle;

        private bool pointerInside;
        private bool interactionLocked;
        private float elapsedTime;

        private void Awake()
        {
            discardAreaView.ShowIdle();
        }

        private void Update()
        {
            if (state != ChargeState.Charging)
            {
                return;
            }

            if (!IsTrackedDragStillValid())
            {
                CancelCharging();
                return;
            }

            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / chargeDuration
                );

            discardAreaView.ShowCharging(
                progress
            );

            if (progress < 1f)
            {
                return;
            }

            state = ChargeState.Ready;
            discardAreaView.ShowReady();
        }

        public void OnPointerEnter(
            PointerEventData eventData
        )
        {
            ValidateReferences();

            pointerInside = true;

            TryBeginCharging();
        }

        public void OnPointerExit(
            PointerEventData eventData
        )
        {
            pointerInside = false;

            CancelCharging();
        }

        public void OnDrop(
    PointerEventData eventData
)
        {
            ValidateReferences();

            if (
                interactionLocked
                || state != ChargeState.Ready
                || !pointerInside
                || !IsTrackedDragStillValid()
            )
            {
                CancelCharging();
                return;
            }

            CardInstance card =
                trackedCard;

            CardDragOrigin origin =
                trackedOrigin;

            if (!TryResolveSource(
                    origin,
                    out CardZoneRuntime source
                ))
            {
                CancelCharging();
                return;
            }

            GameSession session =
                bootstrap.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' cannot process a discard " +
                    "because the game session has not " +
                    "been initialized."
                );
            }

            ManualDiscardResult result =
                bootstrap.SessionFlow.TryDiscard(
                    session,
                    card,
                    source
                );

            if (!result.Succeeded)
            {
                Debug.LogWarning(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' could not discard the " +
                    $"dragged card. Failure: " +
                    $"{result.Failure}. Movement failure: " +
                    $"{result.MovementFailure}.",
                    this
                );

                CancelCharging();
                return;
            }

            RefreshAfterDiscard(
                origin
            );

            CancelCharging();
        }
        public void SetInteractionLocked(
            bool locked
        )
        {
            interactionLocked = locked;

            if (locked)
            {
                CancelCharging();
            }
        }

        private void TryBeginCharging()
        {
            if (interactionLocked)
            {
                return;
            }

            if (!dragPresenter.TryGetDraggedCard(
                    out CardInstance card
                ))
            {
                return;
            }

            CardDragOrigin origin =
                dragPresenter.DragOrigin;

            if (!TryResolveSource(
                    origin,
                    out CardZoneRuntime source
                ))
            {
                return;
            }

            if (!source.Contains(card))
            {
                return;
            }

            trackedCard = card;
            trackedOrigin = origin;

            elapsedTime = 0f;
            state = ChargeState.Charging;

            discardAreaView.ShowCharging(0f);

            dragPresenter.SetInteractionAvailable(true);
            dragPresenter.SetDiscardHoverVisual(true);
        }

        private bool IsTrackedDragStillValid()
        {
            if (
                interactionLocked
                || !pointerInside
                || trackedCard == null
                || trackedOrigin == CardDragOrigin.None
            )
            {
                return false;
            }

            if (!dragPresenter.TryGetDraggedCard(
                    out CardInstance currentCard
                ))
            {
                return false;
            }

            if (!ReferenceEquals(
                    currentCard,
                    trackedCard
                ))
            {
                return false;
            }

            if (
                dragPresenter.DragOrigin
                != trackedOrigin
            )
            {
                return false;
            }

            if (!TryResolveSource(
                    trackedOrigin,
                    out CardZoneRuntime source
                ))
            {
                return false;
            }

            return source.Contains(
                trackedCard
            );
        }

        private bool TryResolveSource(
            CardDragOrigin origin,
            out CardZoneRuntime source
        )
        {
            source = null;

            GameSession session =
                bootstrap.Session;

            if (session == null)
            {
                return false;
            }

            switch (origin)
            {
                case CardDragOrigin.Hand:
                    source = session.Hand;
                    return true;

                case CardDragOrigin.ReactionTable:
                    source = session.ReactionTable;
                    return true;

                case CardDragOrigin.None:
                    return false;

                default:
                    return false;
            }
        }

        private void CancelCharging()
        {
            trackedCard = null;
            trackedOrigin = CardDragOrigin.None;

            elapsedTime = 0f;
            state = ChargeState.Idle;

            if (discardAreaView != null)
            {
                discardAreaView.ShowIdle();
            }

            if (dragPresenter != null)
            {
                dragPresenter.SetInteractionAvailable(
                    false
                );

                dragPresenter.SetDiscardHoverVisual(
                    false
                );
            }
        }

        private void OnDisable()
        {
            pointerInside = false;
            CancelCharging();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' has no " +
                    $"{nameof(HandCardDragPresenter)} assigned."
                );
            }

            if (discardAreaView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' has no " +
                    $"{nameof(DiscardAreaView)} assigned."
                );
            }

            if (handPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' has no " +
                    $"{nameof(InitialHandPresenter)} assigned."
                );
            }

            if (reactionTablePresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' has no " +
                    $"{nameof(ReactionTablePresenter)} assigned."
                );
            }

            if (reactionAvailabilityPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DiscardDropArea)} on " +
                    $"'{name}' has no " +
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    "assigned."
                );
            }
        }

        private void RefreshAfterDiscard(
    CardDragOrigin origin
)
        {
            switch (origin)
            {
                case CardDragOrigin.Hand:
                    handPresenter.PresentInitialHand();
                    break;

                case CardDragOrigin.ReactionTable:
                    reactionTablePresenter.Refresh();
                    reactionAvailabilityPresenter.Refresh();
                    break;

                case CardDragOrigin.None:
                    throw new InvalidOperationException(
                        "A completed discard must have " +
                        "a valid drag origin."
                    );

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(origin),
                        origin,
                        "Unsupported discard origin."
                    );
            }
        }
    }
}