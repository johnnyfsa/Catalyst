using System;
using Catalyst.Cards.Runtime.Missions;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Game.Bootstrap;
using UnityEngine;

namespace Catalyst.Game.UI
{
    public sealed class WaterTankPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("View")]
        [SerializeField]
        private WaterTankView view;

        [Header("Mission Objective")]
        [SerializeField]
        [Min(0)]
        private int deliveryObjectiveIndex;

        private CardDeliveryZoneRuntime deliveryZone;

        public CardDeliveryZoneRuntime DeliveryZone =>
            deliveryZone;

        private void Start()
        {
            ResolveDeliveryZone();
            Refresh();
        }

        public void Refresh()
        {
            ValidateReferences();

            if (deliveryZone == null)
            {
                ResolveDeliveryZone();
            }

            view.SetProgress(
                deliveryZone.CurrentAmount,
                deliveryZone.RequiredAmount
            );
        }

        private void ResolveDeliveryZone()
        {
            ValidateReferences();

            GameSession session =
                bootstrap.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterTankPresenter)} on '{name}' " +
                    "cannot resolve the delivery objective because " +
                    "the game session has not been created."
                );
            }

            MissionRuntime mission =
                session.Mission;

            if (mission == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterTankPresenter)} on '{name}' " +
                    "cannot resolve the delivery objective because " +
                    "the session has no mission."
                );
            }

            if (
                deliveryObjectiveIndex < 0
                || deliveryObjectiveIndex
                    >= mission.DeliveryObjectives.Count
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterTankPresenter)} on '{name}' " +
                    $"uses delivery objective index " +
                    $"{deliveryObjectiveIndex}, but the mission has " +
                    $"{mission.DeliveryObjectives.Count} objectives."
                );
            }

            deliveryZone =
                mission.DeliveryObjectives[
                    deliveryObjectiveIndex
                ];
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterTankPresenter)} on '{name}' " +
                    $"has no {nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (view == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaterTankPresenter)} on '{name}' " +
                    $"has no {nameof(WaterTankView)} assigned."
                );
            }
        }
    }
}