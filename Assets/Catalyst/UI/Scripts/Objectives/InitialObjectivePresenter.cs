using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Game.Bootstrap;
using UnityEngine;

namespace Catalyst.UI.Presentation.Objectives
{
    public sealed class InitialObjectivePresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Objective Identity")]
        [Tooltip(
            "Card definition used to locate the corresponding " +
            "delivery objective in the runtime mission."
        )]
        [SerializeField]
        private CardDefinition acceptedDefinition;

        [Header("Objective Presentation")]
        [SerializeField]
        private ObjectiveEntryView objectiveEntryView;

        [SerializeField]
        private string objectiveTitle = "PRODUCE WATER";

        [SerializeField]
        [TextArea]
        private string objectiveDescription =
            "Deliver water to the reservoir.";

        [SerializeField]
        private Sprite objectiveIcon;

        private void Start()
        {
            PresentInitialObjective();
        }

        [ContextMenu("Present Initial Objective")]
        public void PresentInitialObjective()
        {
            ValidateReferences();

            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialObjectivePresenter)} on '{name}' " +
                    "cannot present the objective because the bootstrap " +
                    "has not initialized a session."
                );
            }

            CardDeliveryZoneRuntime objective =
                FindDeliveryObjective();

            objectiveEntryView.Bind(
                objectiveTitle,
                objectiveDescription,
                objectiveIcon,
                objective.CurrentAmount,
                objective.RequiredAmount
            );
        }

        private CardDeliveryZoneRuntime
            FindDeliveryObjective()
        {
            foreach (
                CardDeliveryZoneRuntime objective
                in bootstrap.Session.Mission.DeliveryObjectives
            )
            {
                if (ReferenceEquals(
                    objective.AcceptedDefinition,
                    acceptedDefinition
                ))
                {
                    return objective;
                }
            }

            throw new InvalidOperationException(
                $"{nameof(InitialObjectivePresenter)} on '{name}' " +
                $"could not find a delivery objective accepting " +
                $"'{acceptedDefinition.name}'."
            );
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialObjectivePresenter)} on '{name}' " +
                    $"has no {nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (acceptedDefinition == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialObjectivePresenter)} on '{name}' " +
                    "has no accepted card definition assigned."
                );
            }

            if (objectiveEntryView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialObjectivePresenter)} on '{name}' " +
                    $"has no {nameof(ObjectiveEntryView)} assigned."
                );
            }
        }
    }
}