using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionBuilderTests
    {
        private CardDefinition hydrogen;
        private CardDefinition oxygen;

        [SetUp]
        public void SetUp()
        {
            hydrogen =
                ScriptableObject.CreateInstance<CardDefinition>();

            oxygen =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (hydrogen != null)
            {
                UnityEngine.Object.DestroyImmediate(hydrogen);
            }

            if (oxygen != null)
            {
                UnityEngine.Object.DestroyImmediate(oxygen);
            }
        }

        [Test]
        public void Build_CreatesAllSessionZones()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            Assert.That(session.Deck, Is.Not.Null);
            Assert.That(session.Hand, Is.Not.Null);
            Assert.That(session.ReactionTable, Is.Not.Null);
            Assert.That(session.DiscardPile, Is.Not.Null);
        }

        [Test]
        public void Build_RegistersEveryCreatedCard()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(12)
            );

            int cardsInZones =
                session.Deck.Count
                + session.Hand.Count
                + session.ReactionTable.Count
                + session.DiscardPile.Count;

            Assert.That(cardsInZones, Is.EqualTo(12));
        }

        [Test]
        public void Build_FormsConfiguredInitialHand()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            Assert.That(session.Hand.Capacity, Is.EqualTo(8));
            Assert.That(session.Hand.Count, Is.EqualTo(8));
            Assert.That(session.Deck.Count, Is.EqualTo(4));

            Assert.That(
                session.ReactionTable.IsEmpty,
                Is.True
            );

            Assert.That(
                session.DiscardPile.IsEmpty,
                Is.True
            );
        }

        [Test]
        public void Build_WithDeckSmallerThanInitialHand_CreatesValidPartialHand()
        {
            GameSession session = BuildSession(
                cardCount: 5,
                seed: 12345
            );

            Assert.That(session.Hand.Count, Is.EqualTo(5));
            Assert.That(session.Deck.IsEmpty, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void Build_CreatesUniqueSessionCardIds()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            int uniqueIdCount =
                session.SessionCards
                    .Select(card => card.InstanceId)
                    .Distinct()
                    .Count();

            Assert.That(uniqueIdCount, Is.EqualTo(12));
        }

        [Test]
        public void Build_ProducesValidInitialOwnership()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void Build_WithSameIdsAndSeed_ProducesSameInitialHand()
        {
            Guid[] ids = CreateSequentialIds(12);

            GameSession firstSession =
                BuildSession(ids, 6789);

            GameSession secondSession =
                BuildSession(ids, 6789);

            Guid[] firstHandOrder =
                firstSession.Hand.Cards
                    .Select(card => card.InstanceId)
                    .ToArray();

            Guid[] secondHandOrder =
                secondSession.Hand.Cards
                    .Select(card => card.InstanceId)
                    .ToArray();

            Assert.That(
                firstHandOrder,
                Is.EqualTo(secondHandOrder)
            );
        }

        [Test]
        public void TryGetCard_ReturnsRegisteredInstance()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            CardInstance expectedCard =
                session.SessionCards[0];

            bool found = session.TryGetCard(
                expectedCard.InstanceId,
                out CardInstance returnedCard
            );

            Assert.That(found, Is.True);
            Assert.That(
                returnedCard,
                Is.SameAs(expectedCard)
            );
        }

        [Test]
        public void ValidateState_WhenCardExistsInTwoZones_Throws()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            CardInstance handCard =
                session.Hand.Cards[0];

            bool added =
                session.ReactionTable.TryAdd(handCard);

            Assert.That(added, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ValidateState_WhenRegisteredCardIsInNoZone_Throws()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            CardInstance handCard =
                session.Hand.Cards[0];

            session.Hand.TryRemove(handCard);

            Assert.That(
                () => session.ValidateState(),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void Build_WithNullConfiguration_Throws()
        {
            GameSessionBuilder builder =
                CreateBuilder(CreateSequentialIds(12));

            Assert.That(
                () => builder.Build(
                    CreateEntries(12),
                    null,
                    new SeededRandomSource(1)
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Build_WithNullRandomSource_Throws()
        {
            GameSessionBuilder builder =
                CreateBuilder(CreateSequentialIds(12));

            Assert.That(
                () => builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(8, 8),
                    null
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private GameSession BuildSession(
            int cardCount,
            int seed
        )
        {
            return BuildSession(
                CreateSequentialIds(cardCount),
                seed
            );
        }

        private GameSession BuildSession(
            Guid[] ids,
            int seed
        )
        {
            GameSessionBuilder builder =
                CreateBuilder(ids);

            return builder.Build(
                CreateEntries(ids.Length),
                new GameSessionConfig(
                    initialHandSize: 8,
                    maxHandSize: 8
                ),
                new SeededRandomSource(seed)
            );
        }

        private GameSessionBuilder CreateBuilder(
            IEnumerable<Guid> ids
        )
        {
            ICardInstanceIdSource idSource =
                new SequenceIdSource(ids);

            CardInstanceFactory instanceFactory =
                new CardInstanceFactory(idSource);

            DeckRuntimeBuilder deckBuilder =
                new DeckRuntimeBuilder(instanceFactory);

            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(movementService);

            return new GameSessionBuilder(
                deckBuilder,
                drawService
            );
        }

        private DeckEntry[] CreateEntries(
            int totalCardCount
        )
        {
            int hydrogenCount = totalCardCount / 2;
            int oxygenCount =
                totalCardCount - hydrogenCount;

            List<DeckEntry> entries =
                new List<DeckEntry>();

            if (hydrogenCount > 0)
            {
                entries.Add(
                    new DeckEntry(
                        hydrogen,
                        hydrogenCount
                    )
                );
            }

            if (oxygenCount > 0)
            {
                entries.Add(
                    new DeckEntry(
                        oxygen,
                        oxygenCount
                    )
                );
            }

            return entries.ToArray();
        }

        private static Guid[] CreateSequentialIds(
            int count
        )
        {
            Guid[] ids = new Guid[count];

            for (int index = 0;
                 index < count;
                 index++)
            {
                string suffix =
                    (index + 1).ToString("D12");

                ids[index] = Guid.Parse(
                    $"00000000-0000-0000-0000-{suffix}"
                );
            }

            return ids;
        }

        private sealed class SequenceIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public SequenceIdSource(
                IEnumerable<Guid> ids
            )
            {
                this.ids = new Queue<Guid>(ids);
            }

            public Guid NextId()
            {
                if (ids.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No more test IDs are available."
                    );
                }

                return ids.Dequeue();
            }
        }
    }
}