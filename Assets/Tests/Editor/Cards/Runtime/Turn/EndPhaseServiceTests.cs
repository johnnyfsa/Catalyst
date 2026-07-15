using Catalyst.Cards.Runtime.Turn;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Turn
{
    public sealed class EndPhaseServiceTests
    {
        private EndPhaseService endPhaseService;

        [SetUp]
        public void SetUp()
        {
            endPhaseService = new EndPhaseService();
        }

        [Test]
        public void Resolve_FromEndPhase_StartsNextTurnInDraw()
        {
            TurnRuntime turn = CreateEndPhaseTurn();

            EndPhaseResult result =
                endPhaseService.Resolve(turn);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(EndPhaseFailure.None)
            );

            Assert.That(
                result.CompletedTurnNumber,
                Is.EqualTo(1)
            );

            Assert.That(
                result.StartedTurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(
                turn.TurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void Resolve_MultipleTimesCompletesMultipleTurns()
        {
            TurnRuntime turn = CreateEndPhaseTurn();

            EndPhaseResult firstResult =
                endPhaseService.Resolve(turn);

            Assert.That(firstResult.Succeeded, Is.True);
            Assert.That(turn.TurnNumber, Is.EqualTo(2));
            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            turn.AdvancePhase(); // Draw → Main
            turn.AdvancePhase(); // Main → End

            EndPhaseResult secondResult =
                endPhaseService.Resolve(turn);

            Assert.That(secondResult.Succeeded, Is.True);

            Assert.That(
                secondResult.CompletedTurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(
                secondResult.StartedTurnNumber,
                Is.EqualTo(3)
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(3));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void Resolve_BeforeTurnStarts_IsRejected()
        {
            TurnRuntime turn = new TurnRuntime();

            EndPhaseResult result =
                endPhaseService.Resolve(turn);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    EndPhaseFailure.TurnNotStarted
                )
            );

            Assert.That(turn.TurnNumber, Is.Zero);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.NotStarted)
            );
        }

        [Test]
        public void Resolve_DuringDrawPhase_IsRejected()
        {
            TurnRuntime turn = new TurnRuntime();
            turn.StartFirstTurn();

            EndPhaseResult result =
                endPhaseService.Resolve(turn);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    EndPhaseFailure.NotInEndPhase
                )
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void Resolve_DuringMainPhase_IsRejected()
        {
            TurnRuntime turn = new TurnRuntime();

            turn.StartFirstTurn();
            turn.AdvancePhase();

            EndPhaseResult result =
                endPhaseService.Resolve(turn);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    EndPhaseFailure.NotInEndPhase
                )
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_WithNullTurn_ReturnsExplicitFailure()
        {
            EndPhaseResult result =
                endPhaseService.Resolve(null);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(EndPhaseFailure.NullTurn)
            );
        }

        private static TurnRuntime CreateEndPhaseTurn()
        {
            TurnRuntime turn = new TurnRuntime();

            turn.StartFirstTurn();
            turn.AdvancePhase(); // Draw → Main
            turn.AdvancePhase(); // Main → End

            return turn;
        }
    }
}