using System;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Turn
{
    public sealed class DrawPhaseService
    {
        private readonly CardDrawService drawService;

        public DrawPhaseService(
            CardDrawService drawService
        )
        {
            this.drawService = drawService
                ?? throw new ArgumentNullException(
                    nameof(drawService)
                );
        }

        public DrawPhaseResult Resolve(
            TurnRuntime turn,
            DeckRuntime deck,
            HandRuntime hand
        )
        {
            if (turn == null)
            {
                throw new ArgumentNullException(nameof(turn));
            }

            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            if (!turn.HasStarted)
            {
                throw new InvalidOperationException(
                    "The turn runtime must be started before resolving the draw phase."
                );
            }

            if (turn.CurrentPhase != GamePhase.Draw)
            {
                throw new InvalidOperationException(
                    $"The draw phase cannot be resolved while the current phase is '{turn.CurrentPhase}'."
                );
            }

            CardDrawResult drawResult =
                drawService.TryDraw(deck, hand);

            switch (drawResult.Failure)
            {
                case CardDrawFailure.None:
                    turn.AdvancePhase();

                    return DrawPhaseResult.CardDrawn(
                        drawResult.DrawnCard
                    );

                case CardDrawFailure.HandFull:
                    turn.AdvancePhase();

                    return DrawPhaseResult.HandFull();

                case CardDrawFailure.DeckEmpty:
                    return DrawPhaseResult.DeckOut();

                default:
                    throw new InvalidOperationException(
                        $"The draw phase failed unexpectedly with '{drawResult.Failure}'."
                    );
            }
        }
    }
}