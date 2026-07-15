using System;
using Catalyst.Cards.Runtime.Turn;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSessionFlowService
    {
        private readonly DrawPhaseService drawPhaseService;

        public GameSessionFlowService(
            DrawPhaseService drawPhaseService
        )
        {
            this.drawPhaseService = drawPhaseService
                ?? throw new ArgumentNullException(
                    nameof(drawPhaseService)
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

        public DrawPhaseResult ResolveDrawPhase(
            GameSession session
        )
        {
            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session)
                );
            }

            if (!session.IsRunning)
            {
                throw new InvalidOperationException(
                    $"Cannot resolve the draw phase while the session state is '{session.State}'."
                );
            }

            DrawPhaseResult result =
                drawPhaseService.Resolve(
                    session.Turn,
                    session.Deck,
                    session.Hand
                );

            if (result.Outcome ==
                DrawPhaseOutcome.DeckOut)
            {
                session.End(
                    GameSessionEndReason.DeckOut
                );
            }

            session.ValidateState();

            return result;
        }
    }
}