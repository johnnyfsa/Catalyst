using System;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Definitions.Results;
using Catalyst.UI.Presentation.Objectives;
using UnityEngine;

namespace Catalyst.UI.Presentation.GameResult
{
    public sealed class GameResultPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Phase Result Context")]
        [SerializeField]
        private GameResultContext resultContext;

        [Header("Objective Source")]
        [SerializeField]
        private SessionObjectivePresenter objectivePresenter;

        [Header("View")]
        [SerializeField]
        private GameResultOverlayView overlayView;

        private GameSession presentedSession;

        public bool HasPresentedResult =>
            presentedSession != null;

        public bool PresentEndedSession()
        {
            ValidateReferences();

            GameSession session =
                bootstrap.Session;

            if (
                session == null
                || !session.HasEnded
            )
            {
                return false;
            }

            if (ReferenceEquals(
                presentedSession,
                session
            ))
            {
                return false;
            }

            GameResultDefinition definition =
                ResolveDefinition(
                    session.EndReason
                );

            GameResultSummary summary =
                BuildSummary(
                    session,
                    definition
                );

            overlayView.Present(
                definition,
                summary
            );

            presentedSession =
                session;

            return true;
        }

        private GameResultDefinition ResolveDefinition(
            GameSessionEndReason endReason
        )
        {
            if (
                resultContext.DefinitionLibrary.TryGet(
                    endReason,
                    out GameResultDefinition definition
                )
            )
            {
                return definition;
            }

            throw new InvalidOperationException(
                $"{nameof(GameResultPresenter)} " +
                $"on '{name}' could not find a " +
                $"{nameof(GameResultDefinition)} for " +
                $"session end reason '{endReason}'."
            );
        }

        private GameResultSummary BuildSummary(
            GameSession session,
            GameResultDefinition definition
        )
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            CardDeliveryZoneRuntime objective =
                objectivePresenter
                    .GetDeliveryObjective();

            if (objective == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultPresenter)} " +
                    $"on '{name}' cannot build the result " +
                    "summary because the session objective " +
                    "could not be resolved."
                );
            }

            string objectiveValue =
                FormatObjectiveValue(
                    objective
                );

            string sessionValue =
                FormatSessionValue(
                    session
                );

            return new GameResultSummary(
                definition.ObjectiveSummaryLabel,
                objectiveValue,
                definition.SessionSummaryLabel,
                sessionValue
            );
        }

        private static string FormatSessionValue(
            GameSession session
        )
        {
            switch (session.EndReason)
            {
                case GameSessionEndReason
                    .MissionCompleted:
                    return FormatTurnProgress(
                        session
                    );

                case GameSessionEndReason
                    .DeckOut:
                    return session.Deck.Count
                        .ToString();

                case GameSessionEndReason
                    .MaxTurnsReached:
                    return FormatMaximumTurns(
                        session
                    );

                case GameSessionEndReason.None:
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(session.EndReason),
                        session.EndReason,
                        "Unsupported session end reason."
                    );
            }
        }

        private static string FormatObjectiveValue(
            CardDeliveryZoneRuntime objective
        )
        {
            string unit =
                objective.AcceptedDefinition.Formula;

            if (string.IsNullOrWhiteSpace(unit))
            {
                unit =
                    objective.AcceptedDefinition
                        .DisplayName;
            }

            string progress =
                $"{objective.CurrentAmount}/" +
                $"{objective.RequiredAmount}";

            return string.IsNullOrWhiteSpace(unit)
                ? progress
                : $"{progress} {unit}";
        }

        private static string FormatTurnProgress(
            GameSession session
        )
        {
            if (!session.HasTurnLimit)
            {
                return session.Turn.TurnNumber
                    .ToString();
            }

            int maximumTurns =
                session.MaximumTurns.Value;

            int completedTurn =
                Math.Min(
                    session.Turn.TurnNumber,
                    maximumTurns
                );

            return
                $"{completedTurn}/{maximumTurns}";
        }

        private static string FormatMaximumTurns(
            GameSession session
        )
        {
            if (!session.HasTurnLimit)
            {
                throw new InvalidOperationException(
                    "A session cannot end with " +
                    $"{GameSessionEndReason.MaxTurnsReached} " +
                    "without having a configured turn limit."
                );
            }

            return session.MaximumTurns.Value
                .ToString();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw MissingReference(
                    nameof(GameSessionBootstrap)
                );
            }

            if (resultContext == null)
            {
                throw MissingReference(
                    nameof(GameResultContext)
                );
            }

            if (objectivePresenter == null)
            {
                throw MissingReference(
                    nameof(SessionObjectivePresenter)
                );
            }

            if (overlayView == null)
            {
                throw MissingReference(
                    nameof(GameResultOverlayView)
                );
            }

            resultContext.ValidateConfiguration();
        }

        private InvalidOperationException MissingReference(
            string referenceName
        )
        {
            return new InvalidOperationException(
                $"{nameof(GameResultPresenter)} " +
                $"on '{name}' has no " +
                $"{referenceName} assigned."
            );
        }
    }
}