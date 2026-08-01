using System;
using System.Collections.Generic;
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
                ScriptableObject.CreateInstance<
                    CardDefinition
                >();

            CardMovementService movementService =
                new CardMovementService();

            CardDrawService drawService =
                new CardDrawService(
                    movementService
                );

            drawPhaseService =
                new DrawPhaseService(
                    drawService
                );
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
        public void Resolve_WithThreeMissingCards_DrawsUntilHandIsFull()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 5);
            AddCards(deck, 10);

            HashSet<Guid> originalCardIds =
                CollectCardIds(
                    hand,
                    deck
                );

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(3)
            );

            Assert.That(
                result.DrawnCard,
                Is.Null
            );

            Assert.That(
                result.CanContinueTurn,
                Is.True
            );

            Assert.That(
                hand.Count,
                Is.EqualTo(8)
            );

            Assert.That(
                hand.IsFull,
                Is.True
            );

            Assert.That(
                deck.Count,
                Is.EqualTo(7)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            AssertCardsArePreserved(
                originalCardIds,
                hand,
                deck
            );
        }

        [Test]
        public void Resolve_WithOneMissingCard_DrawsExactlyOne()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 7);
            AddCards(deck, 10);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(1)
            );

            Assert.That(
                hand.Count,
                Is.EqualTo(8)
            );

            Assert.That(
                deck.Count,
                Is.EqualTo(9)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_WithPartialDeck_DrawsAvailableCardsAndReturnsDeckOut()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 5);
            AddCards(deck, 2);

            HashSet<Guid> originalCardIds =
                CollectCardIds(
                    hand,
                    deck
                );

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.DeckOut
                )
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(2)
            );

            Assert.That(
                result.CanContinueTurn,
                Is.False
            );

            Assert.That(
                hand.Count,
                Is.EqualTo(7)
            );

            Assert.That(
                hand.IsFull,
                Is.False
            );

            Assert.That(
                deck.IsEmpty,
                Is.True
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            Assert.That(
                turn.TurnNumber,
                Is.EqualTo(1)
            );

            AssertCardsArePreserved(
                originalCardIds,
                hand,
                deck
            );
        }

        [Test]
        public void Resolve_WithEmptyDeck_ReturnsDeckOutWithoutDrawing()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 7);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.DeckOut
                )
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(0)
            );

            Assert.That(
                hand.Count,
                Is.EqualTo(7)
            );

            Assert.That(
                deck.IsEmpty,
                Is.True
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            Assert.That(
                turn.TurnNumber,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void Resolve_WithFullHand_EntersMainWithoutDrawing()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 8);
            AddCards(deck, 3);

            CardInstance[] originalHand =
                CopyCards(hand);

            CardInstance[] originalDeck =
                CopyCards(deck);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(0)
            );

            Assert.That(
                hand.Cards,
                Is.EqualTo(originalHand)
            );

            Assert.That(
                deck.Cards,
                Is.EqualTo(originalDeck)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_WhenLastDeckCardFillsHand_ReturnsHandFull()
        {
            TurnRuntime turn = CreateStartedTurn();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 7);
            AddCards(deck, 1);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(1)
            );

            Assert.That(
                hand.Count,
                Is.EqualTo(8)
            );

            Assert.That(
                deck.IsEmpty,
                Is.True
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_BeforeTurnStarts_ThrowsWithoutMutation()
        {
            TurnRuntime turn = new TurnRuntime();
            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 5);
            AddCards(deck, 3);

            CardInstance[] originalHand =
                CopyCards(hand);

            CardInstance[] originalDeck =
                CopyCards(deck);

            Assert.That(
                () => drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                ),
                Throws.TypeOf<
                    InvalidOperationException
                >()
            );

            Assert.That(
                hand.Cards,
                Is.EqualTo(originalHand)
            );

            Assert.That(
                deck.Cards,
                Is.EqualTo(originalDeck)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.NotStarted)
            );
        }

        [Test]
        public void Resolve_OutsideDrawPhase_ThrowsWithoutMutation()
        {
            TurnRuntime turn = CreateStartedTurn();
            turn.AdvancePhase();

            DeckRuntime deck = new DeckRuntime();
            HandRuntime hand = new HandRuntime(8);

            AddCards(hand, 5);
            AddCards(deck, 3);

            CardInstance[] originalHand =
                CopyCards(hand);

            CardInstance[] originalDeck =
                CopyCards(deck);

            Assert.That(
                () => drawPhaseService.Resolve(
                    turn,
                    deck,
                    hand
                ),
                Throws.TypeOf<
                    InvalidOperationException
                >()
            );

            Assert.That(
                hand.Cards,
                Is.EqualTo(originalHand)
            );

            Assert.That(
                deck.Cards,
                Is.EqualTo(originalDeck)
            );

            Assert.That(
                turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        [Test]
        public void Resolve_WithNullTurn_Throws()
        {
            Assert.That(
                () => drawPhaseService.Resolve(
                    null,
                    new DeckRuntime(),
                    new HandRuntime()
                ),
                Throws.TypeOf<
                    ArgumentNullException
                >()
            );
        }

        [Test]
        public void Resolve_WithNullDeck_Throws()
        {
            Assert.That(
                () => drawPhaseService.Resolve(
                    CreateStartedTurn(),
                    null,
                    new HandRuntime()
                ),
                Throws.TypeOf<
                    ArgumentNullException
                >()
            );
        }

        [Test]
        public void Resolve_WithNullHand_Throws()
        {
            Assert.That(
                () => drawPhaseService.Resolve(
                    CreateStartedTurn(),
                    new DeckRuntime(),
                    null
                ),
                Throws.TypeOf<
                    ArgumentNullException
                >()
            );
        }

        [Test]
        public void Constructor_WithNullDrawService_Throws()
        {
            Assert.That(
                () => new DrawPhaseService(null),
                Throws.TypeOf<
                    ArgumentNullException
                >()
            );
        }

        private TurnRuntime CreateStartedTurn()
        {
            TurnRuntime turn =
                new TurnRuntime();

            turn.StartFirstTurn();

            return turn;
        }

        private void AddCards(
            CardZoneRuntime zone,
            int amount
        )
        {
            for (
                int index = 0;
                index < amount;
                index++
            )
            {
                bool added =
                    zone.TryAdd(
                        CreateCard()
                    );

                Assert.That(
                    added,
                    Is.True
                );
            }
        }

        private CardInstance CreateCard()
        {
            return new CardInstance(
                Guid.NewGuid(),
                definition
            );
        }

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

        private static HashSet<Guid> CollectCardIds(
            params CardZoneRuntime[] zones
        )
        {
            var ids =
                new HashSet<Guid>();

            foreach (
                CardZoneRuntime zone
                in zones
            )
            {
                foreach (
                    CardInstance card
                    in zone.Cards
                )
                {
                    bool added =
                        ids.Add(
                            card.InstanceId
                        );

                    Assert.That(
                        added,
                        Is.True,
                        "The setup contains a duplicated card ID."
                    );
                }
            }

            return ids;
        }

        private static void AssertCardsArePreserved(
            HashSet<Guid> originalCardIds,
            params CardZoneRuntime[] zones
        )
        {
            var finalCardIds =
                new HashSet<Guid>();

            int finalCardCount = 0;

            foreach (
                CardZoneRuntime zone
                in zones
            )
            {
                foreach (
                    CardInstance card
                    in zone.Cards
                )
                {
                    finalCardCount++;

                    bool added =
                        finalCardIds.Add(
                            card.InstanceId
                        );

                    Assert.That(
                        added,
                        Is.True,
                        "A card instance appears more than once across the tested zones."
                    );
                }
            }

            Assert.That(
                finalCardCount,
                Is.EqualTo(
                    originalCardIds.Count
                ),
                "A card was lost or duplicated."
            );

            Assert.That(
                finalCardIds,
                Is.EquivalentTo(
                    originalCardIds
                )
            );
        }
    }
}