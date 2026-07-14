using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using UnityEngine;

namespace Catalyst.Game.Bootstrap
{
    public sealed class GameSessionBootstrap : MonoBehaviour
    {
        [Header("Deck")]
        [SerializeField]
        private List<DeckEntry> deckEntries =
            new List<DeckEntry>();

        [Header("Hand")]
        [SerializeField]
        [Min(1)]
        private int initialHandSize = 8;

        [SerializeField]
        [Min(1)]
        private int maxHandSize = 8;

        [Header("Randomness")]
        [SerializeField]
        private int randomSeed = 12345;

        public GameSession Session { get; private set; }

        private void Awake()
        {
            Initialize(
                deckEntries,
                initialHandSize,
                maxHandSize,
                randomSeed
            );
            Debug.Log(
    $"Session initialized. " +
    $"Cards: {Session.SessionCards.Count}, " +
    $"Deck: {Session.Deck.Count}, " +
    $"Hand: {Session.Hand.Count}"
);
        }

        internal GameSession Initialize(
            IEnumerable<DeckEntry> entries,
            int requestedInitialHandSize,
            int requestedMaxHandSize,
            int seed
        )
        {
            if (Session != null)
            {
                throw new InvalidOperationException(
                    "The game session has already been initialized."
                );
            }

            ICardInstanceIdSource idSource =
                new GuidCardInstanceIdSource();

            CardInstanceFactory instanceFactory =
                new CardInstanceFactory(idSource);

            DeckRuntimeBuilder deckBuilder =
                new DeckRuntimeBuilder(instanceFactory);

            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(movementService);

            GameSessionBuilder sessionBuilder =
                new GameSessionBuilder(
                    deckBuilder,
                    drawService
                );

            GameSessionConfig config =
                new GameSessionConfig(
                    requestedInitialHandSize,
                    requestedMaxHandSize
                );

            IRandomSource randomSource =
                new SeededRandomSource(seed);

            Session = sessionBuilder.Build(
                entries,
                config,
                randomSource
            );

            return Session;
        }
    }
}