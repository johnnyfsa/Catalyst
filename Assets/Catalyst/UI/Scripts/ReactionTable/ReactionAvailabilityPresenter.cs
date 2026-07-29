using System;
using System.Collections.Generic;
using System.Text;
using Catalyst.Game.Bootstrap;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using Catalyst.UI.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Catalyst.UI.Presentation.ReactionTable
{
    public sealed class ReactionAvailabilityPresenter :
        MonoBehaviour
    {
        public enum PresentationState
        {
            NoMatch = 0,
            ResolvedAutomatically = 1,
            AwaitingResourceSelection = 2,
            ResolvedByResourceSelection = 3,
            InsufficientResources = 4
        }

        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Availability Visuals")]
        [SerializeField]
        private GameObject frameGlow;

        [SerializeField]
        private GameObject frameLightCore;

        [Header("Resource Entries")]
        [SerializeField]
        private ResourceEntryStyleView heatEntry;

        [SerializeField]
        private ResourceEntryStyleView
            electricityEntry;

        [Header("Reaction Action")]
        [SerializeField]
        private Button reactionButton;

        [SerializeField]
        private ActionButtonVisual
            reactionButtonVisual;

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

        public ReactionDefinition ResolvedReaction =>
            matchedReaction;

        public IReadOnlyList<ReactionDefinition>
            CandidateReactions => candidateReactions;

        public PresentationState CurrentState
        {
            get;
            private set;
        } = PresentationState.NoMatch;

        public bool CanReact
        {
            get;
            private set;
        }

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

            PresentAvailability(
                isAvailable
            );

            PresentSelectionState();

            LogAvailability();
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
                    candidateReactions.Add(
                        reaction
                    );

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

        private void PresentSelectionState()
        {
            ResetResourceEntries();

            CanReact = false;

            if (candidateReactions.Count == 0)
            {
                PresentNoMatch();
                return;
            }

            if (candidateReactions.Count == 1)
            {
                PresentSingleReaction(
                    candidateReactions[0]
                );

                return;
            }

            PresentAmbiguousReactions();
        }

        private void PresentNoMatch()
        {
            matchedReaction = null;

            CurrentState =
                PresentationState.NoMatch;

            PresentReactionButton(false);
        }

        private void PresentSingleReaction(
            ReactionDefinition reaction
        )
        {
            matchedReaction = reaction;

            PresentRequiredResources(
                reaction
            );

            bool hasEnoughHeat =
                bootstrap.Session
                    .Heat
                    .CanConsume(
                        reaction.RequiredHeat
                    );

            bool hasEnoughElectricity =
                bootstrap.Session
                    .Electricity
                    .CanConsume(
                        reaction
                            .RequiredElectricity
                    );

            CanReact =
                hasEnoughHeat
                && hasEnoughElectricity;

            CurrentState = CanReact
                ? PresentationState
                    .ResolvedAutomatically
                : PresentationState
                    .InsufficientResources;

            PresentReactionButton(
                CanReact
            );
        }

        private void PresentRequiredResources(
            ReactionDefinition reaction
        )
        {
            if (reaction.RequiredHeat > 0)
            {
                heatEntry.SetOutlineState(
                    ResourceEntryStyleView
                        .OutlineState
                        .Steady
                );
            }

            if (
                reaction.RequiredElectricity
                > 0
            )
            {
                electricityEntry
                    .SetOutlineState(
                        ResourceEntryStyleView
                            .OutlineState
                            .Steady
                    );
            }
        }

        private void PresentAmbiguousReactions()
        {
            matchedReaction = null;
            CanReact = false;

            bool heatIsCandidate = false;
            bool electricityIsCandidate = false;

            foreach (
                ReactionDefinition candidate
                in candidateReactions
            )
            {
                if (
                    IsHeatSelectableCandidate(
                        candidate
                    )
                )
                {
                    heatIsCandidate = true;
                }

                if (
                    IsElectricitySelectableCandidate(
                        candidate
                    )
                )
                {
                    electricityIsCandidate = true;
                }
            }

            if (heatIsCandidate)
            {
                heatEntry.SetOutlineState(
                    ResourceEntryStyleView
                        .OutlineState
                        .Pulsing
                );
            }

            if (electricityIsCandidate)
            {
                electricityEntry
                    .SetOutlineState(
                        ResourceEntryStyleView
                            .OutlineState
                            .Pulsing
                    );
            }

            CurrentState =
                PresentationState
                    .AwaitingResourceSelection;

            PresentReactionButton(false);

            if (
                !heatIsCandidate
                && !electricityIsCandidate
            )
            {
                Debug.LogWarning(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' detected " +
                    $"{candidateReactions.Count} compatible " +
                    "reactions, but none can currently be " +
                    "distinguished by an exclusive Heat or " +
                    "Electricity requirement.",
                    this
                );
            }
        }

        private static bool
            IsHeatSelectableCandidate(
                ReactionDefinition reaction
            )
        {
            return reaction.RequiredHeat > 0
                && reaction
                    .RequiredElectricity == 0;
        }

        private static bool
            IsElectricitySelectableCandidate(
                ReactionDefinition reaction
            )
        {
            return reaction
                    .RequiredElectricity > 0
                && reaction.RequiredHeat == 0;
        }

        private void ResetResourceEntries()
        {
            heatEntry.SetOutlineState(
                ResourceEntryStyleView
                    .OutlineState
                    .Off
            );

            electricityEntry.SetOutlineState(
                ResourceEntryStyleView
                    .OutlineState
                    .Off
            );
        }

        private void PresentReactionButton(
            bool canReact
        )
        {
            reactionButton.interactable =
                canReact;

            if (canReact)
            {
                reactionButtonVisual
                    .SetActive();
            }
            else
            {
                reactionButtonVisual
                    .SetInactive();
            }
        }

        private void PresentAvailability(
            bool isAvailable
        )
        {
            frameGlow.SetActive(
                isAvailable
            );

            frameLightCore.SetActive(
                isAvailable
            );
        }

        private void LogAvailability()
        {
            if (
                !logDetectedReaction
                || candidateReactions.Count == 0
            )
            {
                return;
            }

            if (matchedReaction != null)
            {
                Debug.Log(
                    $"Reaction detected: " +
                    $"{matchedReaction.name}. " +
                    $"State: {CurrentState}. " +
                    $"Can react: {CanReact}.",
                    this
                );

                return;
            }

            Debug.Log(
                $"Ambiguous reaction candidates " +
                $"detected " +
                $"({candidateReactions.Count}): " +
                $"{BuildCandidateDescription()}. " +
                $"State: {CurrentState}.",
                this
            );
        }

        private string BuildCandidateDescription()
        {
            var builder =
                new StringBuilder();

            for (
                int index = 0;
                index
                    < candidateReactions.Count;
                index++
            )
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(
                    candidateReactions[
                        index
                    ].name
                );
            }

            return builder.ToString();
        }

        private void OnDisable()
        {
            if (heatEntry != null)
            {
                heatEntry.SetOutlineState(
                    ResourceEntryStyleView
                        .OutlineState
                        .Off
                );
            }

            if (electricityEntry != null)
            {
                electricityEntry
                    .SetOutlineState(
                        ResourceEntryStyleView
                            .OutlineState
                            .Off
                    );
            }

            if (reactionButton != null)
            {
                reactionButton.interactable =
                    false;
            }

            if (reactionButtonVisual != null)
            {
                reactionButtonVisual
                    .SetInactive();
            }
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

            if (
                ReferenceEquals(
                    frameGlow,
                    frameLightCore
                )
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' requires different objects " +
                    "for Frame Glow and Frame Light Core."
                );
            }

            if (heatEntry == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no Heat Entry assigned."
                );
            }

            if (electricityEntry == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no Electricity Entry " +
                    "assigned."
                );
            }

            if (
                ReferenceEquals(
                    heatEntry,
                    electricityEntry
                )
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' requires different views " +
                    "for Heat and Electricity."
                );
            }

            if (reactionButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no reaction Button " +
                    "assigned."
                );
            }

            if (reactionButtonVisual == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(ActionButtonVisual)} assigned."
                );
            }
        }
    }
}