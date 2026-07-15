using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Cards.Runtime.Turn;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSessionBuilder
    {
        private readonly DeckRuntimeBuilder deckBuilder;
        private readonly CardDrawService drawService;

        public GameSessionBuilder(
            DeckRuntimeBuilder deckBuilder,
            CardDrawService drawService
        )
        {
            this.deckBuilder = deckBuilder
                ?? throw new ArgumentNullException(
                    nameof(deckBuilder)
                );

            this.drawService = drawService
                ?? throw new ArgumentNullException(
                    nameof(drawService)
                );
        }

        public GameSession Build(
            IEnumerable<DeckEntry> deckEntries,
            GameSessionConfig config,
            IRandomSource randomSource
        )
        {
            if (deckEntries == null)
            {
                throw new ArgumentNullException(
                    nameof(deckEntries)
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

            DeckRuntime deck =
                deckBuilder.Build(deckEntries);

            CardInstance[] sessionCards =
                deck.Cards.ToArray();

            deck.Shuffle(randomSource);

            HandRuntime hand =
                new HandRuntime(config.MaxHandSize);

            ReactionTableRuntime reactionTable =
                new ReactionTableRuntime();

            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            drawService.DrawInitialHand(
                deck,
                hand,
                config.InitialHandSize
            );
            TurnRuntime turn = new TurnRuntime();

            GameSession session = new GameSession(
                sessionCards,
                deck,
                hand,
                reactionTable,
                discardPile,
                turn
            );

            session.ValidateState();

            return session;
        }
    }
}