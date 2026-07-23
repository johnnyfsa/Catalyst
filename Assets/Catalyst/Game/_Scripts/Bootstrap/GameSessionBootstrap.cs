using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Delivery;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Reactions.Definitions;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Turn;
using UnityEngine;

namespace Catalyst.Game.Bootstrap
{
    public sealed class GameSessionBootstrap : MonoBehaviour
    {
        [Serializable]
        private sealed class DeliveryZoneSetup
        {
            [SerializeField]
            private CardDefinition acceptedDefinition;

            [SerializeField]
            [Min(1)]
            private int requiredAmount = 1;

            public CardDeliveryZoneConfig CreateRuntimeConfig()
            {
                if (acceptedDefinition == null)
                {
                    throw new InvalidOperationException(
                        "A delivery zone must have an accepted CardDefinition."
                    );
                }

                return new CardDeliveryZoneConfig(
                    acceptedDefinition,
                    requiredAmount
                );
            }
        }

        [Header("Deck")]
        [SerializeField]
        private DeckDefinition deckDefinition;

        [Header("Reactions")]
        [SerializeField]
        private ReactionLibraryDefinition reactionLibrary;

        [Header("Hand")]
        [SerializeField]
        [Min(1)]
        private int initialHandSize = 8;

        [SerializeField]
        [Min(1)]
        private int maxHandSize = 8;

        [Header("Resources")]
        [SerializeField]
        [Min(0)]
        private int initialHeat;

        [SerializeField]
        [Min(0)]
        private int initialElectricity;

        [Header("Mission")]
        [SerializeField]
        private List<DeliveryZoneSetup> deliveryZones =
            new List<DeliveryZoneSetup>();

        [Header("Turn Limit")]
        [SerializeField]
        private bool useTurnLimit = true;

        [SerializeField]
        [Min(1)]
        private int maximumTurns = 10;

        [Header("Randomness")]
        [SerializeField]
        private int randomSeed = 12345;

        public GameSession Session { get; private set; }

        public GameSessionFlowService SessionFlow
        {
            get;
            private set;
        }

        public IReadOnlyList<ReactionDefinition>
            AvailableReactions =>
                reactionLibrary != null
                    ? reactionLibrary.Reactions
                    : Array.Empty<ReactionDefinition>();

        private void Awake()
        {
            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(
                    movementService
                );

            GameSessionBuilder sessionBuilder =
                CreateSessionBuilder(
                    drawService
                );

            SessionFlow =
                CreateSessionFlow(
                    movementService,
                    drawService
                );

            CardDeliveryZoneConfig[] deliveryZoneConfigs =
                BuildDeliveryZoneConfigs();

            int? configuredMaximumTurns =
                useTurnLimit
                    ? maximumTurns
                    : null;

            if (deckDefinition == null)
            {
                throw new InvalidOperationException(
                    "A DeckDefinition must be assigned."
                );
            }

            if (reactionLibrary == null)
            {
                throw new InvalidOperationException(
                    "A ReactionLibraryDefinition must be assigned."
                );
            }

            InitializeFromDeckDefinition(
                sessionBuilder,
                deckDefinition,
                new GameSessionConfig(
                    initialHandSize: initialHandSize,
                    maxHandSize: maxHandSize,
                    initialHeat: initialHeat,
                    initialElectricity:
                        initialElectricity,
                    deliveryZones:
                        deliveryZoneConfigs,
                    maximumTurns:
                        configuredMaximumTurns
                ),
                new SeededRandomSource(
                    randomSeed
                )
            );

            Debug.Log(
                $"Session initialized. " +
                $"Cards: {Session.SessionCards.Count}, " +
                $"Deck: {Session.Deck.Count}, " +
                $"Hand: {Session.Hand.Count}, " +
                $"Delivery zones: {Session.DeliveryZones.Count}, " +
                $"Reactions: {AvailableReactions.Count}, " +
                $"Heat: {Session.Heat.Amount}, " +
                $"Electricity: {Session.Electricity.Amount}, " +
                $"Turn limit: {FormatTurnLimit(Session)}"
            );
        }

        internal GameSession InitializeFromDeckDefinition(
            DeckDefinition definition,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(
                    movementService
                );

            GameSessionBuilder sessionBuilder =
                CreateSessionBuilder(
                    drawService
                );

            return InitializeFromDeckDefinition(
                sessionBuilder,
                definition,
                config,
                randomSource
            );
        }

