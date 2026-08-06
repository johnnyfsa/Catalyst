using System;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Inspection;
using Catalyst.UI.Presentation.Audio;
using UnityEngine;

namespace Catalyst.UI.Presentation.Hand
{
    public sealed class InitialHandPresenter : MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Inspection")]
        [SerializeField]
        private CardInspectionPresenter inspectionPresenter;

        [Header("Drag")]
        [SerializeField]
        private HandCardDragPresenter dragPresenter;

        [Header("Audio")]
        [SerializeField]
        private BasicAudioPresenter audioPresenter;

        [Header("Hand Views")]
        [Tooltip(
            "Assign the hand card views in the same visual order " +
            "as they should represent Session.Hand.Cards."
        )]
        [SerializeField]
        private HandCardView[] cardViews =
            Array.Empty<HandCardView>();

        private bool interactionLocked;

        private void Start()
        {
            PresentInitialHand();
        }

        [ContextMenu("Present Initial Hand")]
        public void PresentInitialHand()
        {
            ValidateReferences();

            GameSession session = bootstrap.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialHandPresenter)} on '{name}' " +
                    "cannot present the hand because the bootstrap " +
                    "has not initialized a session."
                );
            }

            if (cardViews.Length < session.Hand.Count)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialHandPresenter)} on '{name}' " +
                    $"has {cardViews.Length} card views, but the " +
                    $"runtime hand contains {session.Hand.Count} cards."
                );
            }

            for (
                int index = 0;
                index < cardViews.Length;
                index++
            )
            {
                HandCardView cardView =
                    cardViews[index];

                HandCardInteractionView interactionView =
                    cardView.GetComponent<HandCardInteractionView>();

                if (interactionView == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(InitialHandPresenter)} on '{name}' " +
                        $"found no {nameof(HandCardInteractionView)} " +
                        $"on hand card view at index {index}."
                    );
                }

                interactionView.Initialize(
                    inspectionPresenter,
                    dragPresenter,
                    audioPresenter
                );
                interactionView.SetInteractionLocked(
                    interactionLocked
                );

                if (index < session.Hand.Cards.Count)
                {
                    CardInstance card =
                        session.Hand.Cards[index];

                    cardView.gameObject.SetActive(true);
                    cardView.Bind(card);
                }
                else
                {
                    cardView.Clear();
                    cardView.gameObject.SetActive(false);
                }
            }
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialHandPresenter)} on '{name}' " +
                    $"has no {nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (inspectionPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialHandPresenter)} on '{name}' " +
                    $"has no {nameof(CardInspectionPresenter)} assigned."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialHandPresenter)} on '{name}' " +
                    $"has no {nameof(HandCardDragPresenter)} assigned."
                );
            }

            if (cardViews == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialHandPresenter)} on '{name}' " +
                    "has a null card view collection."
                );
            }

            for (
                int index = 0;
                index < cardViews.Length;
                index++
            )
            {
                if (cardViews[index] == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(InitialHandPresenter)} on '{name}' " +
                        $"has no card view assigned at index {index}."
                    );
                }

                for (
                    int comparisonIndex = index + 1;
                    comparisonIndex < cardViews.Length;
                    comparisonIndex++
                )
                {
                    if (ReferenceEquals(
                        cardViews[index],
                        cardViews[comparisonIndex]
                    ))
                    {
                        throw new InvalidOperationException(
                            $"{nameof(InitialHandPresenter)} on '{name}' " +
                            $"contains the same card view at indices " +
                            $"{index} and {comparisonIndex}."
                        );
                    }
                }
            }
        }
        public void SetInteractionLocked(
    bool locked
)
        {
            interactionLocked = locked;

            if (cardViews == null)
            {
                return;
            }

            foreach (
                HandCardView cardView
                in cardViews
            )
            {
                if (cardView == null)
                {
                    continue;
                }

                HandCardInteractionView interactionView =
                    cardView.GetComponent<
                        HandCardInteractionView
                    >();

                if (interactionView != null)
                {
                    interactionView
                        .SetInteractionLocked(
                            locked
                        );
                }
            }
        }
    }
}