using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Cards.Runtime.Resources;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSession
    {
        private readonly List<CardInstance> sessionCards;

        private readonly ReadOnlyCollection<CardInstance>
            readOnlySessionCards;

        private readonly Dictionary<Guid, CardInstance>
            cardsById;

        internal GameSession(
    IEnumerable<CardInstance> sessionCards,
    DeckRuntime deck,
    HandRuntime hand,
    ReactionTableRuntime reactionTable,
    DiscardPileRuntime discardPile,
    TurnRuntime turn,
    ResourceCounterRuntime heat,
    ResourceCounterRuntime electricity
)
        {
            if (sessionCards == null)
            {
                throw new ArgumentNullException(
                    nameof(sessionCards)
                );
            }

            Deck = deck
                ?? throw new ArgumentNullException(nameof(deck));

            Hand = hand
                ?? throw new ArgumentNullException(nameof(hand));

            ReactionTable = reactionTable
                ?? throw new ArgumentNullException(
                    nameof(reactionTable)
                );

            DiscardPile = discardPile
                ?? throw new ArgumentNullException(
                    nameof(discardPile)
                );

            Turn = turn
                ?? throw new ArgumentNullException(nameof(turn));

            this.sessionCards = new List<CardInstance>();
            cardsById = new Dictionary<Guid, CardInstance>();

            foreach (CardInstance card in sessionCards)
            {
                RegisterCard(card);
            }

            readOnlySessionCards =
                this.sessionCards.AsReadOnly();

            Heat = heat
?? throw new ArgumentNullException(
    nameof(heat)
);

            Electricity = electricity
                ?? throw new ArgumentNullException(
                    nameof(electricity)
                );
        }

        public IReadOnlyList<CardInstance> SessionCards =>
            readOnlySessionCards;

        public DeckRuntime Deck { get; }

        public HandRuntime Hand { get; }

        public ReactionTableRuntime ReactionTable { get; }

        public DiscardPileRuntime DiscardPile { get; }

        public ResourceCounterRuntime Heat { get; }

        public ResourceCounterRuntime Electricity { get; }

        public TurnRuntime Turn { get; }

        public GameSessionState State { get; private set; } =
            GameSessionState.NotStarted;

        public GameSessionEndReason EndReason
        {
            get;
            private set;
        } = GameSessionEndReason.None;

        public bool IsRunning =>
            State == GameSessionState.Running;

        public bool HasEnded =>
            State == GameSessionState.Ended;

        public bool ContainsCard(Guid instanceId)
        {
            return cardsById.ContainsKey(instanceId);
        }

        public bool TryGetCard(
            Guid instanceId,
            out CardInstance card
        )
        {
            return cardsById.TryGetValue(
                instanceId,
                out card
            );
        }

        internal void Start()
        {
            if (State != GameSessionState.NotStarted)
            {
                throw new InvalidOperationException(
                    $"Cannot start a session in state '{State}'."
                );
            }

            Turn.StartFirstTurn();

            State = GameSessionState.Running;
            EndReason = GameSessionEndReason.None;
        }

        internal void End(GameSessionEndReason reason)
        {
            if (State != GameSessionState.Running)
            {
                throw new InvalidOperationException(
                    $"Cannot end a session in state '{State}'."
                );
            }

            if (reason == GameSessionEndReason.None)
            {
                throw new ArgumentException(
                    "An ended session must have an end reason.",
                    nameof(reason)
                );
            }

            State = GameSessionState.Ended;
            EndReason = reason;
        }

        internal void ValidateState()
        {
            Dictionary<Guid, int> zoneOccurrences =
                new Dictionary<Guid, int>();

            foreach (CardInstance card in sessionCards)
            {
                zoneOccurrences.Add(card.InstanceId, 0);
            }

            CountZoneCards(Deck, zoneOccurrences);
            CountZoneCards(Hand, zoneOccurrences);
            CountZoneCards(
                ReactionTable,
                zoneOccurrences
            );
            CountZoneCards(
                DiscardPile,
                zoneOccurrences
            );

            foreach (
                KeyValuePair<Guid, int> occurrence
                in zoneOccurrences
            )
            {
                if (occurrence.Value != 1)
                {
                    throw new InvalidOperationException(
                        $"Card instance '{occurrence.Key}' must belong to exactly one zone, but was found in {occurrence.Value} zones."
                    );
                }
            }
        }

        private void RegisterCard(CardInstance card)
        {
            if (card == null)
            {
                throw new ArgumentException(
                    "Session card collection cannot contain null entries.",
                    nameof(card)
                );
            }

            if (cardsById.ContainsKey(card.InstanceId))
            {
                throw new InvalidOperationException(
                    $"The session already contains a card with instance ID '{card.InstanceId}'."
                );
            }

            cardsById.Add(
                card.InstanceId,
                card
            );

            sessionCards.Add(card);
        }

        internal void RegisterCreatedCard(CardInstance card)
        {
            RegisterCard(card);
        }

        private void CountZoneCards(
            CardZoneRuntime zone,
            IDictionary<Guid, int> zoneOccurrences
        )
        {
            foreach (CardInstance card in zone.Cards)
            {
                if (!cardsById.TryGetValue(
                    card.InstanceId,
                    out CardInstance registeredCard
                ))
                {
                    throw new InvalidOperationException(
                        $"Zone contains unregistered card instance '{card.InstanceId}'."
                    );
                }

                if (!ReferenceEquals(card, registeredCard))
                {
                    throw new InvalidOperationException(
                        $"Zone contains a different object using registered instance ID '{card.InstanceId}'."
                    );
                }

                zoneOccurrences[card.InstanceId]++;
            }
        }
    }
}