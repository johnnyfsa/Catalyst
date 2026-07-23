using System;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Game.Bootstrap;
using UnityEngine;

namespace Catalyst.Game.Diagnostics
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameSessionBootstrap))]
    public sealed class GameSessionStartupDiagnostics
        : MonoBehaviour
    {
        private GameSessionBootstrap bootstrap;

        private void Awake()
        {
            bootstrap =
                GetComponent<GameSessionBootstrap>();

            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    "GameSessionStartupDiagnostics requires a GameSessionBootstrap on the same GameObject."
                );
            }
        }

        private void Start()
        {
            ValidateBootstrapState();
            LogInitialState();
        }

        private void ValidateBootstrapState()
        {
            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    "GameSessionBootstrap did not create a GameSession."
                );
            }

            if (bootstrap.SessionFlow == null)
            {
                throw new InvalidOperationException(
                    "GameSessionBootstrap did not create a GameSessionFlowService."
                );
            }

            if (bootstrap.ReactionFlow == null)
            {
                throw new InvalidOperationException(
                    "GameSessionBootstrap did not create a ReactionFlowService."
                );
            }

            bootstrap.Session.ValidateState();
        }

        private void LogInitialState()
        {
            GameSession session =
                bootstrap.Session;

            string deliveryProgress =
                FormatDeliveryProgress(session);

            Debug.Log(
                $"Grande Seca startup validated. " +
                $"Session cards: {session.SessionCards.Count}, " +
                $"Hand: {session.Hand.Count}, " +
                $"Deck: {session.Deck.Count}, " +
                $"Delivery zones: {session.DeliveryZones.Count}, " +
                $"Available reactions: " +
                $"{bootstrap.ReactionFlow.AvailableReactions.Count}, " +
                $"Heat: {session.Heat.Amount}, " +
                $"Electricity: {session.Electricity.Amount}, " +
                $"Turn: {session.Turn.TurnNumber}, " +
                $"Phase: {session.Turn.CurrentPhase}, " +
                $"Maximum turns: {FormatTurnLimit(session)}, " +
                $"Delivery progress: {deliveryProgress}, " +
                $"ValidateState: Passed.",
                this
            );
        }

        private static string FormatDeliveryProgress(
            GameSession session
        )
        {
            if (session.DeliveryZones.Count == 0)
            {
                return "No delivery zones";
            }

            if (session.DeliveryZones.Count == 1)
            {
                CardDeliveryZoneRuntime zone =
                    session.DeliveryZones[0];

                return
                    $"{zone.AcceptedDefinition.name} " +
                    $"{zone.CurrentAmount}/" +
                    $"{zone.RequiredAmount}";
            }

            string[] progressEntries =
                new string[
                    session.DeliveryZones.Count
                ];

            for (
                int index = 0;
                index < session.DeliveryZones.Count;
                index++
            )
            {
                CardDeliveryZoneRuntime zone =
                    session.DeliveryZones[index];

                progressEntries[index] =
                    $"{zone.AcceptedDefinition.name} " +
                    $"{zone.CurrentAmount}/" +
                    $"{zone.RequiredAmount}";
            }

            return string.Join(
                ", ",
                progressEntries
            );
        }

        private static string FormatTurnLimit(
            GameSession session
        )
        {
            return session.HasTurnLimit
                ? session.MaximumTurns.Value.ToString()
                : "None";
        }
    }
}