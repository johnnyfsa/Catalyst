using System;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Presentation.Hand;
using UnityEngine;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ReactionTablePresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Reaction Table Views")]
        [Tooltip(
            "Assign the card views in the same visual order " +
            "as Session.ReactionTable.Cards."
        )]
        [SerializeField]
        private HandCardView[] cardViews =
            Array.Empty<HandCardView>();

        public int VisualCapacity =>
            cardViews?.Length ?? 0;

        private void Start()
        {
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

            // Temporary diagnostic if needed:
            // Debug.Log(
            //     $"{nameof(ReactionTablePresenter)} refreshed. " +
            //     $"Runtime cards: {runtimeCardCount}, " +
            //     $"Visual slots: {cardViews.Length}.",
            //     this
            // );
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

            if (cardViews == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionTablePresenter)} on " +
                    $"'{name}' has a null card view collection."
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
                        cardViews[index],
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
        }
    }
}