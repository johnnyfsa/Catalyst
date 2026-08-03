using System;
using Catalyst.UI.Definitions.Results;
using UnityEngine;

namespace Catalyst.UI.Presentation.GameResult
{
    public sealed class GameResultContext :
        MonoBehaviour
    {
        [Header("Phase Result Definitions")]
        [SerializeField]
        private GameResultDefinitionLibrary
            definitionLibrary;

        public GameResultDefinitionLibrary
            DefinitionLibrary =>
                definitionLibrary;

        private void Awake()
        {
            ValidateConfiguration();
        }

        public void ValidateConfiguration()
        {
            if (definitionLibrary == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultContext)} " +
                    $"on '{name}' has no " +
                    $"{nameof(GameResultDefinitionLibrary)} " +
                    "assigned."
                );
            }

            definitionLibrary
                .ValidateConfiguration();
        }
    }
}