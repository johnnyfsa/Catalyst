using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Delivery;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Cards.Runtime.Discard;
using NUnit.Framework;
using UnityEngine;


namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class GameSessionFlowServiceTests
    {
        private CardDefinition acceptedDefinition;
        private CardDefinition otherDefinition;

        private CardMovementService movementService;
        private CardDrawService drawService;
        private DrawPhaseService drawPhaseService;
        private MainPhaseService mainPhaseService;
        private ManualDiscardService manualDiscardService;
        private EndPhaseService endPhaseService;
        private CardDeliveryService cardDeliveryService;

        private GameSessionFlowService flowService;

        [SetUp]
        public void SetUp()
        {
            acceptedDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            otherDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            movementService =
                new CardMovementService();

            drawService =
                new CardDrawService(
                    movementService
                );

            drawPhaseService =
                new DrawPhaseService(
                    drawService
                );

            mainPhaseService =
                new MainPhaseService(
                    movementService
                );

            manualDiscardService =
                new ManualDiscardService(
                    movementService
                );

            endPhaseService =
                new EndPhaseService();

            cardDeliveryService =
                new CardDeliveryService(
                    movementService
                );

            flowService =
                new GameSessionFlowService(
                    drawPhaseService,
                    mainPhaseService,
                    manualDiscardService,
                    endPhaseService,
                    cardDeliveryService
                );
        }

        [TearDown]
        public void TearDown()
        {
            if (acceptedDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    acceptedDefinition
                );
            }

            if (otherDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    otherDefinition
                );
            }
        }

        [Test]
        public void TryDeliverCard_WithSessionNotStarted_Throws()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            CardInstance card =
                session.Hand.Cards[0];

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            Assert.That(
                () => flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                ),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void TryDeliverCard_OutsideMainPhase_Throws()
        {
            GameSession session =
                CreateSessionWithDeliveryZone();

            flowService.Start(session);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            CardInstance card =
                session.Hand.Cards[0];

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            Assert.That(
                () => flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                ),
                Throws.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void TryDiscard_FromHand_MovesCardToDiscardPile()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardInstance card =
                session.Hand.Cards[0];

            int sessionCardCount =
                session.SessionCards.Count;

            ManualDiscardResult result =
                flowService.TryDiscard(
                    session,
                    card,
                    session.Hand
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(session.Hand.Contains(card), Is.False);
            Assert.That(session.DiscardPile.Contains(card), Is.True);

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(sessionCardCount)
            );

            Assert.That(
                session.ContainsCard(card.InstanceId),
                Is.True
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryDiscard_FromReactionTable_MovesCardToDiscardPile()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardInstance card =
                session.Hand.Cards[0];

            CardMovementResult preparationResult =
                movementService.TryMove(
                    card,
                    session.Hand,
                    session.ReactionTable
                );

            Assert.That(
                preparationResult.Succeeded,
                Is.True
            );

            int sessionCardCount =
                session.SessionCards.Count;

            ManualDiscardResult result =
                flowService.TryDiscard(
                    session,
                    card,
                    session.ReactionTable
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.ReactionTable.Contains(card),
                Is.False
            );

            Assert.That(
                session.DiscardPile.Contains(card),
                Is.True
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(sessionCardCount)
            );

            Assert.That(
                session.ContainsCard(card.InstanceId),
                Is.True
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryAdvanceTurn_WithValidMainPhase_EndsTurnAndEntersDraw()
        {
            GameSession session =
            CreateSession(
                handCapacity: 8,
                initialHandSize: 5,
                remainingDeckCards: 4
            );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                drawResult.DrawnCardCount,
                Is.EqualTo(3)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(8));
            Assert.That(session.Deck.Count, Is.EqualTo(1));
            Assert.That(session.Turn.TurnNumber, Is.EqualTo(1));

            CardInstance tableCard =
                session.Hand.Cards[
                    session.Hand.Count - 1
                ];

            CardMovementResult preparationResult =
                movementService.TryMove(
                    tableCard,
                    session.Hand,
                    session.ReactionTable
                );

            Assert.That(
                preparationResult.Succeeded,
                Is.True
            );

            Assert.That(session.Hand.Count, Is.EqualTo(7));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(1));

            CardInstance discardedCard =
                session.Hand.Cards[0];

            ManualDiscardResult discardResult =
                flowService.TryDiscard(
                    session,
                    discardedCard,
                    session.Hand
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(6));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(1));

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(session);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(TurnAdvanceFailure.None)
            );

            Assert.That(
                result.MainPhaseFailure,
                Is.EqualTo(MainPhaseEndFailure.None)
            );

            Assert.That(
                result.EndPhaseFailure,
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

            Assert.That(session.Hand.Count, Is.EqualTo(7));
            Assert.That(session.Hand.Contains(tableCard), Is.True);
            Assert.That(session.ReactionTable.IsEmpty, Is.True);

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryAdvanceTurn_WhenProjectedHandIsFull_FailsWithoutAdvancingTurn()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            flowService.ResolveDrawPhase(session);

            CardInstance firstTableCard =
                session.Hand.Cards[6];

            CardInstance secondTableCard =
                session.Hand.Cards[7];

            CardMovementService preparationMovementService =
                new CardMovementService();

            Assert.That(
                preparationMovementService.TryMove(
                    firstTableCard,
                    session.Hand,
                    session.ReactionTable
                ).Succeeded,
                Is.True
            );

            Assert.That(
                preparationMovementService.TryMove(
                    secondTableCard,
                    session.Hand,
                    session.ReactionTable
                ).Succeeded,
                Is.True
            );

            Assert.That(session.Hand.Count, Is.EqualTo(6));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(2));

            CardInstance[] originalHand =
                CopyCards(session.Hand);

            CardInstance[] originalTable =
                CopyCards(session.ReactionTable);

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(session);

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    TurnAdvanceFailure.MainPhaseCouldNotEnd
                )
            );

            Assert.That(
                result.MainPhaseFailure,
                Is.EqualTo(MainPhaseEndFailure.HandFull)
            );

            Assert.That(
                result.EndPhaseFailure,
                Is.EqualTo(EndPhaseFailure.None)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(1)
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
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryAdvanceTurn_BeforeMaximumTurn_StartsNextPlayableTurn()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 8,
                    maximumTurns: 3
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(
                    session
                );

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            PrepareMainPhaseForTurnAdvance(
                session,
                flowService
            );

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(
                    session
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.CompletedTurnNumber,
                Is.EqualTo(1)
            );

            Assert.That(
                result.StartedTurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryAdvanceTurn_OnMaximumTurn_EndsSession()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 12,
                    maximumTurns: 3
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            AdvanceRunningSessionToMainPhase(
                session,
                flowService,
                targetTurn: 3
            );

            PrepareMainPhaseForTurnAdvance(
                session,
                flowService
            );

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(
                    session
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.CompletedTurnNumber,
                Is.EqualTo(3)
            );

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(session.HasEnded, Is.True);
            Assert.That(session.IsRunning, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryAdvanceTurn_OnMaximumTurn_PreventsNextDrawResolution()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 6,
                    maximumTurns: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(
                    session
                );

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            PrepareMainPhaseForTurnAdvance(
                session,
                flowService
            );

            int handCountBeforeAdvance =
                session.Hand.Count;

            int deckCountBeforeAdvance =
                session.Deck.Count;

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(
                    session
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(session.HasEnded, Is.True);

            Assert.That(
                () => flowService.ResolveDrawPhase(
                    session
                ),
                Throws.TypeOf<
                    InvalidOperationException
                >()
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(handCountBeforeAdvance)
            );

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(deckCountBeforeAdvance)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void SessionEndedByTurnLimit_PreservesOriginalReasonAfterRejectedOperation()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 6,
                    maximumTurns: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(
                    session
                );

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            PrepareMainPhaseForTurnAdvance(
                session,
                flowService
            );

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(
                    session
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            Assert.That(
                () => flowService.TryAdvanceTurn(
                    session
                ),
                Throws.TypeOf<
                    InvalidOperationException
                >()
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryAdvanceTurn_OnSuccess_DoesNotResolveNewDrawPhase()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 5,
                    remainingDeckCards: 3
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                drawResult.DrawnCardCount,
                Is.EqualTo(3)
            );

            ManualDiscardResult discardResult =
                flowService.TryDiscard(
                    session,
                    session.Hand.Cards[0],
                    session.Hand
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(7));

            int deckCountBeforeAdvance =
                session.Deck.Count;

            int handCountBeforeAdvance =
                session.Hand.Count;

            TurnAdvanceResult result =
                flowService.TryAdvanceTurn(session);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(deckCountBeforeAdvance)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(handCountBeforeAdvance)
            );
        }

        [Test]
        public void TryDiscard_WithForeignHand_ReturnsExplicitFailure()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            HandRuntime foreignHand =
                new HandRuntime();

            CardInstance card =
                session.Hand.Cards[0];

            ManualDiscardResult result =
                flowService.TryDiscard(
                    session,
                    card,
                    foreignHand
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ManualDiscardFailure.UnsupportedSource
                )
            );

            Assert.That(
                session.Hand.Contains(card),
                Is.True
            );

            Assert.That(
                session.DiscardPile.IsEmpty,
                Is.True
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryDiscard_WithCardAbsentFromOwnedSource_FailsWithoutMutation()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardInstance card =
                session.Hand.Cards[0];

            ManualDiscardResult result =
                flowService.TryDiscard(
                    session,
                    card,
                    session.ReactionTable
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ManualDiscardFailure.CardNotInSource
                )
            );

            Assert.That(
                session.Hand.Contains(card),
                Is.True
            );

            Assert.That(
                session.ReactionTable.IsEmpty,
                Is.True
            );

            Assert.That(
                session.DiscardPile.IsEmpty,
                Is.True
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryEndMainPhase_WithFiveInHandAndTwoOnTable_Succeeds()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            flowService.ResolveDrawPhase(session);

            CardMovementService movementService =
                new CardMovementService();

            CardInstance firstTableCard =
                session.Hand.Cards[6];

            CardInstance secondTableCard =
                session.Hand.Cards[7];

            Assert.That(
                movementService.TryMove(
                    firstTableCard,
                    session.Hand,
                    session.ReactionTable
                ).Succeeded,
                Is.True
            );

            Assert.That(
                movementService.TryMove(
                    secondTableCard,
                    session.Hand,
                    session.ReactionTable
                ).Succeeded,
                Is.True
            );

            Assert.That(session.Hand.Count, Is.EqualTo(6));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(2));

            CardInstance discardedCard =
                session.Hand.Cards[0];

            ManualDiscardResult discardResult =
                flowService.TryDiscard(
                    session,
                    discardedCard,
                    session.Hand
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(5));

            MainPhaseEndResult result =
                flowService.TryEndMainPhase(session);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(7));
            Assert.That(session.ReactionTable.IsEmpty, Is.True);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            Assert.That(session.Turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryEndMainPhase_WithSixInHandAndTwoOnTable_FailsWithoutMutation()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            flowService.ResolveDrawPhase(session);

            CardMovementService movementService =
                new CardMovementService();

            CardInstance firstTableCard =
                session.Hand.Cards[6];

            CardInstance secondTableCard =
                session.Hand.Cards[7];

            Assert.That(
                movementService.TryMove(
                    firstTableCard,
                    session.Hand,
                    session.ReactionTable
                ).Succeeded,
                Is.True
            );

            Assert.That(
                movementService.TryMove(
                    secondTableCard,
                    session.Hand,
                    session.ReactionTable
                ).Succeeded,
                Is.True
            );

            Assert.That(session.Hand.Count, Is.EqualTo(6));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(2));

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
                session.Hand.Cards,
                Is.EqualTo(originalHand)
            );

            Assert.That(
                session.ReactionTable.Cards,
                Is.EqualTo(originalTable)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(session.Turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryEndMainPhase_WithSevenInHandAndEmptyTable_Succeeds()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            ManualDiscardResult discardResult =
                flowService.TryDiscard(
                    session,
                    session.Hand.Cards[0],
                    session.Hand
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(7));
            Assert.That(session.ReactionTable.IsEmpty, Is.True);

            MainPhaseEndResult result =
                flowService.TryEndMainPhase(session);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
            );

            Assert.That(session.Turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryEndMainPhase_WithFullHandAndEmptyTable_Fails()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            flowService.ResolveDrawPhase(session);

            Assert.That(session.Hand.Count, Is.EqualTo(8));
            Assert.That(session.ReactionTable.IsEmpty, Is.True);

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

            Assert.That(session.Turn.TurnNumber, Is.EqualTo(1));

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryDeliverCard_WithForeignZone_ReturnsExplicitFailure()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardInstance card =
                session.Hand.Cards[0];

            var foreignZone =
                new CardDeliveryZoneRuntime(
                    card.Definition,
                    requiredAmount: 1
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    foreignZone
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardDeliveryFailure
                        .DeliveryZoneDoesNotBelongToSession
                )
            );
        }

        [Test]
        public void TryDeliverCard_WithEquivalentForeignZone_ReturnsExplicitFailure()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardDeliveryZoneRuntime ownedZone =
                session.DeliveryZones[0];

            var foreignZone =
                new CardDeliveryZoneRuntime(
                    ownedZone.AcceptedDefinition,
                    ownedZone.RequiredAmount
                );

            CardInstance card =
                session.Hand.Cards[0];

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    foreignZone
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardDeliveryFailure
                        .DeliveryZoneDoesNotBelongToSession
                )
            );
        }

        [Test]
        public void TryDeliverCard_WithForeignZone_DoesNotMutateSession()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardInstance card =
                session.Hand.Cards[0];

            int initialHandCount =
                session.Hand.Count;

            CardDeliveryZoneRuntime ownedZone =
                session.DeliveryZones[0];

            var foreignZone =
                new CardDeliveryZoneRuntime(
                    ownedZone.AcceptedDefinition,
                    ownedZone.RequiredAmount
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    foreignZone
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(session.Hand.Count, Is.EqualTo(initialHandCount));
            Assert.That(session.Hand.Contains(card), Is.True);
            Assert.That(ownedZone.IsEmpty, Is.True);
            Assert.That(foreignZone.IsEmpty, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void ResolveEndPhase_WithoutTurnLimit_StartsNextTurn()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 3,
                    maximumTurns: null
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            AdvanceToEndPhase(session, flowService);

            EndPhaseResult result =
                flowService.ResolveEndPhase(
                    session
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Running)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Draw)
            );
        }

        [Test]
        public void ResolveEndPhase_BeforeMaximumTurn_KeepsSessionRunning()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 3,
                    maximumTurns: 2
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(1)
            );

            AdvanceToEndPhase(session, flowService);

            EndPhaseResult result =
                flowService.ResolveEndPhase(
                    session
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void ResolveEndPhase_OnMaximumTurn_EndsSession()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 4,
                    maximumTurns: 2
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            AdvanceToEndPhase(session, flowService);

            EndPhaseResult firstEnd =
                flowService.ResolveEndPhase(
                    session
                );

            Assert.That(firstEnd.Succeeded, Is.True);
            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.Turn.TurnNumber, Is.EqualTo(2));

            AdvanceToEndPhase(session, flowService);

            EndPhaseResult secondEnd =
                flowService.ResolveEndPhase(
                    session
                );

            Assert.That(secondEnd.Succeeded, Is.True);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(session.HasEnded, Is.True);
            Assert.That(session.IsRunning, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );
        }

        [Test]
        public void ResolveEndPhase_WithOneTurnLimit_EndsAfterFirstTurn()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 3,
                    maximumTurns: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(1)
            );

            AdvanceToEndPhase(session, flowService);

            EndPhaseResult result =
                flowService.ResolveEndPhase(
                    session
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            Assert.That(session.HasEnded, Is.True);
        }


        [Test]
        public void SessionEndedByMaximumTurns_RejectsFurtherFlowOperations()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 3,
                    maximumTurns: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            AdvanceToEndPhase(session, flowService);
            flowService.ResolveEndPhase(session);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            Assert.That(
                () => flowService.ResolveDrawPhase(
                    session
                ),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );
        }

        [Test]
        public void CompletingMissionOnMaximumTurn_PreservesVictory()
        {
            GameSession session =
                CreateRunningSessionInMainPhase(
                    requiredAmount: 1,
                    maximumTurns: 1
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MissionCompleted
                )
            );

            Assert.That(
                () => flowService.ResolveEndPhase(
                    session
                ),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MissionCompleted
                )
            );
        }

        [Test]
        public void TryDeliverCard_WithWrongDefinition_ReturnsExplicitFailure()
        {
            GameSession session =
                CreateRunningMixedSessionInMainPhase();

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance incompatibleCard =
                session.Hand.Cards.FirstOrDefault(
                    card => !ReferenceEquals(
                        card.Definition,
                        zone.AcceptedDefinition
                    )
                );

            Assert.That(
                incompatibleCard,
                Is.Not.Null,
                "The test session must contain an incompatible card in the hand."
            );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    incompatibleCard,
                    zone
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardDeliveryFailure
                        .DeliveryZoneRejectedCard
                )
            );

            Assert.That(
                session.Hand.Cards,
                Does.Contain(incompatibleCard)
            );

            Assert.That(zone.IsEmpty, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryDeliverCard_WithCardNotInHand_ReturnsExplicitFailure()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance externalCard =
                new CardInstance(
                    Guid.NewGuid(),
                    zone.AcceptedDefinition
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    externalCard,
                    zone
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardDeliveryFailure.CardNotInSource
                )
            );

            Assert.That(zone.IsEmpty, Is.True);
        }

        [Test]
        public void TryDeliverCard_WithValidCard_MovesCardToDeliveryZone()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(session.Hand.Contains(card), Is.False);
            Assert.That(zone.Contains(card), Is.True);
            Assert.That(zone.CurrentAmount, Is.EqualTo(1));
        }

        [Test]
        public void TryDeliverCard_WithValidCard_PreservesValidSessionState()
        {
            GameSession session =
                CreateRunningSessionInMainPhase();

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryDeliverCard_CompletingMission_EndsSessionImmediately()
        {
            GameSession session =
                CreateRunningSessionInMainPhase(
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(zone.IsCompleted, Is.True);
            Assert.That(session.Mission.IsCompleted, Is.True);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(session.IsRunning, Is.False);
            Assert.That(session.HasEnded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MissionCompleted
                )
            );
        }

        [Test]
        public void TryDeliverCard_WithIncompleteMission_KeepsSessionRunning()
        {
            GameSession session =
                CreateRunningSessionInMainPhase(
                    requiredAmount: 2
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(zone.CurrentAmount, Is.EqualTo(1));
            Assert.That(zone.IsCompleted, Is.False);
            Assert.That(session.Mission.IsCompleted, Is.False);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Running)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );
        }

        [Test]
        public void TryDeliverCard_CompletingOnlyOneObjective_DoesNotEndSession()
        {
            GameSession session =
                CreateRunningSessionWithTwoDeliveryZonesInMainPhase();

            CardDeliveryZoneRuntime firstZone =
                session.DeliveryZones[0];

            CardInstance firstCard =
                session.Hand.Cards.First(
                    card => ReferenceEquals(
                        card.Definition,
                        firstZone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    firstCard,
                    firstZone
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(firstZone.IsCompleted, Is.True);

            Assert.That(
                session.DeliveryZones[1].IsCompleted,
                Is.False
            );

            Assert.That(
                session.Mission.IsCompleted,
                Is.False
            );

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Running)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );
        }

        [Test]
        public void TryDeliverCard_CompletingLastObjective_EndsSession()
        {
            GameSession session =
                CreateRunningSessionWithTwoDeliveryZonesInMainPhase();

            CardDeliveryZoneRuntime firstZone =
                session.DeliveryZones[0];

            CardDeliveryZoneRuntime secondZone =
                session.DeliveryZones[1];

            CardInstance firstCard =
                session.Hand.Cards.First(
                    card => ReferenceEquals(
                        card.Definition,
                        firstZone.AcceptedDefinition
                    )
                );

            CardInstance secondCard =
                session.Hand.Cards.First(
                    card => ReferenceEquals(
                        card.Definition,
                        secondZone.AcceptedDefinition
                    )
                );

            CardDeliveryResult firstResult =
                flowService.TryDeliverCard(
                    session,
                    firstCard,
                    firstZone
                );

            Assert.That(firstResult.Succeeded, Is.True);
            Assert.That(session.IsRunning, Is.True);

            CardDeliveryResult secondResult =
                flowService.TryDeliverCard(
                    session,
                    secondCard,
                    secondZone
                );

            Assert.That(secondResult.Succeeded, Is.True);

            Assert.That(firstZone.IsCompleted, Is.True);
            Assert.That(secondZone.IsCompleted, Is.True);
            Assert.That(session.Mission.IsCompleted, Is.True);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MissionCompleted
                )
            );
        }

        [Test]
        public void TryDeliverCard_WithRejectedCard_DoesNotEndSession()
        {
            GameSession session =
                CreateRunningMixedSessionInMainPhase(
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance incompatibleCard =
                session.Hand.Cards.FirstOrDefault(
                    card => !ReferenceEquals(
                        card.Definition,
                        zone.AcceptedDefinition
                    )
                );

            Assert.That(
                incompatibleCard,
                Is.Not.Null,
                "The test session must contain an incompatible card."
            );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    incompatibleCard,
                    zone
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    CardDeliveryFailure
                        .DeliveryZoneRejectedCard
                )
            );

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Running)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(zone.IsEmpty, Is.True);
        }

        [Test]
        public void CompletedMission_PreventsLaterDeckOutCheck()
        {
            GameSession session =
                CreateRunningSessionInMainPhase(
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MissionCompleted
                )
            );

            Assert.That(
                () => flowService.ResolveDrawPhase(
                    session
                ),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MissionCompleted
                )
            );
        }

        [Test]
        public void TryDeliverCard_CompletingMission_PreservesValidSessionState()
        {
            GameSession session =
                CreateRunningSessionInMainPhase(
                    requiredAmount: 1
                );

            CardDeliveryZoneRuntime zone =
                session.DeliveryZones[0];

            CardInstance card =
                session.Hand.Cards.First(
                    candidate => ReferenceEquals(
                        candidate.Definition,
                        zone.AcceptedDefinition
                    )
                );

            CardDeliveryResult result =
                flowService.TryDeliverCard(
                    session,
                    card,
                    zone
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );

            Assert.That(
                session.ContainsCard(card.InstanceId),
                Is.True
            );

            Assert.That(
                session.Hand.Contains(card),
                Is.False
            );

            Assert.That(
                zone.Contains(card),
                Is.True
            );
        }

        [Test]
        public void Constructor_WithNullCardDeliveryService_Throws()
        {
            Assert.That(
                () => new GameSessionFlowService(
                    drawPhaseService,
                    mainPhaseService,
                    manualDiscardService,
                    endPhaseService,
                    cardDeliveryService: null
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
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
        public void ResolveDrawPhase_FillsHandAndContinuesSession()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 5,
                    remainingDeckCards: 10
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            int originalSessionCardCount =
                session.SessionCards.Count;

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(3)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(8));
            Assert.That(session.Deck.Count, Is.EqualTo(7));

            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(originalSessionCardCount)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void ResolveDrawPhase_WithPartialDeck_EndsSessionWithDeckOut()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 5,
                    remainingDeckCards: 2
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            int originalSessionCardCount =
                session.SessionCards.Count;

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(session.IsRunning, Is.False);
            Assert.That(session.HasEnded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.DeckOut)
            );
        }

        [Test]
        public void ResolveDrawPhase_WithEmptyDeck_EndsSessionWithDeckOut()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 0
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.DeckOut)
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(0)
            );

            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Ended)
            );

            Assert.That(session.IsRunning, Is.False);
            Assert.That(session.HasEnded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.DeckOut)
            );
        }

        [Test]
        public void ResolveDrawPhase_WhenLastDeckCardFillsHand_KeepsSessionRunning()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 7,
                    remainingDeckCards: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(1)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(8));
            Assert.That(session.Deck.IsEmpty, Is.True);
            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void ResolveDrawPhase_WithFullHandAndEmptyDeck_KeepsSessionRunning()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 8,
                    initialHandSize: 8,
                    remainingDeckCards: 0
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                result.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                result.DrawnCardCount,
                Is.EqualTo(0)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(8));
            Assert.That(session.Deck.IsEmpty, Is.True);
            Assert.That(session.IsRunning, Is.True);
            Assert.That(session.HasEnded, Is.False);

            Assert.That(
                session.EndReason,
                Is.EqualTo(GameSessionEndReason.None)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
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
        public void ResolveDrawPhase_AfterSessionEnded_ThrowsWithoutMutation()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 1,
                    remainingDeckCards: 3,
                    maximumTurns: 1
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);
            AdvanceToEndPhase(session, flowService);
            flowService.ResolveEndPhase(session);

            Assert.That(session.HasEnded, Is.True);

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );

            int originalHandCount =
                session.Hand.Count;

            int originalDeckCount =
                session.Deck.Count;

            GamePhase originalPhase =
                session.Turn.CurrentPhase;

            Assert.That(
                () => flowService.ResolveDrawPhase(session),
                Throws.TypeOf<InvalidOperationException>()
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(originalHandCount)
            );

            Assert.That(
                session.Deck.Count,
                Is.EqualTo(originalDeckCount)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(originalPhase)
            );

            Assert.That(
                session.EndReason,
                Is.EqualTo(
                    GameSessionEndReason.MaxTurnsReached
                )
            );
        }

        [Test]
        public void Constructor_WithNullDrawPhaseService_Throws()
        {
            Assert.That(
                () => new GameSessionFlowService(null, null, null, null, null),
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
                    discardedCard,
                    session.Hand
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
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                secondDraw.DrawnCardCount,
                Is.EqualTo(1)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(2));
            Assert.That(session.Deck.IsEmpty, Is.True);

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryEndMainPhase_WithCardsOnTable_ReturnsCardsAndAdvancesToEnd()
        {
            GameSession session =
                CreateSession(
                    handCapacity: 4,
                    initialHandSize: 2,
                    remainingDeckCards: 2
                );

            GameSessionFlowService flowService =
                CreateFlowService();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                drawResult.DrawnCardCount,
                Is.EqualTo(2)
            );

            Assert.That(session.Hand.Count, Is.EqualTo(4));

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
            Assert.That(session.Hand.Count, Is.EqualTo(3));
            Assert.That(session.ReactionTable.Count, Is.EqualTo(1));

            CardInstance discardedCard =
                session.Hand.Cards[0];

            ManualDiscardResult discardResult =
                flowService.TryDiscard(
                    session,
                    discardedCard,
                    session.Hand
                );

            Assert.That(discardResult.Succeeded, Is.True);
            Assert.That(session.Hand.Count, Is.EqualTo(2));

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
                Is.EqualTo(DrawPhaseOutcome.HandFull)
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
    int remainingDeckCards,
    int? maximumTurns = null
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
                    acceptedDefinition,
                    totalCardCount
                )
                    }
                    : Array.Empty<DeckEntry>();

            GameSession session = builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize,
                    handCapacity,
                    maximumTurns: maximumTurns
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

        private void AdvanceToEndPhase(
    GameSession session,
    GameSessionFlowService flowService
)
        {
            if (session.Turn.CurrentPhase == GamePhase.Draw)
            {
                DrawPhaseResult drawResult =
                    flowService.ResolveDrawPhase(session);

                Assert.That(
                    drawResult.Outcome,
                    Is.EqualTo(DrawPhaseOutcome.HandFull)
                );
            }

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            while (session.Hand.IsFull)
            {
                CardInstance card =
                    session.Hand.Cards[0];

                ManualDiscardResult discardResult =
                    flowService.TryDiscard(
                        session,
                        card,
                        session.Hand
                    );

                Assert.That(
                    discardResult.Succeeded,
                    Is.True
                );
            }

            MainPhaseEndResult mainResult =
                flowService.TryEndMainPhase(
                    session
                );

            Assert.That(
                mainResult.Succeeded,
                Is.True
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.End)
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

            MainPhaseService mainPhaseService =
                new MainPhaseService(movementService);

            ManualDiscardService manualDiscardService =
                new ManualDiscardService(movementService);

            EndPhaseService endPhaseService =
                new EndPhaseService();
            CardDeliveryService cardDeliveryService =
                new CardDeliveryService(movementService);

            return new GameSessionFlowService(
                drawPhaseService,
                mainPhaseService,
                manualDiscardService,
                endPhaseService,
                cardDeliveryService
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


        private GameSession CreateSessionWithDeliveryZone(
               int requiredAmount = 2,
               int initialHandSize = 2,
               int maxHandSize = 4,
               int remainingDeckCards = 1,
               int? maximumTurns = null
           )
        {
            int acceptedCardCount =
                initialHandSize + remainingDeckCards;

            var entries =
                new[]
                {
                    new DeckEntry(
                        acceptedDefinition,
                        acceptedCardCount
                    )
                };

            var zoneConfig =
                new CardDeliveryZoneConfig(
                    acceptedDefinition,
                    requiredAmount
                );

            int totalCardCount =
                acceptedCardCount;

            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(
                        totalCardCount
                    )
                );

            return builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize,
                    maxHandSize,
                    deliveryZones: new[]
                    {
                        zoneConfig
                    },
                    maximumTurns: maximumTurns
                ),
                new SeededRandomSource(12345)
            );
        }
        private GameSession CreateSessionWithMixedCards(
    int requiredAmount = 2
)
        {
            var entries =
                new[]
                {
            new DeckEntry(
                acceptedDefinition,
                quantity: 3
            ),
            new DeckEntry(
                otherDefinition,
                quantity: 2
            )
                };

            var zoneConfig =
                new CardDeliveryZoneConfig(
                    acceptedDefinition,
                    requiredAmount
                );

            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(5)
                );

            return builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize: 4,
                    maxHandSize: 5,
                    deliveryZones: new[]
                    {
                zoneConfig
                    }
                ),
                new SeededRandomSource(12345)
            );
        }
        private GameSession CreateRunningSessionInMainPhase(
    int requiredAmount = 2,
    int? maximumTurns = null
)
        {
            GameSession session =
                CreateSessionWithDeliveryZone(
                    requiredAmount: requiredAmount,
                    initialHandSize: 2,
                    maxHandSize: 4,
                    remainingDeckCards: 2,
                    maximumTurns: maximumTurns
                );

            flowService.Start(session);

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(
                    session
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            Assert.That(
                session.State,
                Is.EqualTo(
                    GameSessionState.Running
                )
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            Assert.That(
                session.MaximumTurns,
                Is.EqualTo(maximumTurns)
            );

            return session;
        }
        private GameSession CreateRunningMixedSessionInMainPhase(
            int requiredAmount = 2
        )
        {
            GameSession session =
                CreateSessionWithMixedCards(
                    requiredAmount
                );

            flowService.Start(session);

            DrawPhaseResult result =
                flowService.ResolveDrawPhase(
                    session
                );

            Assert.That(
                result.Outcome,
                Is.EqualTo(
                    DrawPhaseOutcome.HandFull
                )
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            return session;
        }

        private GameSessionBuilder CreateBuilder(
            IEnumerable<Guid> ids
        )
        {
            ICardInstanceIdSource idSource =
                new SequenceIdSource(ids);

            CardInstanceFactory instanceFactory =
                new CardInstanceFactory(
                    idSource
                );

            DeckRuntimeBuilder deckBuilder =
                new DeckRuntimeBuilder(
                    instanceFactory
                );

            return new GameSessionBuilder(
                deckBuilder,
                drawService
            );
        }

        private static Guid[] CreateSequentialIds(
            int count
        )
        {
            Guid[] ids =
                new Guid[count];

            for (
                int index = 0;
                index < count;
                index++
            )
            {
                string suffix =
                    (index + 1)
                    .ToString("D12");

                ids[index] =
                    Guid.Parse(
                        $"00000000-0000-0000-0000-{suffix}"
                    );
            }

            return ids;
        }

        private sealed class SequenceIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public SequenceIdSource(
                IEnumerable<Guid> ids
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

        private GameSession
    CreateRunningSessionWithTwoDeliveryZonesInMainPhase()
        {
            GameSession session =
                CreateSessionWithTwoDeliveryZones();

            flowService.Start(session);

            DrawPhaseResult drawResult =
                flowService.ResolveDrawPhase(session);

            Assert.That(
                drawResult.Outcome,
                Is.EqualTo(DrawPhaseOutcome.HandFull)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            return session;
        }

        private GameSession CreateSessionWithTwoDeliveryZones(
    int remainingDeckCards = 1
)
        {
            const int firstDefinitionCount = 2;
            const int secondDefinitionCount = 2;

            int totalCardCount =
                firstDefinitionCount
                + secondDefinitionCount;

            GameSessionBuilder builder =
                CreateBuilder(
                    CreateSequentialIds(totalCardCount)
                );

            var entries =
                new[]
                {
            new DeckEntry(
                acceptedDefinition,
                firstDefinitionCount
            ),
            new DeckEntry(
                otherDefinition,
                secondDefinitionCount
            )
                };

            var firstZoneConfig =
                new CardDeliveryZoneConfig(
                    acceptedDefinition,
                    requiredAmount: 1
                );

            var secondZoneConfig =
                new CardDeliveryZoneConfig(
                    otherDefinition,
                    requiredAmount: 1
                );

            return builder.Build(
                entries,
                new GameSessionConfig(
                    initialHandSize: 3,
                    maxHandSize: 4,
                    deliveryZones: new[]
                    {
                firstZoneConfig,
                secondZoneConfig
                    }
                ),
                new SeededRandomSource(12345)
            );
        }
        private void PrepareMainPhaseForTurnAdvance(
    GameSession session,
    GameSessionFlowService flowService
)
        {
            Assert.That(
                session.State,
                Is.EqualTo(GameSessionState.Running)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );

            while (session.Hand.IsFull)
            {
                CardInstance card =
                    session.Hand.Cards[0];

                ManualDiscardResult discardResult =
                    flowService.TryDiscard(
                        session,
                        card,
                        session.Hand
                    );

                Assert.That(
                    discardResult.Succeeded,
                    Is.True
                );
            }
        }

        private void AdvanceRunningSessionToMainPhase(
    GameSession session,
    GameSessionFlowService flowService,
    int targetTurn
)
        {
            if (targetTurn <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetTurn)
                );
            }

            if (
                session.Turn.CurrentPhase
                == GamePhase.Draw
            )
            {
                DrawPhaseResult initialDraw =
                    flowService.ResolveDrawPhase(
                        session
                    );

                Assert.That(
                    initialDraw.Outcome,
                    Is.EqualTo(
                        DrawPhaseOutcome.HandFull
                    )
                );
            }

            while (
                session.IsRunning
                && session.Turn.TurnNumber
                    < targetTurn
            )
            {
                PrepareMainPhaseForTurnAdvance(
                    session,
                    flowService
                );

                TurnAdvanceResult advanceResult =
                    flowService.TryAdvanceTurn(
                        session
                    );

                Assert.That(
                    advanceResult.Succeeded,
                    Is.True
                );

                Assert.That(
                    session.IsRunning,
                    Is.True
                );

                Assert.That(
                    session.Turn.CurrentPhase,
                    Is.EqualTo(GamePhase.Draw)
                );

                DrawPhaseResult drawResult =
                    flowService.ResolveDrawPhase(
                        session
                    );

                Assert.That(
                    drawResult.Outcome,
                    Is.EqualTo(
                        DrawPhaseOutcome.HandFull
                    )
                );
            }

            Assert.That(
                session.Turn.TurnNumber,
                Is.EqualTo(targetTurn)
            );

            Assert.That(
                session.Turn.CurrentPhase,
                Is.EqualTo(GamePhase.Main)
            );
        }

        #endregion
    }
}