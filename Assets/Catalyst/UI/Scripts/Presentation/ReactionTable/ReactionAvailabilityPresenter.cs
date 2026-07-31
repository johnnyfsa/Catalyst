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

        private ReactionResourceSelection
            selectedResource =
                ReactionResourceSelection.None;

        public bool HasAvailableReaction =>
            candidateReactions.Count > 0;

        public ReactionDefinition MatchedReaction =>
            matchedReaction;

        public ReactionDefinition ResolvedReaction =>
            matchedReaction;

        public IReadOnlyList<ReactionDefinition>
            CandidateReactions => candidateReactions;

        public ReactionResourceSelection
            SelectedResource => selectedResource;

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

        private bool interactionLocked;

        public bool InteractionLocked =>
            interactionLocked;

        public void SetInteractionLocked(
            bool locked
        )
        {
            interactionLocked = locked;

            if (locked)
            {
                reactionButton.interactable = false;
                reactionButtonVisual.SetInactive();
            }
            else
            {
                Refresh();
            }
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

            ClearExplicitSelection();
            EvaluateCandidates();

            bool isAvailable =
                candidateReactions.Count > 0;

            PresentAvailability(
                isAvailable
            );

            PresentSelectionState();

            LogAvailability();
        }

        public void SelectResource(
            ReactionResourceSelection resource
        )
        {
            ValidateReferences();
            if (interactionLocked)
            {
                return;
            }

            if (
                resource
                == ReactionResourceSelection.None
            )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resource),
                    resource,
                    "A concrete reaction resource must be selected."
                );
            }

            if (
                CurrentState
                    != PresentationState
                        .AwaitingResourceSelection
                && CurrentState
                    != PresentationState
                        .ResolvedByResourceSelection
                && CurrentState
                    != PresentationState
                        .InsufficientResources
            )
            {
                return;
            }

            if (candidateReactions.Count <= 1)
            {
                return;
            }

            if (!IsSelectableResource(resource))
            {
                return;
            }

            ReactionDefinition resolvedReaction =
                FindCandidateForResource(resource);

            if (resolvedReaction == null)
            {
                Debug.LogWarning(
                    $"{nameof(ReactionAvailabilityPresenter)} " +
                    $"on '{name}' could not resolve a reaction " +
                    $"for selected resource '{resource}'.",
                    this
                );

                return;
            }

            selectedResource = resource;
            matchedReaction = resolvedReaction;

            PresentSelectedReaction(
                resolvedReaction,
                resource
            );

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

            PresentMultipleCandidates();
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

            bool canReact =
                HasEnoughResources(reaction);

            CanReact = canReact;

            CurrentState = canReact
                ? PresentationState
                    .ResolvedAutomatically
                : PresentationState
                    .InsufficientResources;

            PresentReactionButton(canReact);
        }

        private void PresentAmbiguousReactions()
        {
            matchedReaction = null;
            selectedResource =
                ReactionResourceSelection.None;

            CanReact = false;

            bool heatIsCandidate =
                IsSelectableResource(
                    ReactionResourceSelection.Heat
                );

            bool electricityIsCandidate =
                IsSelectableResource(
                    ReactionResourceSelection
                        .Electricity
                );

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
                electricityEntry.SetOutlineState(
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
                    "distinguished by Heat or Electricity.",
                    this
                );
            }
        }

        private void PresentSelectedReaction(
            ReactionDefinition reaction,
            ReactionResourceSelection resource
        )
        {
            ResetResourceEntries();

            SetResourceOutline(
                resource,
                ResourceEntryStyleView
                    .OutlineState
                    .Steady
            );

            bool canReact =
                HasEnoughResources(reaction);

            CanReact = canReact;

            CurrentState = canReact
                ? PresentationState
                    .ResolvedByResourceSelection
                : PresentationState
                    .InsufficientResources;

            PresentReactionButton(canReact);
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
                electricityEntry.SetOutlineState(
                    ResourceEntryStyleView
                        .OutlineState
                        .Steady
                );
            }
        }

        private bool HasEnoughResources(
            ReactionDefinition reaction
        )
        {
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
                        reaction.RequiredElectricity
                    );

            return hasEnoughHeat
                && hasEnoughElectricity;
        }

        private bool IsSelectableResource(
            ReactionResourceSelection resource
        )
        {
            return FindCandidateForResource(
                resource
            ) != null;
        }

        private ReactionDefinition
            FindCandidateForResource(
                ReactionResourceSelection resource
            )
        {
            ReactionDefinition result = null;

            foreach (
                ReactionDefinition candidate
                in candidateReactions
            )
            {
                if (
                    !RequiresSelectedResource(
                        candidate,
                        resource
                    )
                )
                {
                    continue;
                }

                if (result != null)
                {
                    Debug.LogWarning(
                        $"{nameof(ReactionAvailabilityPresenter)} " +
                        $"on '{name}' found more than one " +
                        $"candidate reaction for resource " +
                        $"'{resource}'. The resource alone " +
                        "cannot uniquely resolve the reaction.",
                        this
                    );

                    return null;
                }

                result = candidate;
            }

            return result;
        }

        private static bool RequiresSelectedResource(
            ReactionDefinition reaction,
            ReactionResourceSelection resource
        )
        {
            switch (resource)
            {
                case ReactionResourceSelection.Heat:
                    return reaction.RequiredHeat > 0
                        && reaction
                            .RequiredElectricity == 0;

                case ReactionResourceSelection
                        .Electricity:
                    return reaction
                            .RequiredElectricity > 0
                        && reaction.RequiredHeat == 0;

                case ReactionResourceSelection.None:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(resource),
                        resource,
                        "Unsupported reaction resource."
                    );
            }
        }

        private void SetResourceOutline(
            ReactionResourceSelection resource,
            ResourceEntryStyleView.OutlineState state
        )
        {
            switch (resource)
            {
                case ReactionResourceSelection.Heat:
                    heatEntry.SetOutlineState(state);
                    break;

                case ReactionResourceSelection
                        .Electricity:
                    electricityEntry
                        .SetOutlineState(state);
                    break;

                case ReactionResourceSelection.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(resource),
                        resource,
                        "Unsupported reaction resource."
                    );
            }
        }

        private void ClearExplicitSelection()
        {
            selectedResource =
                ReactionResourceSelection.None;

            matchedReaction = null;
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
            bool shouldEnable =
                canReact && !interactionLocked;

            reactionButton.interactable =
                shouldEnable;

            if (shouldEnable)
            {
                reactionButtonVisual.SetActive();
            }
            else
            {
                reactionButtonVisual.SetInactive();
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
            if (!logDetectedReaction)
            {
                return;
            }

            if (candidateReactions.Count == 0)
            {
                Debug.Log(
                    "No compatible reaction detected.",
                    this
                );

                return;
            }

            if (matchedReaction != null)
            {
                Debug.Log(
                    $"Resolved reaction: " +
                    $"{matchedReaction.name}. " +
                    $"Selection: {selectedResource}. " +
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
                    candidateReactions[index].name
                );
            }

            return builder.ToString();
        }

        private void OnDisable()
        {
            selectedResource =
                ReactionResourceSelection.None;

            matchedReaction = null;
            CanReact = false;

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
                electricityEntry.SetOutlineState(
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
                reactionButtonVisual.SetInactive();
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
        private bool CanExecuteCandidate(ReactionDefinition reaction)
        {
            return reaction != null
                && HasEnoughResources(reaction);
        }

        private void PresentMultipleCandidates()
        {
            ReactionDefinition onlyExecutableReaction =
                FindOnlyExecutableCandidate();

            if (onlyExecutableReaction != null)
            {
                PresentSingleReaction(
                    onlyExecutableReaction
                );

                return;
            }

            PresentAmbiguousReactions();
        }

        private ReactionDefinition
    FindOnlyExecutableCandidate()
        {
            ReactionDefinition result = null;

            foreach (
                ReactionDefinition candidate
                in candidateReactions
            )
            {
                if (!HasEnoughResources(candidate))
                {
                    continue;
                }

                if (result != null)
                {
                    return null;
                }

                result = candidate;
            }

            return result;
        }
    }
}