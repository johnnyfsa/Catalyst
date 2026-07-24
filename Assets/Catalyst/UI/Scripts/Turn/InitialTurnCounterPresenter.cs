using System;
using Catalyst.Game.Bootstrap;
using TMPro;
using UnityEngine;

namespace Catalyst.UI.Presentation.Turn
{
    public sealed class InitialTurnCounterPresenter :
        MonoBehaviour
    {
        [Header("Runtime Source")]
        [SerializeField]
        private GameSessionBootstrap bootstrap;

        [Header("Turn View")]
        [SerializeField]
        private TMP_Text turnCountText;

        private void Start()
        {
            PresentInitialTurn();
        }

        [ContextMenu("Present Initial Turn")]
        public void PresentInitialTurn()
        {
            ValidateReferences();

            if (bootstrap.Session == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialTurnCounterPresenter)} " +
                    $"on '{name}' cannot present the current turn " +
                    "because the bootstrap has not initialized a session."
                );
            }

            turnCountText.text =
                bootstrap.Session.Turn
                    .TurnNumber
                    .ToString();
        }

        private void ValidateReferences()
        {
            if (bootstrap == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialTurnCounterPresenter)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameSessionBootstrap)} assigned."
                );
            }

            if (turnCountText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InitialTurnCounterPresenter)} " +
                    $"on '{name}' has no turn count text assigned."
                );
            }
        }
    }
}