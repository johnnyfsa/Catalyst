using System;
using System.Collections.Generic;
using Catalyst.Cards.Runtime.Session;
using UnityEngine;

namespace Catalyst.UI.Definitions.Results
{
    [CreateAssetMenu(
        fileName = "GameResultDefinitionLibrary",
        menuName =
            "Catalyst/UI/Game Result Definition Library"
    )]
    public sealed class GameResultDefinitionLibrary :
        ScriptableObject
    {
        [SerializeField]
        private List<GameResultDefinition> definitions =
            new List<GameResultDefinition>();

        public IReadOnlyList<GameResultDefinition>
            Definitions => definitions;

        public bool TryGet(
            GameSessionEndReason reason,
            out GameResultDefinition definition
        )
        {
            definition = null;

            if (reason == GameSessionEndReason.None)
            {
                return false;
            }

            ValidateConfiguration();

            foreach (
                GameResultDefinition candidate
                in definitions
            )
            {
                if (candidate.EndReason == reason)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public void ValidateConfiguration()
        {
            if (definitions == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultDefinitionLibrary)} " +
                    $"'{name}' has a null definitions collection."
                );
            }

            var configuredReasons =
                new HashSet<GameSessionEndReason>();

            for (
                int index = 0;
                index < definitions.Count;
                index++
            )
            {
                GameResultDefinition definition =
                    definitions[index];

                if (definition == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(GameResultDefinitionLibrary)} " +
                        $"'{name}' contains a null definition " +
                        $"at index {index}."
                    );
                }

                GameSessionEndReason reason =
                    definition.EndReason;

                if (reason == GameSessionEndReason.None)
                {
                    throw new InvalidOperationException(
                        $"{nameof(GameResultDefinitionLibrary)} " +
                        $"'{name}' contains definition " +
                        $"'{definition.name}' using " +
                        $"{nameof(GameSessionEndReason)}." +
                        $"{nameof(GameSessionEndReason.None)}."
                    );
                }

                if (!configuredReasons.Add(reason))
                {
                    throw new InvalidOperationException(
                        $"{nameof(GameResultDefinitionLibrary)} " +
                        $"'{name}' contains more than one " +
                        $"definition for end reason '{reason}'."
                    );
                }
            }
        }
    }
}