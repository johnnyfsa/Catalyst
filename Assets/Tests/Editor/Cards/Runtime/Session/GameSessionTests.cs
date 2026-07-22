using System;
using System.Collections.Generic;
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
using System.Linq;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionTests
    {
        private CardDefinition firstDefinition;
        private CardDefinition secondDefinition;
        [SetUp]
        public void SetUp()
        {
            firstDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            secondDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (firstDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    firstDefinition
                );
            }

            if (secondDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    secondDefinition
                );
            }
        }

        [Test]
        public void ValidateState_WhenCardIsInDeliveryZone_DoesNotThrow()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardInstance card =
                session.Hand.Cards[0];

            CardDeliveryZoneRuntime deliveryZone =
                session.DeliveryZones[0];

            bool removed =
                session.Hand.TryRemove(card);

            bool added =
                deliveryZone.TryAdd(card);

            Assert.That(removed, Is.True);
            Assert.That(added, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void ValidateState_WhenCardExistsInHandAndDeliveryZone_Throws()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardInstance card =
                session.Hand.Cards[0];

            CardDeliveryZoneRuntime deliveryZone =
                session.DeliveryZones[0];

            bool added =
                deliveryZone.TryAdd(card);

            Assert.That(added, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ValidateState_WhenDeliveryZoneContainsUnregisteredCard_Throws()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardInstance unregisteredCard =
                CreateCard(100);

            CardDeliveryZoneRuntime deliveryZone =
                session.DeliveryZones[0];

            bool added =
                deliveryZone.TryAdd(
                    unregisteredCard
                );

            Assert.That(added, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void RegisterCreatedCard_RegistersCard()
        {
            GameSession session =
                CreateSession();

            CardInstance createdCard =
                CreateCard(100);

            session.RegisterCreatedCard(
                createdCard
            );

            Assert.That(
                session.ContainsCard(
                    createdCard.InstanceId
                ),
                Is.True
            );

            Assert.That(
                session.SessionCards,
                Does.Contain(createdCard)
            );
        }

        [Test]
        public void RegisterCreatedCard_CanBeRetrievedById()
        {
            GameSession session =
                CreateSession();

            CardInstance createdCard =
                CreateCard(100);

            session.RegisterCreatedCard(
                createdCard
            );

            bool found =
                session.TryGetCard(
                    createdCard.InstanceId,
                    out CardInstance returnedCard
                );

            Assert.That(found, Is.True);

            Assert.That(
                returnedCard,
                Is.SameAs(createdCard)
            );
        }

        [Test]
        public void RegisterCreatedCard_WithNullCard_Throws()
        {
            GameSession session =
                CreateSession();

            Assert.That(
                () => session.RegisterCreatedCard(null),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void RegisterCreatedCard_WithDuplicateId_Throws()
        {
            GameSession session =
                CreateSession();

            CardInstance registeredCard =
                session.SessionCards[0];

            CardInstance duplicateCard =
                new CardInstance(
                    registeredCard.InstanceId,
                    firstDefinition
                );

            Assert.That(
                () => session.RegisterCreatedCard(
                    duplicateCard
                ),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void RegisterCreatedCard_WithDuplicateId_DoesNotMutateSession()
        {
            GameSession session =
                CreateSession();

            int initialCount =
                session.SessionCards.Count;

            CardInstance registeredCard =
                session.SessionCards[0];

            CardInstance duplicateCard =
                new CardInstance(
                    registeredCard.InstanceId,
                    firstDefinition
                );

            Assert.That(
                () => session.RegisterCreatedCard(
                    duplicateCard
                ),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(initialCount)
            );

            bool found =
                session.TryGetCard(
                    registeredCard.InstanceId,
                    out CardInstance returnedCard
                );

            Assert.That(found, Is.True);

            Assert.That(
                returnedCard,
                Is.SameAs(registeredCard)
            );
        }

        [Test]
        public void ContainsDeliveryZone_WithOwnedZone_ReturnsTrue()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            Assert.That(
                session.ContainsDeliveryZone(zone),
                Is.True
            );
        }

        [Test]
        public void ContainsDeliveryZone_WithForeignZone_ReturnsFalse()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            var foreignZone =
                new CardDeliveryZoneRuntime(
                    secondDefinition,
                    requiredAmount: 1
                );

            Assert.That(
                session.ContainsDeliveryZone(foreignZone),
                Is.False
            );
        }

        [Test]
        public void ContainsDeliveryZone_WithEquivalentForeignZone_ReturnsFalse()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardDeliveryZoneRuntime ownedZone =
                session.DeliveryZones[0];

            var equivalentForeignZone =
                new CardDeliveryZoneRuntime(
                    ownedZone.AcceptedDefinition,
                    ownedZone.RequiredAmount
                );

            Assert.That(
                session.ContainsDeliveryZone(
                    equivalentForeignZone
                ),
                Is.False
            );
        }

        [Test]
        public void Mission_BecomesCompletedWhenDeliveryZoneCompletes()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards[0];

            Assert.That(
                session.Mission.IsCompleted,
                Is.False
            );

            Assert.That(
                session.Hand.TryRemove(card),
                Is.True
            );

            Assert.That(
                zone.TryAdd(card),
                Is.True
            );

            Assert.That(
                zone.IsCompleted,
                Is.True
            );

            Assert.That(
                session.Mission.IsCompleted,
                Is.True
            );
        }

        [Test]
        public void Mission_RemainsIncompleteWhileAnyDeliveryZoneIsIncomplete()
        {
            GameSession session =
                CreateSessionWithTwoDeliveryZones();

            CardDeliveryZoneRuntime firstZone =
                session.DeliveryZones[0];

            CardDeliveryZoneRuntime secondZone =
                session.DeliveryZones[1];

            CardInstance matchingCard =
                session.Hand.Cards.First(
                    card => ReferenceEquals(
                        card.Definition,
                        firstZone.AcceptedDefinition
                    )
                );

            Assert.That(
                session.Mission.IsCompleted,
                Is.False
            );

            Assert.That(
                session.Hand.TryRemove(matchingCard),
                Is.True
            );

            Assert.That(
                firstZone.TryAdd(matchingCard),
                Is.True
            );

            Assert.That(
                firstZone.IsCompleted,
                Is.True
            );

            Assert.That(
                secondZone.IsCompleted,
                Is.False
            );

            Assert.That(
                session.Mission.IsCompleted,
                Is.False
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void Mission_BecomesCompletedWhenAllDeliveryZonesComplete()
        {
            GameSession session =
                CreateSessionWithTwoDeliveryZones();

            foreach (
                CardDeliveryZoneRuntime zone
                in session.DeliveryZones
            )
            {
                CardInstance matchingCard =
                    session.Hand.Cards.First(
                        card => ReferenceEquals(
                            card.Definition,
                            zone.AcceptedDefinition
                        )
                    );

                Assert.That(
                    session.Hand.TryRemove(matchingCard),
                    Is.True
                );

                Assert.That(
                    zone.TryAdd(matchingCard),
                    Is.True
                );
            }

            Assert.That(
                session.Mission.IsCompleted,
                Is.True
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void ContainsDeliveryZone_WithNullZone_ReturnsFalse()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            Assert.That(
                session.ContainsDeliveryZone(null),
                Is.False
            );
        }

        #region Helpers

        private GameSession CreateSession()
        {
            var idSource =
                new QueueIdSource(
                    CreateGuid(1)
                );

            var instanceFactory =
                new CardInstanceFactory(idSource);

            var deckBuilder =
                new DeckRuntimeBuilder(instanceFactory);

            var movementService =
                new CardMovementService();

            var drawService =
                new CardDrawService(movementService);

            var builder =
                new GameSessionBuilder(
                    deckBuilder,
                    drawService
                );

            var entries =
                new[]
                {
            new DeckEntry(
                firstDefinition,
                quantity: 1
            )
                };

            return builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize: 1,
                    maxHandSize: 8
                ),
                new SeededRandomSource(12345)
            );
        }

        private CardInstance CreateCard(
            int id
        )
        {
            return new CardInstance(
                CreateGuid(id),
                firstDefinition
            );
        }

        private static Guid CreateGuid(
            int value
        )
        {
            string suffix =
                value.ToString("D12");

            return Guid.Parse(
                $"00000000-0000-0000-0000-{suffix}"
            );
        }

        private sealed class QueueIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public QueueIdSource(
                params Guid[] ids
            )
            {
                this.ids =
                    new Queue<Guid>(ids);
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

        private GameSession CreateSessionWithDeliveryZone(
     int requiredAmount = 1
 )
        {
            var idSource =
                new QueueIdSource(
                    CreateGuid(1)
                );

            var instanceFactory =
                new CardInstanceFactory(idSource);

            var deckBuilder =
                new DeckRuntimeBuilder(instanceFactory);

            var movementService =
                new CardMovementService();

            var drawService =
                new CardDrawService(movementService);

            var builder =
                new GameSessionBuilder(
                    deckBuilder,
                    drawService
                );

            var entries =
                new[]
                {
            new DeckEntry(
                firstDefinition,
                quantity: 1
            )
                };

            var deliveryZoneConfig =
                new CardDeliveryZoneConfig(
                    firstDefinition,
                    requiredAmount
                );

            return builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize: 1,
                    maxHandSize: 8,
                    deliveryZones: new[]
                    {
                deliveryZoneConfig
                    }
                ),
                new SeededRandomSource(12345)
            );
        }

        private GameSession CreateSessionWithTwoDeliveryZones()
        {
            Guid[] ids =
            {
        CreateGuid(1),
        CreateGuid(2)
    };

            var idSource =
                new QueueIdSource(ids);

            var instanceFactory =
                new CardInstanceFactory(idSource);

            var deckBuilder =
                new DeckRuntimeBuilder(instanceFactory);

            var movementService =
                new CardMovementService();

            var drawService =
                new CardDrawService(movementService);

            var builder =
                new GameSessionBuilder(
                    deckBuilder,
                    drawService
                );

            var entries =
                new[]
                {
            new DeckEntry(
                firstDefinition,
                quantity: 1
            ),
            new DeckEntry(
                secondDefinition,
                quantity: 1
            )
                };

            var firstZoneConfig =
                new CardDeliveryZoneConfig(
                    firstDefinition,
                    requiredAmount: 1
                );

            var secondZoneConfig =
                new CardDeliveryZoneConfig(
                    secondDefinition,
                    requiredAmount: 1
                );

            return builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize: 2,
                    maxHandSize: 8,
                    deliveryZones: new[]
                    {
                firstZoneConfig,
                secondZoneConfig
                    }
                ),
                new SeededRandomSource(12345)
            );
        }
        #endregion
    }
}