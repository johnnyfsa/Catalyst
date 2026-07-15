using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.Cards.Runtime.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Turn
{
    public sealed class MainPhaseServiceTests
    {
        private CardDefinition definition;
        private MainPhaseService mainPhaseService;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();

            mainPhaseService = new MainPhaseService();
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
        public void TryEnd_WithAvailableHandSpace_AdvancesToEnd()
        {
            TurnRuntime turn = CreateMainPhaseTurn();
            HandRuntime hand = new HandRuntime(8);

            FillHand(hand, 7);

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(MainPhaseEndFailure.None)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(1));
        }

        [Test]
        public void TryEnd_WithFullHand_IsRejected()
        {
            TurnRuntime turn = CreateMainPhaseTurn();
            HandRuntime hand = new HandRuntime(8);

            FillHand(hand, 8);

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(MainPhaseEndFailure.HandFull)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(hand.Count, Is.EqualTo(8));
        }

        [Test]
        public void TryEnd_WithEmptyHand_AdvancesToEnd()
        {
            TurnRuntime turn = CreateMainPhaseTurn();
            HandRuntime hand = new HandRuntime(8);

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );
        }

        [Test]
        public void TryEnd_AfterRemovingCardFromFullHand_AdvancesToEnd()
        {
            TurnRuntime turn = CreateMainPhaseTurn();
            HandRuntime hand = new HandRuntime(8);

            FillHand(hand, 8);

            CardInstance removedCard = hand.Cards[3];

            bool removed = hand.TryRemove(removedCard);

            Assert.That(removed, Is.True);
            Assert.That(hand.Count, Is.EqualTo(7));
            Assert.That(hand.IsFull, Is.False);

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );
        }

        [Test]
        public void TryEnd_BeforeTurnStarts_IsRejected()
        {
            TurnRuntime turn = new TurnRuntime();
            HandRuntime hand = new HandRuntime();

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    MainPhaseEndFailure.TurnNotStarted
                )
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.NotStarted)
            );
        }

        [Test]
        public void TryEnd_DuringDrawPhase_IsRejected()
        {
            TurnRuntime turn = new TurnRuntime();
            turn.StartFirstTurn();

            HandRuntime hand = new HandRuntime();

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    MainPhaseEndFailure.NotInMainPhase
                )
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void TryEnd_DuringEndPhase_IsRejected()
        {
            TurnRuntime turn = CreateMainPhaseTurn();
            HandRuntime hand = new HandRuntime();

            turn.AdvancePhase();

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, hand);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    MainPhaseEndFailure.NotInMainPhase
                )
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );
        }

        [Test]
        public void TryEnd_WithNullTurn_ReturnsExplicitFailure()
        {
            MainPhaseEndResult result =
                mainPhaseService.TryEnd(
                    null,
                    new HandRuntime()
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(MainPhaseEndFailure.NullTurn)
            );
        }

        [Test]
        public void TryEnd_WithNullHand_ReturnsExplicitFailure()
        {
            TurnRuntime turn = CreateMainPhaseTurn();

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(turn, null);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(MainPhaseEndFailure.NullHand)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }
        [Test]
        public void TryEnd_AfterManualDiscardFromFullHand_AdvancesToEnd()
        {
            TurnRuntime turn = CreateMainPhaseTurn();
            HandRuntime hand = new HandRuntime(2);
            DiscardPileRuntime discardPile =
                new DiscardPileRuntime();

            CardInstance first = new CardInstance(
                Guid.NewGuid(),
                definition
            );

            CardInstance second = new CardInstance(
                Guid.NewGuid(),
                definition
            );

            hand.TryAdd(first);
            hand.TryAdd(second);

            ManualDiscardService discardService =
                new ManualDiscardService(
                    new CardMovementService()
                );

            ManualDiscardResult discardResult =
                discardService.TryDiscard(
                    first,
                    hand,
                    discardPile
                );

            MainPhaseEndResult endResult =
                mainPhaseService.TryEnd(
                    turn,
                    hand
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(endResult.Succeeded, Is.True);

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(discardPile.Count, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );
        }

        private TurnRuntime CreateMainPhaseTurn()
        {
            TurnRuntime turn = new TurnRuntime();

            turn.StartFirstTurn();
            turn.AdvancePhase();

            return turn;
        }

        private void FillHand(
            HandRuntime hand,
            int cardCount
        )
        {
            for (int index = 0;
                 index < cardCount;
                 index++)
            {
                bool added = hand.TryAdd(
                    new CardInstance(
                        Guid.NewGuid(),
                        definition
                    )
                );

                Assert.That(added, Is.True);
            }
        }
    }
}