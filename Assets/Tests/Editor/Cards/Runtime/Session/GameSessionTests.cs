using System;
using System.Collections.Generic;
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
    public sealed class GameSessionTests
    {
        private CardDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (definition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    definition
                );
            }
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
                    definition
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
                    definition
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
                        definition,
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
                definition
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
    }
}