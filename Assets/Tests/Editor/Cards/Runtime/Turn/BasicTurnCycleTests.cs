using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Turn
{
    public sealed class BasicTurnCycleTests
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
        public void FullHand_Discard_EndMainAndStartNextTurn()
        {
            TurnRuntime turn = new TurnRuntime();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(2);

            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardMovementService movementService =
                new CardMovementService();

            DrawPhaseService drawPhaseService =
                new DrawPhaseService(
                    new CardDrawService(movementService)
                );

            MainPhaseService mainPhaseService =
                new MainPhaseService(movementService);

            ManualDiscardService discardService =
                new ManualDiscardService(
                    movementService
                );

            EndPhaseService endPhaseService =
                new EndPhaseService();

            CardInstance firstHandCard = CreateCard();
            CardInstance secondHandCard = CreateCard();
            CardInstance deckCard = CreateCard();

            hand.TryAdd(firstHandCard);
            hand.TryAdd(secondHandCard);
            deck.TryAdd(deckCard);

            turn.StartFirstTurn();

            DrawPhaseResult drawResult =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            MainPhaseEndResult blockedEnd =
                mainPhaseService.TryEnd(
                    turn,
                    hand,
                    reactionTable: new ReactionTableRuntime()
                );

            Assert.That(blockedEnd.Succeeded, Is.False);

            Assert.That(
                blockedEnd.Failure,
                Is.EqualTo(MainPhaseEndFailure.HandFull)
            );

            ManualDiscardResult discardResult =
                discardService.TryDiscard(
                    firstHandCard,
                    hand,
                    discardPile
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(hand.Count, Is.EqualTo(1));

            MainPhaseEndResult mainEndResult =
                mainPhaseService.TryEnd(
                    turn,
                    hand,
                    reactionTable: new ReactionTableRuntime()
                );

            Assert.That(mainEndResult.Succeeded, Is.True);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            EndPhaseResult endPhaseResult =
                endPhaseService.Resolve(turn);

            Assert.That(endPhaseResult.Succeeded, Is.True);

            Assert.That(turn.TurnNumber, Is.EqualTo(2));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            DrawPhaseResult nextDraw =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                nextDraw.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(hand.Count, Is.EqualTo(2));
            Assert.That(deck.IsEmpty, Is.True);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        private CardInstance CreateCard()
        {
            return new CardInstance(
                Guid.NewGuid(),
                definition
            );
        }
    }
}