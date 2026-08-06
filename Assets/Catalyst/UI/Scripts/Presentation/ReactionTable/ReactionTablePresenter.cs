using System;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Hand;
using Catalyst.UI.Presentation.Inspection;
using Catalyst.UI.Presentation.Audio;
using UnityEngine;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ReactionTablePresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Shared Interaction")]
        [SerializeField]
        private CardInspectionPresenter
            inspectionPresenter;

        [SerializeField]
        private HandCardDragPresenter dragPresenter;

        [SerializeField]
        private BasicAudioPresenter audioPresenter;

        [Header("Reaction Table Views")]
        [Tooltip(
            "Assign the card views in the same visual order " +
            "as Session.ReactionTable.Cards."
        )]
        [SerializeField]
        private HandCardView[] cardViews =
            Array.Empty<HandCardView>();

        [Tooltip(
            "Assign the interaction views in the same order " +
            "as the corresponding card views."
        )]
        [SerializeField]
        private ReactionTableCardInteractionView[]
            interactionViews =
                Array.Empty<
                    ReactionTableCardInteractionView
                >();

        public int VisualCapacity =>
            cardViews?.Length ?? 0;

        private bool interactionLocked;

        private void Start()
        {
            InitializeInteractionViews();
            Refresh();
        }

        public bool CanPresentAdditionalCard()
        {
            if (
                bootstrap == null
                || bootstrap.Session == null
            )
            {
                return false;
            }

            return bootstrap.Session.ReactionTable.Count
                < VisualCapacity;
        }

        [ContextMenu("Refresh Reaction Table")]
        public void Refresh()
        {
            ValidateReferences();

            GameSession session = bootstrap.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' cannot present the reaction " +
                    "table because the bootstrap has not " +
                    "initialized a session."
                );
            }

            int runtimeCardCount =
                session.ReactionTable.Count;

            if (runtimeCardCount > cardViews.Length)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has {cardViews.Length} card " +
                    $"views, but the runtime reaction table " +
                    $"contains {runtimeCardCount} cards."
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

                if (
                    index
                    < session.ReactionTable.Cards.Count
                )
                {
                    CardInstance card =
                        session.ReactionTable.Cards[index];

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

        private void InitializeInteractionViews()
        {
            ValidateReferences();

            for (
                int index = 0;
                index < interactionViews.Length;
                index++
            )
            {
                interactionViews[index].Initialize(
                    inspectionPresenter,
                    dragPresenter,
                    audioPresenter
                );
                interactionViews[index].SetInteractionLocked(
                    interactionLocked
                );
            }
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (inspectionPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has no " +
                    $"{nameof(CardInspectionPresenter)} assigned."
                );
            }

            if (dragPresenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has no " +
                    $"{nameof(HandCardDragPresenter)} assigned."
                );
            }

            if (cardViews == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has a null card view collection."
                );
            }

            if (interactionViews == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has a null interaction view " +
                    "collection."
                );
            }

            if (
                cardViews.Length
                != interactionViews.Length
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has {cardViews.Length} card " +
                    $"views but {interactionViews.Length} " +
                    "interaction views. Both collections " +
                    "must follow the same slot order."
                );
            }

            for (
                int index = 0;
                index < cardViews.Length;
                index++
            )
            {
                ValidateCardView(index);
                ValidateInteractionView(index);
            }
        }

        private void ValidateCardView(
            int index
        )
        {
            HandCardView cardView =
                cardViews[index];

            if (cardView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has no card view assigned " +
                    $"at index {index}."
                );
            }

            for (
                int comparisonIndex = index + 1;
                comparisonIndex < cardViews.Length;
                comparisonIndex++
            )
            {
                if (ReferenceEquals(
                    cardView,
                    cardViews[comparisonIndex]
                ))
                {
                    throw new InvalidOperationException(
                        $"{nameof(ReactionTablePresenter)} on " +
                        $"'{name}' contains the same card " +
                        $"view at indices {index} and " +
                        $"{comparisonIndex}."
                    );
                }
            }
        }

        private void ValidateInteractionView(
            int index
        )
        {
            ReactionTableCardInteractionView
                interactionView =
                    interactionViews[index];

            if (interactionView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has no interaction view " +
                    $"assigned at index {index}."
                );
            }

            HandCardView cardView =
                cardViews[index];

            bool belongsToSameCard =
                interactionView.transform
                    == cardView.transform
                || interactionView.transform.IsChildOf(
                    cardView.transform
                )
                || cardView.transform.IsChildOf(
                    interactionView.transform
                );

            if (!belongsToSameCard)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' requires card view and " +
                    $"interaction view at index {index} " +
                    "to belong to the same card hierarchy."
                );
            }

            for (
                int comparisonIndex = index + 1;
                comparisonIndex
                    < interactionViews.Length;
                comparisonIndex++
            )
            {
                if (ReferenceEquals(
                    interactionView,
                    interactionViews[comparisonIndex]
                ))
                {
                    throw new InvalidOperationException(
                        $"{nameof(ReactionTablePresenter)} on " +
                        $"'{name}' contains the same " +
                        "interaction view at indices " +
                        $"{index} and {comparisonIndex}."
                    );
                }
            }
        }
        public void SetInteractionLocked(
    bool locked
)
        {
            interactionLocked = locked;

            if (interactionViews == null)
            {
                return;
            }

            foreach (
                ReactionTableCardInteractionView
                    interactionView
                in interactionViews
            )
            {
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