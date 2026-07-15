using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Turn
{
    public sealed class DrawPhaseServiceTests
    {
        private CardDefinition definition;
        private DrawPhaseService drawPhaseService;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();

            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(movementService);

            drawPhaseService =
                new DrawPhaseService(drawService);
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
        public void Resolve_WithAvailableCard_DrawsAndEntersMain()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            CardInstance card = CreateCard();
            deck.TryAdd(card);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(result.DrawnCard, Is.SameAs(card));
            Assert.That(result.CanContinueTurn, Is.True);

            Assert.That(deck.IsEmpty, Is.True);
            Assert.That(hand.Cards[0], Is.SameAs(card));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_WithFullHand_EntersMainWithoutDrawing()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(1);

            CardInstance handCard = CreateCard();
            CardInstance deckCard = CreateCard();

            hand.TryAdd(handCard);
            deck.TryAdd(deckCard);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(result.DrawnCard, Is.Null);
            Assert.That(result.CanContinueTurn, Is.True);

            Assert.That(deck.Count, Is.EqualTo(1));
            Assert.That(deck.Cards[0], Is.SameAs(deckCard));

            Assert.That(hand.Count, Is.EqualTo(1));
            Assert.That(hand.Cards[0], Is.SameAs(handCard));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_WithEmptyDeck_ReturnsDeckOutAndStaysInDraw()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime();

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.DeckOut)
            );

            Assert.That(result.DrawnCard, Is.Null);
            Assert.That(result.CanContinueTurn, Is.False);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_AfterDrawingLastCard_AllowsCurrentTurn()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(2);

            deck.TryAdd(CreateCard());

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            Assert.That(deck.IsEmpty, Is.True);
            Assert.That(result.CanContinueTurn, Is.True);

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_OnFollowingTurnWithEmptyDeck_ReturnsDeckOut()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(2);

            deck.TryAdd(CreateCard());

            DrawPhaseResult firstDraw =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                firstDraw.Outcome,
                Is.EqualTo(DrawPhaseOutcome.CardDrawn)
            );

            turn.AdvancePhase(); // Main -> End
            turn.AdvancePhase(); // End -> Turn 2 Draw

            DrawPhaseResult secondDraw =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                secondDraw.Outcome,
                Is.EqualTo(DrawPhaseOutcome.DeckOut)
            );

            Assert.That(turn.TurnNumber, Is.EqualTo(2));

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void Resolve_BeforeTurnStarts_Throws()
        {
            TurnRuntime turn = new TurnRuntime();

            Assert.That(
                () => drawPhaseService.Resolve(
                    turn,
                    new DeckRuntime(),
                    new HandRuntime()
                ),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void Resolve_OutsideDrawPhase_Throws()
        {
            TurnRuntime turn = CreateStartedTurn();
            turn.AdvancePhase();

            Assert.That(
                () => drawPhaseService.Resolve(
                    turn,
                    new DeckRuntime(),
                    new HandRuntime()
                ),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void Constructor_WithNullDrawService_Throws()
        {
            Assert.That(
                () => new DrawPhaseService(null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private TurnRuntime CreateStartedTurn()
        {
            TurnRuntime turn = new TurnRuntime();
            turn.StartFirstTurn();

            return turn;
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