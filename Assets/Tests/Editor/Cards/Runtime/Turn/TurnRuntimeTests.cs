using System;
using Catalyst.Cards.Runtime.Turn;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Turn
{
    public sealed class TurnRuntimeTests
    {
        [Test]
        public void NewRuntime_StartsNotStarted()
        {
            TurnRuntime turn = new TurnRuntime();

            Assert.That(turn.HasStarted, Is.False);
            Assert.That(turn.TurnNumber, Is.Zero);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.NotStarted)
            );
        }

        [Test]
        public void StartFirstTurn_StartsTurnOneInDrawPhase()
        {
            TurnRuntime turn = new TurnRuntime();

            turn.StartFirstTurn();

            Assert.That(turn.HasStarted, Is.True);
            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void StartFirstTurn_WhenAlreadyStarted_Throws()
        {
            TurnRuntime turn = new TurnRuntime();

            turn.StartFirstTurn();

            Assert.That(
                () => turn.StartFirstTurn(),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void AdvancePhase_FromDraw_EntersMain()
        {
            TurnRuntime turn = CreateStartedTurn();

            turn.AdvancePhase();

            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void AdvancePhase_FromMain_EntersEnd()
        {
            TurnRuntime turn = CreateStartedTurn();

            turn.AdvancePhase();
            turn.AdvancePhase();

            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );
        }

        [Test]
        public void AdvancePhase_FromEnd_StartsNextTurnInDraw()
        {
            TurnRuntime turn = CreateStartedTurn();

            turn.AdvancePhase();
            turn.AdvancePhase();
            turn.AdvancePhase();

            Assert.That(turn.TurnNumber, Is.EqualTo(2));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void AdvancePhase_BeforeStart_Throws()
        {
            TurnRuntime turn = new TurnRuntime();

            Assert.That(
                () => turn.AdvancePhase(),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(turn.TurnNumber, Is.Zero);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.NotStarted)
            );
        }

        [Test]
        public void MultipleCycles_IncrementTurnOnlyAfterEndPhase()
        {
            TurnRuntime turn = CreateStartedTurn();

            turn.AdvancePhase(); // Main
            turn.AdvancePhase(); // End

            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            turn.AdvancePhase(); // Turn 2 Draw
            turn.AdvancePhase(); // Turn 2 Main
            turn.AdvancePhase(); // Turn 2 End

            Assert.That(turn.TurnNumber, Is.EqualTo(2));

            turn.AdvancePhase(); // Turn 3 Draw

            Assert.That(turn.TurnNumber, Is.EqualTo(3));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        private static TurnRuntime CreateStartedTurn()
        {
            TurnRuntime turn = new TurnRuntime();
            turn.StartFirstTurn();

            return turn;
        }
    }
}