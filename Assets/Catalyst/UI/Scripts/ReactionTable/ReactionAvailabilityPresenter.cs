using System;
using System.Collections.Generic;
using System.Text;
using Catalyst.Game.Bootstrap;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using UnityEngine;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ReactionAvailabilityPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Availability Visuals")]
        [SerializeField]
        private GameObject frameGlow;

        [SerializeField]
        private GameObject frameLightCore;

        [Header("Debug")]
        [SerializeField]
        private bool logDetectedReaction;

        private readonly ReactionMatcherService matcher =
            new ReactionMatcherService();

        private readonly List<ReactionDefinition>
            candidateReactions =
                new List<ReactionDefinition>();

        private ReactionDefinition matchedReaction;

        public bool HasAvailableReaction =>
            candidateReactions.Count > 0;

        public ReactionDefinition MatchedReaction =>
            matchedReaction;

        public IReadOnlyList<ReactionDefinition>
            CandidateReactions => candidateReactions;

        private void Start()
        {
            Refresh();
        }

        [ContextMenu("Refresh Reaction Availability")]
        public void Refresh()
        {
            ValidateReferences();

            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' cannot evaluate reaction " +
                    "availability because the game session " +
                    "has not been initialized."
                );
            }

            if (bootstrap.ReactionFlow == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' cannot evaluate reaction " +
                    "availability because the reaction flow " +
                    "has not been initialized."
                );
            }

            EvaluateCandidates();

            bool isAvailable =
                candidateReactions.Count > 0;

            PresentAvailability(isAvailable);

            if (
                logDetectedReaction
                && candidateReactions.Count > 0
            )
            {
                if (matchedReaction != null)
                {
                    Debug.Log(
                        $"Reaction detected: " +
                        $"{matchedReaction.name}.",
                        this
                    );
                }
                else
                {
                    Debug.Log(
                        $"Ambiguous reaction candidates " +
                        $"detected ({candidateReactions.Count}): " +
                        $"{BuildCandidateDescription()}.",
                        this
                    );
                }
            }
        }

        private void EvaluateCandidates()
        {
            candidateReactions.Clear();
            matchedReaction = null;

            foreach (
                ReactionDefinition reaction
                in bootstrap.ReactionFlow
                    .AvailableReactions
            )
            {
                ReactionMatchResult result =
                    matcher.Match(
                        reaction,
                        bootstrap.Session
                            .ReactionTable
                            .Cards
                    );

                if (result.Succeeded)
                {
                    candidateReactions.Add(reaction);
                    continue;
                }

                if (
                    result.Failure
                    != ReactionMatchFailure
                        .CompositionDoesNotMatch
                )
                {
                    Debug.LogWarning(
                        $"{nameof(ReactionAvailabilityPresenter)} " +
                        $"on '{name}' could not evaluate " +
                        $"reaction '{reaction.name}'. " +
                        $"Failure: {result.Failure}. " +
                        $"Definition failure: " +
                        $"{result.DefinitionFailure}.",
                        this
                    );
                }
            }

            if (candidateReactions.Count == 1)
            {
                matchedReaction =
                    candidateReactions[0];
            }
        }

        private string BuildCandidateDescription()
        {
            var builder = new StringBuilder();

            for (
                int index = 0;
                index < candidateReactions.Count;
                index++
            )
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(
                    candidateReactions[index].name
                );
            }

            return builder.ToString();
        }

        private void PresentAvailability(
            bool isAvailable
        )
        {
            frameGlow.SetActive(isAvailable);
            frameLightCore.SetActive(isAvailable);
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (frameGlow == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no Frame Glow assigned."
                );
            }

            if (frameLightCore == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no Frame Light Core " +
                    "assigned."
                );
            }

            if (ReferenceEquals(
                    frameGlow,
                    frameLightCore
                ))
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' requires different objects " +
                    "for Frame Glow and Frame Light Core."
                );
            }
        }
    }
}