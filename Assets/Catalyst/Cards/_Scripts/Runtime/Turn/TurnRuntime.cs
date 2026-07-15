using System;

namespace Catalyst.Cards.Runtime.Turn
{
    public sealed class TurnRuntime
    {
        public int TurnNumber { get; private set; }

        public GamePhase CurrentPhase { get; private set; } =
            GamePhase.NotStarted;

        public bool HasStarted =>
            CurrentPhase != GamePhase.NotStarted;

        internal void StartFirstTurn()
        {
            if (HasStarted)
            {
                throw new InvalidOperationException(
                    "The turn runtime has already been started."
                );
            }

            TurnNumber = 1;
            CurrentPhase = GamePhase.Draw;
        }

        internal void AdvancePhase()
        {
            if (!HasStarted)
            {
                throw new InvalidOperationException(
                    "The turn runtime must be started before advancing phases."
                );
            }

            switch (CurrentPhase)
            {
                case GamePhase.Draw:
                    CurrentPhase = GamePhase.Main;
                    break;

                case GamePhase.Main:
                    CurrentPhase = GamePhase.End;
                    break;

                case GamePhase.End:
                    TurnNumber++;
                    CurrentPhase = GamePhase.Draw;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported game phase '{CurrentPhase}'."
                    );
            }
        }
    }
}