        internal GameSession InitializeFromDeckEntries(
            IEnumerable<DeckEntry> entries,
            int requestedInitialHandSize,
            int requestedMaxHandSize,
            int seed
        )
        {
            return InitializeFromDeckEntries(
                entries,
                new GameSessionConfig(
                    initialHandSize:
                        requestedInitialHandSize,
                    maxHandSize:
                        requestedMaxHandSize
                ),
                new SeededRandomSource(seed)
            );
        }

        internal GameSession InitializeFromDeckEntries(
            IEnumerable<DeckEntry> entries,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(
                    movementService
                );

            GameSessionBuilder sessionBuilder =
                CreateSessionBuilder(
                    drawService
                );

            return Initialize(
                sessionBuilder,
                entries,
                config,
                randomSource
            );
        }

        internal GameSession Initialize(
            IEnumerable<DeckEntry> entries,
            int requestedInitialHandSize,
            int requestedMaxHandSize,
            int seed
        )
        {
            return InitializeFromDeckEntries(
                entries,
                requestedInitialHandSize,
                requestedMaxHandSize,
                seed
            );
        }

        internal GameSession Initialize(
            IEnumerable<DeckEntry> entries,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            return InitializeFromDeckEntries(
                entries,
                config,
                randomSource
            );
        }

        private GameSession InitializeFromDeckDefinition(
            GameSessionBuilder sessionBuilder,
            DeckDefinition definition,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition)
                );
            }

            return Initialize(
                sessionBuilder,
                definition.Entries,
                config,
                randomSource
            );
        }

        private GameSession Initialize(
            GameSessionBuilder sessionBuilder,
            IEnumerable<DeckEntry> entries,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            if (Session != null)
            {
                throw new InvalidOperationException(
                    "The game session has already been initialized."
                );
            }

            if (sessionBuilder == null)
            {
                throw new ArgumentNullException(
                    nameof(sessionBuilder)
                );
            }

            if (entries == null)
            {
                throw new ArgumentNullException(
                    nameof(entries)
                );
            }

            if (config == null)
            {
                throw new ArgumentNullException(
                    nameof(config)
                );
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(
                    nameof(randomSource)
                );
            }

            Session = sessionBuilder.Build(
                entries,
                config,
                randomSource
            );

            return Session;
        }

        private CardDeliveryZoneConfig[]
            BuildDeliveryZoneConfigs()
        {
            if (deliveryZones == null)
            {
                return Array.Empty
                    <CardDeliveryZoneConfig>();
            }

            return deliveryZones
                .Select(
                    setup =>
                    {
                        if (setup == null)
                        {
                            throw new InvalidOperationException(
                                "Delivery zone setup collection cannot contain null entries."
                            );
                        }

                        return setup
                            .CreateRuntimeConfig();
                    }
                )
                .ToArray();
        }

        private static GameSessionBuilder
            CreateSessionBuilder(
                CardDrawService drawService
            )
        {
            ICardInstanceIdSource idSource =
                new GuidCardInstanceIdSource();

            CardInstanceFactory instanceFactory =
                new CardInstanceFactory(
                    idSource
                );

            DeckRuntimeBuilder deckBuilder =
                new DeckRuntimeBuilder(
                    instanceFactory
                );

            return new GameSessionBuilder(
                deckBuilder,
                drawService
            );
        }

        private static GameSessionFlowService
            CreateSessionFlow(
                CardMovementService movementService,
                CardDrawService drawService
            )
        {
            DrawPhaseService drawPhaseService =
                new DrawPhaseService(
                    drawService
                );

            MainPhaseService mainPhaseService =
                new MainPhaseService(
                    movementService
                );

            ManualDiscardService manualDiscardService =
                new ManualDiscardService(
                    movementService
                );

            EndPhaseService endPhaseService =
                new EndPhaseService();

            CardDeliveryService cardDeliveryService =
                new CardDeliveryService(
                    movementService
                );

            return new GameSessionFlowService(
                drawPhaseService,
                mainPhaseService,
                manualDiscardService,
                endPhaseService,
                cardDeliveryService
            );
        }

        private static string FormatTurnLimit(
            GameSession session
        )
        {
            return session.HasTurnLimit
                ? session.MaximumTurns.Value
                    .ToString()
                : "None";
        }
    }
}