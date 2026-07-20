using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
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
                () => new GameSessionFlowService(null, null, null, null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void RunningSession_CompletesBasicTurnCycle()
        {
            GameSession session = CreateSession(
                handCapacity: 2,
                initialHandSize: 2,
                remainingDeckCards: 1
            );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult firstDraw =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                firstDraw.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            MainPhaseEndResult blockedEnd =
                flowService.TryEndMainPhase(session);

            Assert.That(blockedEnd.Succeeded, Is.False);

            Assert.That(
                blockedEnd.Failure,
                Is.EqualTo(MainPhaseEndFailure.HandFull)
            );

            CardInstance discardedCard =
                session.Hand.Cards[0];

            ManualDiscardResult discardResult =
                flowService.TryDiscard(
                    session,
                    discardedCard
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(1));
            Assert.That(session.DiscardPile.Count, Is.EqualTo(1));

            MainPhaseEndResult mainEnd =
                flowService.TryEndMainPhase(session);

            Assert.That(mainEnd.Succeeded, Is.True);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            EndPhaseResult endResult =
                flowService.ResolveEndPhase(session);

            Assert.That(endResult.Succeeded, Is.True);
            Assert.That(session.Turn.TurnNumber, Is.EqualTo(2));

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            DrawPhaseResult secondDraw =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                secondDraw.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(2));
            Assert.That(session.Deck.IsEmpty, Is.True);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void TryEndMainPhase_WithCardsOnTable_ReturnsCardsAndAdvancesToEnd()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 2,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(3));

            CardInstance preparedCard =
                session.Hand.Cards[1];

            CardMovementService movementService =
                new CardMovementService();

            CardMovementResult movementResult =
                movementService.TryMove(
                    preparedCard,
                    session.Hand,
                    session.ReactionTable
                );

            Assert.That(movementResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(2));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(1));

            MainPhaseEndResult result =
                flowService.TryEndMainPhase(session);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(MainPhaseEndFailure.None)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            Assert.That(
                session.ReactionTable.IsEmpty,
                Is.True
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(3)
            );

            Assert.That(
                session.Hand.Contains(preparedCard),
                Is.True
            );

            session.ValidateState();
        }
        [Test]
        public void TryEndMainPhase_WhenProjectedHandIsFull_FailsWithoutMutation()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 3,
                    initialHandSize: 2,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(3));
            Assert.That(session.Hand.IsFull, Is.True);

            CardInstance preparedCard =
                session.Hand.Cards[1];

            CardMovementService movementService =
                new CardMovementService();

            CardMovementResult movementResult =
                movementService.TryMove(
                    preparedCard,
                    session.Hand,
                    session.ReactionTable
                );

            Assert.That(movementResult.Succeeded, Is.True);

            Assert.That(session.Hand.Count, Is.EqualTo(2));
            Assert.That(session.Hand.IsFull, Is.False);
            Assert.That(session.ReactionTable.Count, Is.EqualTo(1));

            CardInstance[] originalHand =
                CopyCards(session.Hand);

            CardInstance[] originalTable =
                CopyCards(session.ReactionTable);

            MainPhaseEndResult result =
                flowService.TryEndMainPhase(session);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(MainPhaseEndFailure.HandFull)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                session.Hand.Cards,
                Is.EqualTo(originalHand)
            );

            Assert.That(
                session.ReactionTable.Cards,
                Is.EqualTo(originalTable)
            );

            Assert.That(
                session.Hand.Contains(preparedCard),
                Is.False
            );

            Assert.That(
                session.ReactionTable.Contains(preparedCard),
                Is.True
            );

            session.ValidateState();
        }

        #region Helpers

        private static CardInstance[] CopyCards(
    CardZoneRuntime zone
)
        {
            CardInstance[] cards =
                new CardInstance[zone.Count];

            for (
                int index = 0;
                index < zone.Count;
                index++
            )
            {
                cards[index] =
                    zone.Cards[index];
            }

            return cards;
        }
        private GameSession CreateSession(int cardCount)
        {
            const int initialHandSize = 8;
            const int handCapacity = 9;

            int actualInitialHandSize =
                Math.Min(cardCount, initialHandSize);

            int remainingDeckCards =
                Math.Max(
                    0,
                    cardCount - actualInitialHandSize
                );

            return CreateSession(
                handCapacity,
                actualInitialHandSize,
                remainingDeckCards
            );
        }

        private GameSession CreateSession(
    int handCapacity,
    int initialHandSize,
    int remainingDeckCards
)
        {
            if (handCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(handCapacity)
                );
            }

            if (initialHandSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialHandSize)
                );
            }

            if (initialHandSize > handCapacity)
            {
                throw new ArgumentException(
                    "Initial hand size cannot exceed hand capacity.",
                    nameof(initialHandSize)
                );
            }

            if (remainingDeckCards < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remainingDeckCards)
                );
            }

            int totalCardCount =
                initialHandSize + remainingDeckCards;

            QueueIdSource idSource =
                new QueueIdSource(totalCardCount);

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

            IEnumerable<DeckEntry> entries =
                totalCardCount > 0
                    ? new[]
                    {
                new DeckEntry(
                    definition,
                    totalCardCount
                )
                    }
                    : Array.Empty<DeckEntry>();

            GameSession session = builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize,
                    handCapacity
                ),
                new SeededRandomSource(12345)
            );

            Assert.That(
                session.Hand.Capacity,
                Is.EqualTo(handCapacity)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(initialHandSize)
            );

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(remainingDeckCards)
            );

            return session;
        }

        private GameSessionFlowService CreateFlowService()
        {
            CardMovementService movementService =
    new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(movementService);

            DrawPhaseService drawPhaseService =
                new DrawPhaseService(drawService);

            MainPhaseService mainPhaseService =
                new MainPhaseService(movementService);

            ManualDiscardService manualDiscardService =
                new ManualDiscardService(movementService);

            EndPhaseService endPhaseService =
                new EndPhaseService();

            return new GameSessionFlowService(
                drawPhaseService,
                mainPhaseService,
                manualDiscardService,
                endPhaseService
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
        #endregion
    }
}