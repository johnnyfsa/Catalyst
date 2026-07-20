using System;
using System.Collections.Generic;
using Catalyst.Reactions.Definitions;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Common.Creation
{
    internal sealed class TestReactionFactory
        : IDisposable
    {
        private readonly List<ReactionDefinition>
            createdDefinitions = new();

        public ReactionDefinition Create(
    string reactionId,
    IEnumerable<ReactionCardAmount> reactants,
    IEnumerable<ReactionCardAmount> products,
    int requiredHeat = 0,
    int producedHeat = 0,
    int requiredElectricity = 0,
    int producedElectricity = 0
)
        {
            ReactionDefinition definition =
                ScriptableObject
                    .CreateInstance<ReactionDefinition>();

            definition.ConfigureForTests(
                reactionId,
                reactants,
                products,
                requiredHeat,
                producedHeat,
                requiredElectricity,
                producedElectricity
            );

            createdDefinitions.Add(definition);

            return definition;
        }

        public void Dispose()
        {
            foreach (
                ReactionDefinition definition
                in createdDefinitions
            )
            {
                if (definition != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        definition
                    );
                }
            }

            createdDefinitions.Clear();
        }
    }
}