using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Turn;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionFlowServiceTests
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
        public void NewSession_StartsNotStarted()
        {
            GameSession session = CreateSession(10);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.NotStarted)
            );

            Assert.That(session.IsRunning, Is.False);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );
        }

        [Test]
        public void Start_BeginsRunningSessionInTurnOneDraw()
        {
            GameSession session = CreateSession(10);
            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Running)
            );

            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.Turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void Start_WhenSessionAlreadyStarted_Throws()
        {
            GameSession session = CreateSession(10);
            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            Assert.That(
                () => flowService.Start(session),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ResolveDrawPhase_WithCardAvailable_ContinuesSession()
        {
            GameSession session = CreateSession(10);
            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            int originalDeckCount = session.Deck.Count;
            int originalHandCount = session.Hand.Count;

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(originalDeckCount - 1)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(originalHandCount + 1)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void ResolveDrawPhase_WithEmptyDeck_EndsSessionByDeckOut()
        {
            GameSession session = CreateSession(8);
            GameSessionFlowService flowService =
                CreateFlowService();

            Assert.That(session.Deck.IsEmpty, Is.True);

            flowService.Start(session);

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.DeckOut)
            );

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(session.HasEnded, Is.True);
            Assert.That(session.IsRunning, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.DeckOut)
            );
        }

        [Test]
        public void ResolveDrawPhase_AfterSessionEnded_Throws()
        {
            GameSession session = CreateSession(8);
            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            flowService.ResolveDrawPhase(session);

            Assert.That(session.HasEnded, Is.True);

            Assert.That(
                () => flowService.ResolveDrawPhase(session),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ResolveDrawPhase_BeforeSessionStarts_Throws()
        {
            GameSession session = CreateSession(10);
            GameSessionFlowService flowService =
                CreateFlowService();

            Assert.That(
                () => flowService.ResolveDrawPhase(session),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void Constructor_WithNullDrawPhaseService_Throws()
        {
            Assert.That(
                () => new GameSessionFlowService(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private GameSession CreateSession(int cardCount)
        {
            QueueIdSource idSource =
                new QueueIdSource(cardCount);

            CardInstanceFactory factory =
                new CardInstanceFactory(idSource);

            DeckRuntimeBuilder deckBuilder =
                new DeckRuntimeBuilder(factory);

            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(movementService);

            GameSessionBuilder builder =
                new GameSessionBuilder(
                    deckBuilder,
                    drawService
                );

            return builder.Build(
                new[]
                {
                    new DeckEntry(
                        definition,
                        cardCount
                    )
                },
                new GameSessionConfig(
                    initialHandSize: 8,
                    maxHandSize: 9
                ),
                new SeededRandomSource(12345)
            );
        }

        private GameSessionFlowService CreateFlowService()
        {
            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(movementService);

            DrawPhaseService drawPhaseService =
                new DrawPhaseService(drawService);

            return new GameSessionFlowService(
                drawPhaseService
            );
        }

        private sealed class QueueIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public QueueIdSource(int count)
            {
                ids = new Queue<Guid>();

                for (int index = 0;
                     index < count;
                     index++)
                {
                    string suffix =
                        (index + 1).ToString("D12");

                    ids.Enqueue(
                        Guid.Parse(
                            $"00000000-0000-0000-0000-{suffix}"
                        )
                    );
                }
            }

            public Guid NextId()
            {
                return ids.Dequeue();
            }
        }
    }
}