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
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;
using Catalyst.Cards.Runtime.Turn;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionBuilderTests
    {
        private CardDefinition hydrogen;
        private CardDefinition oxygen;
        private CardDefinition water;

        [SetUp]
        public void SetUp()
        {
            hydrogen =
                ScriptableObject.CreateInstance<CardDefinition>();

            oxygen =
                ScriptableObject.CreateInstance<CardDefinition>();
            water =
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
        public void Build_CreatesConfiguredDeliveryZone()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var zoneConfig =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    zoneConfig
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            Assert.That(
                session.DeliveryZones.Count,
                Is.EqualTo(1)
            );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            Assert.That(
                zone.AcceptedDefinition,
                Is.SameAs(water)
            );

            Assert.That(
                zone.RequiredAmount,
                Is.EqualTo(10)
            );

            Assert.That(
                zone.CurrentAmount,
                Is.Zero
            );

            Assert.That(
                zone.IsCompleted,
                Is.False
            );

            Assert.That(
                zone.IsEmpty,
                Is.True
            );
        }

        [Test]
        public void Build_CreatesAllConfiguredDeliveryZones()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var waterZoneConfig =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            var oxygenZoneConfig =
                new CardDeliveryZoneConfig(
                    oxygen,
                    requiredAmount: 2
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    waterZoneConfig,
                    oxygenZoneConfig
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            Assert.That(
                session.DeliveryZones.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                session.DeliveryZones[0]
                    .AcceptedDefinition,
                Is.SameAs(water)
            );

            Assert.That(
                session.DeliveryZones[0]
                    .RequiredAmount,
                Is.EqualTo(10)
            );

            Assert.That(
                session.DeliveryZones[1]
                    .AcceptedDefinition,
                Is.SameAs(oxygen)
            );

            Assert.That(
                session.DeliveryZones[1]
                    .RequiredAmount,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Build_DeliveryZonesStartEmpty()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var hydrogenZoneConfig =
                new CardDeliveryZoneConfig(
                    hydrogen,
                    requiredAmount: 3
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    hydrogenZoneConfig
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            Assert.That(zone.Count, Is.Zero);
            Assert.That(zone.CurrentAmount, Is.Zero);
            Assert.That(zone.IsEmpty, Is.True);
            Assert.That(zone.IsCompleted, Is.False);
        }

        [Test]
        public void Build_DoesNotPlaceMatchingCardsInDeliveryZone()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var hydrogenZoneConfig =
                new CardDeliveryZoneConfig(
                    hydrogen,
                    requiredAmount: 3
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    hydrogenZoneConfig
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            bool matchingCardExistsInSession =
                session.SessionCards.Any(
                    card => ReferenceEquals(
                        card.Definition,
                        hydrogen
                    )
                );

            Assert.That(
                matchingCardExistsInSession,
                Is.True
            );

            Assert.That(
                zone.CurrentAmount,
                Is.Zero
            );

            Assert.That(
                zone.IsEmpty,
                Is.True
            );
        }


        [Test]
        public void Build_WithDeliveryZones_ProducesValidInitialOwnership()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var zoneConfig =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    zoneConfig
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void Build_DeliveryZoneCollectionCannotBeModifiedExternally()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var zoneConfig =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    zoneConfig
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            var exposedCollection =
                session.DeliveryZones
                    as IList<CardDeliveryZoneRuntime>;

            Assert.That(
                exposedCollection,
                Is.Not.Null
            );

            Assert.That(
                exposedCollection.IsReadOnly,
                Is.True
            );

            Assert.That(
                () => exposedCollection.Add(
                    new CardDeliveryZoneRuntime(
                        water,
                        requiredAmount: 2
                    )
                ),
                Throws.TypeOf<NotSupportedException>()
            );

            Assert.That(
                session.DeliveryZones.Count,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void Build_CreatesMissionRuntime()
        {
            GameSession session =
                BuildSession(
                    cardCount: 12,
                    seed: 12345
                );

            Assert.That(
                session.Mission,
                Is.Not.Null
            );
        }

        [Test]
        public void Build_WithoutDeliveryZones_CreatesInactiveMission()
        {
            GameSession session =
                BuildSession(
                    cardCount: 12,
                    seed: 12345
                );

            Assert.That(
                session.Mission.HasObjectives,
                Is.False
            );

            Assert.That(
                session.Mission.IsCompleted,
                Is.False
            );
        }

        [Test]
        public void Build_MissionObservesConfiguredDeliveryZones()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var waterObjective =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    waterObjective
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            Assert.That(
                session.Mission.HasObjectives,
                Is.True
            );

            Assert.That(
                session.Mission.DeliveryObjectives.Count,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void Build_MissionUsesSameDeliveryZoneInstances()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var waterObjective =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 10
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    waterObjective
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            Assert.That(
                session.Mission.DeliveryObjectives[0],
                Is.SameAs(session.DeliveryZones[0])
            );
        }

        [Test]
        public void Build_MissionStartsIncomplete()
        {
            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(12)
                );

            var waterObjective =
                new CardDeliveryZoneConfig(
                    water,
                    requiredAmount: 1
                );

            GameSession session =
                builder.Build(
                    CreateEntries(12),
                    new GameSessionConfig(
                        initialHandSize: 8,
                        maxHandSize: 8,
                        deliveryZones: new[]
                        {
                    waterObjective
                        }
                    ),
                    new SeededRandomSource(12345)
                );

            Assert.That(
                session.Mission.IsCompleted,
                Is.False
            );
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

            Assert.That(
                session.DeliveryZones,
                Is.Not.Null
            );

            Assert.That(
                session.DeliveryZones,
                Is.Empty
            );

            Assert.That(session.Turn, Is.Not.Null);
        }
        [Test]
        public void Build_CreatesTurnRuntimeNotYetStarted()
        {
            GameSession session = BuildSession(
                cardCount: 12,
                seed: 12345
            );

            Assert.That(session.Turn.HasStarted, Is.False);
            Assert.That(session.Turn.TurnNumber, Is.Zero);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.NotStarted)
            );
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

            int cardsInDeliveryZones =
                session.DeliveryZones.Sum(
                    zone => zone.Count
                );

            int cardsInZones =
                session.Deck.Count
                + session.Hand.Count
                + session.ReactionTable.Count
                + session.DiscardPile.Count
                + cardsInDeliveryZones;

            Assert.That(
                cardsInZones,
                Is.EqualTo(12)
            );
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

        [Test]
        public void Build_CreatesResourcesWithConfiguredAmounts()
        {
            GameSession session =
                BuildSession(
                    cardCount: 12,
                    seed: 12345,
                    initialHeat: 3,
                    initialElectricity: 4
                );

            Assert.That(
                session.Heat,
                Is.Not.Null
            );

            Assert.That(
                session.Heat.Amount,
                Is.EqualTo(3)
            );

            Assert.That(
                session.Electricity,
                Is.Not.Null
            );

            Assert.That(
                session.Electricity.Amount,
                Is.EqualTo(4)
            );
        }

        [Test]
        public void Build_WithoutConfiguredResources_StartsResourcesAtZero()
        {
            GameSession session =
                BuildSession(
                    cardCount: 12,
                    seed: 12345
                );

            Assert.That(
                session.Heat.Amount,
                Is.Zero
            );

            Assert.That(
                session.Electricity.Amount,
                Is.Zero
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
    int cardCount,
    int seed,
    int initialHeat = 0,
    int initialElectricity = 0
)
        {
            return BuildSession(
                CreateSequentialIds(cardCount),
                seed,
                initialHeat,
                initialElectricity
            );
        }

        private GameSession BuildSession(
            Guid[] ids,
            int seed,
            int initialHeat = 0,
            int initialElectricity = 0
        )
        {
            GameSessionBuilder builder =
                CreateBuilder(ids);

            return builder.Build(
                CreateEntries(ids.Length),
                new GameSessionConfig(
                    initialHandSize: 8,
                    maxHandSize: 8,
                    initialHeat: initialHeat,
                    initialElectricity: initialElectricity
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