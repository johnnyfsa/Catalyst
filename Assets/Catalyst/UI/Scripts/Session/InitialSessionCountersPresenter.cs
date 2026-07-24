using System;
using Catalyst.Game.Bootstrap;
using Catalyst.UI.Definitions;
using Catalyst.UI.Presentation;
using TMPro;
using UnityEngine;

namespace Catalyst.UI.Presentation.Session
{
    public sealed class InitialSessionCountersPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Deck")]
        [SerializeField]
        private TMP_Text remainingCardsAmountText;

        [Header("Heat")]
        [SerializeField]
        private ResourceEntryStyleView heatView;

        [SerializeField]
        private ResourceEntryStyleAsset heatStyle;

        [Header("Electricity")]
        [SerializeField]
        private ResourceEntryStyleView electricityView;

        [SerializeField]
        private ResourceEntryStyleAsset electricityStyle;

        private void Start()
        {
            PresentInitialCounters();
        }

        [ContextMenu("Present Initial Counters")]
        public void PresentInitialCounters()
        {
            ValidateReferences();

            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' cannot present session counters " +
                    "because the bootstrap has not initialized a session."
                );
            }

            remainingCardsAmountText.text =
                bootstrap.Session.Deck.Count.ToString();

            heatView.Bind(
                heatStyle,
                bootstrap.Session.Heat.Amount
            );

            electricityView.Bind(
                electricityStyle,
                bootstrap.Session.Electricity.Amount
            );
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (remainingCardsAmountText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' has no remaining cards amount " +
                    "text assigned."
                );
            }

            if (heatView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' has no heat view assigned."
                );
            }

            if (heatStyle == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' has no heat style assigned."
                );
            }

            if (electricityView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' has no electricity view assigned."
                );
            }

            if (electricityStyle == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' has no electricity style assigned."
                );
            }

            if (ReferenceEquals(
                heatView,
                electricityView
            ))
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialSessionCountersPresenter)} " +
                    $"on '{name}' cannot use the same resource " +
                    "entry view for heat and electricity."
                );
            }
        }
    }
}