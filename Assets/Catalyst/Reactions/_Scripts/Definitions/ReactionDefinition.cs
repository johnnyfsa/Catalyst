using System.Collections.Generic;
using Catalyst.Reactions.Runtime;
using UnityEngine;

namespace Catalyst.Reactions.Definitions
{
    [CreateAssetMenu(
        fileName = "ReactionDefinition",
        menuName = "Catalyst/Reactions/Reaction Definition")]
    public sealed class ReactionDefinition : ScriptableObject
    {
        [SerializeField]
        private string reactionId;

        [SerializeField]
        private List<ReactionCardAmount> reactants = new();

        [SerializeField]
        private List<ReactionCardAmount> products = new();

        [Header("Phase 1 Resources")]
        [SerializeField, Min(0)]
        private int requiredHeat;

        [SerializeField, Min(0)]
        private int producedHeat;

        public string ReactionId => reactionId;

        public IReadOnlyList<ReactionCardAmount> Reactants =>
            reactants;

        public IReadOnlyList<ReactionCardAmount> Products =>
            products;

        public int RequiredHeat => requiredHeat;

        public int ProducedHeat => producedHeat;

#if UNITY_EDITOR
        internal void ConfigureForTests(
            string id,
            IEnumerable<ReactionCardAmount> reactionReactants,
            IEnumerable<ReactionCardAmount> reactionProducts,
            int heatRequired = 0,
            int heatProduced = 0)
        {
            reactionId = id;

            reactants = reactionReactants is null
                ? null
                : new List<ReactionCardAmount>(reactionReactants);

            products = reactionProducts is null
                ? null
                : new List<ReactionCardAmount>(reactionProducts);

            requiredHeat = heatRequired;
            producedHeat = heatProduced;
        }

        private void OnValidate()
        {
            ReactionDefinitionValidationResult result =
                ReactionDefinitionValidator.Validate(this);

            if (!result.IsValid)
            {
                Debug.LogWarning(
                    $"Reaction definition '{name}' is invalid: " +
                    $"{result.Failure}.",
                    this);
            }
        }
#endif
    }
}