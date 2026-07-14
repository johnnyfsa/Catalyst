using System.Collections.Generic;
using UnityEngine;

namespace Catalyst.Cards.Definitions
{
    [CreateAssetMenu(
        fileName = "DeckDefinition",
        menuName = "Catalyst/Cards/Deck Definition"
    )]
    public sealed class DeckDefinition : ScriptableObject
    {
        [SerializeField]
        private List<DeckEntry> entries =
            new List<DeckEntry>();

        public IReadOnlyList<DeckEntry> Entries => entries;
    }
}