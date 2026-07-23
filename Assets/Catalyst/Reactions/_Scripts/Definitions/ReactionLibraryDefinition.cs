using System.Collections.Generic;
using UnityEngine;

namespace Catalyst.Reactions.Definitions
{
    [CreateAssetMenu(
        fileName = "ReactionLibrary",
        menuName = "Catalyst/Reactions/Reaction Library"
    )]
    public sealed class ReactionLibraryDefinition
        : ScriptableObject
    {
        [SerializeField]
        private List<ReactionDefinition> reactions =
            new List<ReactionDefinition>();

        public IReadOnlyList<ReactionDefinition>
            Reactions => reactions;
    }
}