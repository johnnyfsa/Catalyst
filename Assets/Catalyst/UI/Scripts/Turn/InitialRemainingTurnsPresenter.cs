using System;
using Catalyst.Game.Bootstrap;
using TMPro;
using UnityEngine;

namespace Catalyst.UI.Presentation.Turn
{
    public sealed class InitialRemainingTurnsPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Remaining Turns View")]
        [SerializeField]
        private GameObject remainingTurnsArea;

        [SerializeField]
        private TMP_Text remainingTurnsAmountText;

        private void Start()
        {
            PresentInitialRemainingTurns();
        }

        [ContextMenu("Present Initial Remaining Turns")]
        public void PresentInitialRemainingTurns()
        {
            ValidateReferences();

            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialRemainingTurnsPresenter)} " +
                    $"on '{name}' cannot present remaining turns " +
                    "because the bootstrap has not initialized a session."
                );
            }

            if (!bootstrap.Session.HasTurnLimit)
            {
                remainingTurnsArea.SetActive(true);
                remainingTurnsAmountText.text = "∞";
                return;
            }

            int maximumTurns =
                bootstrap.Session.MaximumTurns.Value;

            int currentTurn =
                bootstrap.Session.Turn.TurnNumber;

            int remainingTurns =
                maximumTurns - currentTurn + 1;

            if (remainingTurns < 0)
            {
                remainingTurns = 0;
            }

            remainingTurnsArea.SetActive(true);

            remainingTurnsAmountText.text =
                remainingTurns.ToString();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialRemainingTurnsPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (remainingTurnsArea == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialRemainingTurnsPresenter)} " +
                    $"on '{name}' has no remaining turns area assigned."
                );
            }

            if (remainingTurnsAmountText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialRemainingTurnsPresenter)} " +
                    $"on '{name}' has no remaining turns amount text assigned."
                );
            }
        }
    }
}