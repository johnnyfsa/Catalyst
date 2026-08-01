using System;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Discard;
using Catalyst.Cards.Runtime.Delivery;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSessionFlowService
    {
        private readonly DrawPhaseService drawPhaseService;
        private readonly MainPhaseService mainPhaseService;
        private readonly ManualDiscardService manualDiscardService;
        private readonly EndPhaseService endPhaseService;

        private readonly CardDeliveryService cardDeliveryService;

        public GameSessionFlowService(
    DrawPhaseService drawPhaseService,
    MainPhaseService mainPhaseService,
    ManualDiscardService manualDiscardService,
    EndPhaseService endPhaseService,
    CardDeliveryService cardDeliveryService
)
        {
            this.drawPhaseService = drawPhaseService
                ?? throw new ArgumentNullException(
                    nameof(drawPhaseService)
                );

            this.mainPhaseService = mainPhaseService
                ?? throw new ArgumentNullException(
                    nameof(mainPhaseService)
                );

            this.manualDiscardService = manualDiscardService
                ?? throw new ArgumentNullException(
                    nameof(manualDiscardService)
                );

            this.endPhaseService = endPhaseService
                ?? throw new ArgumentNullException(
                    nameof(endPhaseService)
                );
            this.cardDeliveryService = cardDeliveryService
                ?? throw new ArgumentNullException(
                    nameof(cardDeliveryService)
                );
        }

        public void Start(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session)
                );
            }

            session.Start();
            session.ValidateState();
        }
        /// <summary>
        /// Resolves the draw phase for the given game session. If the deck is out of cards, the session will end with a DeckOut reason.
        /// </summary>
        /// <param name="session"></param>
        /// <returns>DrawPhaseResult</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public DrawPhaseResult ResolveDrawPhase(
            GameSession session
        )
        {
            EnsureSessionIsRunning(session);

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    session.Turn,
                    session.Deck,
                    session.Hand
                );

            session.ValidateState();

            return result;
        }
        /// <summary>
        /// Attempts to end the main phase for the given game session. If the hand is over the limit, the session will remain in the main phase and return a result indicating that the hand is full.
        /// </summary>
        /// <param name="session"></param>
        /// <returns>MainPhaseEndResult</returns>
        public MainPhaseEndResult TryEndMainPhase(
    GameSession session
)
        {
            EnsureSessionIsRunning(session);

            MainPhaseEndResult result =
                mainPhaseService.TryEnd(
                    session.Turn,
                    session.Hand,
                    session.ReactionTable
                );

            session.ValidateState();

            return result;
        }
        /// <summary>
        /// Attempts to manually discard a card from a supported
        /// session zone to the discard pile. Manual discard is
        /// only available during the Main phase.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="card"></param>
        /// <param name="source"></param>
        /// <returns>ManualDiscardResult</returns>
        public ManualDiscardResult TryDiscard(
    GameSession session,
    CardInstance card,
    CardZoneRuntime source
)
        {
            EnsureSessionIsRunning(session);

            if (session.Turn.CurrentPhase != GamePhase.Main)
            {
                throw new InvalidOperationException(
                    "Cards can only be manually discarded " +
                    "during the Main phase."
                );
            }

            if (!IsOwnedDiscardSource(
                    session,
                    source
                ))
            {
                return ManualDiscardResult.Fail(
                    ManualDiscardFailure.UnsupportedSource
                );
            }

            ManualDiscardResult result =
                manualDiscardService.TryDiscard(
                    card,
                    source,
                    session.DiscardPile
                );

            session.ValidateState();

            return result;
        }
        /// <summary>
        /// Attempts to end the current phase for the given game session. If the session is not running, an exception will be thrown. If the current phase is the end phase, the session will end with a Normal reason.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public EndPhaseResult ResolveEndPhase(
    GameSession session
)
        {
            EnsureSessionIsRunning(session);

            int completedTurnNumber =
                session.Turn.TurnNumber;

            EndPhaseResult result =
                endPhaseService.Resolve(
                    session.Turn
                );

            if (result.Succeeded &&
                HasReachedTurnLimit(
                    session,
                    completedTurnNumber
                ))
            {
                session.End(
                    GameSessionEndReason.MaxTurnsReached
                );
            }

            session.ValidateState();

            return result;
        }

        public CardDeliveryResult TryDeliverCard(
     GameSession session,
     CardInstance card,
     CardDeliveryZoneRuntime deliveryZone
 )
        {
            EnsureSessionIsRunning(session);

            if (session.Turn.CurrentPhase != GamePhase.Main)
            {
                throw new InvalidOperationException(
                    "Cards can only be delivered during the Main phase."
                );
            }

            if (deliveryZone == null)
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure.NullDeliveryZone
                );
            }

            if (!session.ContainsDeliveryZone(
                deliveryZone
            ))
            {
                return CardDeliveryResult.Fail(
                    CardDeliveryFailure
                        .DeliveryZoneDoesNotBelongToSession
                );
            }

            CardDeliveryResult result =
                cardDeliveryService.TryDeliver(
                    card,
                    session.Hand,
                    deliveryZone
                );

            if (result.Succeeded &&
                session.Mission.IsCompleted)
            {
                session.End(
                    GameSessionEndReason.MissionCompleted
                );
            }

            session.ValidateState();

            return result;
        }

        public TurnAdvanceResult TryAdvanceTurn(
    GameSession session
)
        {
            EnsureSessionIsRunning(session);

            MainPhaseEndResult mainResult =
                TryEndMainPhase(session);

            if (!mainResult.Succeeded)
            {
                return TurnAdvanceResult.MainPhaseFailed(
                    mainResult.Failure
                );
            }

            EndPhaseResult endResult =
                ResolveEndPhase(session);

            if (!endResult.Succeeded)
            {
                return TurnAdvanceResult.EndPhaseFailed(
                    endResult.Failure
                );
            }

            return TurnAdvanceResult.Success(
                endResult.CompletedTurnNumber,
                endResult.StartedTurnNumber
            );
        }


        #region helpers
        private static void EnsureSessionIsRunning(
    GameSession session
)
        {
            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session)
                );
            }

            if (session.State != GameSessionState.Running)
            {
                throw new InvalidOperationException(
                    "The game session must be running."
                );
            }
        }

        private static bool HasReachedTurnLimit(
    GameSession session,
    int completedTurnNumber
)
        {
            return session.HasTurnLimit
                && completedTurnNumber >=
                    session.MaximumTurns.Value;
        }

        private static bool IsOwnedDiscardSource(
    GameSession session,
    CardZoneRuntime source
)
        {
            return ReferenceEquals(
                    source,
                    session.Hand
                )
                || ReferenceEquals(
                    source,
                    session.ReactionTable
                );
        }
        #endregion

    }

}