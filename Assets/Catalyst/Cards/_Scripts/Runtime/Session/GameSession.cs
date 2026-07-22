using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catalyst.Cards.Runtime.Resources;
using Catalyst.Cards.Runtime.Turn;
using Catalyst.Cards.Runtime.Zones;
using Catalyst.Cards.Runtime.Missions;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSession
    {
        private readonly List<CardInstance> sessionCards;

        private readonly ReadOnlyCollection<CardInstance>
            readOnlySessionCards;

        private readonly Dictionary<Guid, CardInstance>
            cardsById;

        private readonly ReadOnlyCollection
            <CardDeliveryZoneRuntime> deliveryZones;
        public MissionRuntime Mission { get; }

        internal GameSession(
            IEnumerable<CardInstance> sessionCards,
            DeckRuntime deck,
            HandRuntime hand,
            ReactionTableRuntime reactionTable,
            DiscardPileRuntime discardPile,
            IEnumerable<CardDeliveryZoneRuntime> deliveryZones,
            MissionRuntime mission,
            TurnRuntime turn,
            ResourceCounterRuntime heat,
            ResourceCounterRuntime electricity,
            int? maximumTurns
        )
        {
            if (sessionCards == null)
            {
                throw new ArgumentNullException(
                    nameof(sessionCards)
                );
            }

            Deck = deck
                ?? throw new ArgumentNullException(
                    nameof(deck)
                );

            Hand = hand
                ?? throw new ArgumentNullException(
                    nameof(hand)
                );

            ReactionTable = reactionTable
                ?? throw new ArgumentNullException(
                    nameof(reactionTable)
                );

            DiscardPile = discardPile
                ?? throw new ArgumentNullException(
                    nameof(discardPile)
                );

            Turn = turn
                ?? throw new ArgumentNullException(
                    nameof(turn)
                );

            Heat = heat
                ?? throw new ArgumentNullException(
                    nameof(heat)
                );

            Electricity = electricity
                ?? throw new ArgumentNullException(
                    nameof(electricity)
                );
            Mission = mission
                ?? throw new ArgumentNullException(
                    nameof(mission)
                );
            if (maximumTurns.HasValue && maximumTurns.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTurns),
                    maximumTurns,
                    "Maximum turns must be greater than zero when a turn limit is enabled."
                );
            }

            this.deliveryZones =
                CopyDeliveryZones(deliveryZones);

            this.sessionCards =
                new List<CardInstance>();

            cardsById =
                new Dictionary<Guid, CardInstance>();

            foreach (CardInstance card in sessionCards)
            {
                RegisterCard(card);
            }

            readOnlySessionCards =
                this.sessionCards.AsReadOnly();

            MaximumTurns = maximumTurns;
        }

        public IReadOnlyList<CardInstance> SessionCards =>
            readOnlySessionCards;

        public DeckRuntime Deck { get; }

        public HandRuntime Hand { get; }

        public ReactionTableRuntime ReactionTable { get; }

        public DiscardPileRuntime DiscardPile { get; }

        public IReadOnlyList<CardDeliveryZoneRuntime>
            DeliveryZones => deliveryZones;

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

        public int? MaximumTurns { get; }

        public bool HasTurnLimit =>
            MaximumTurns.HasValue;

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
            var zoneOccurrences =
                new Dictionary<Guid, int>();

            foreach (CardInstance card in sessionCards)
            {
                zoneOccurrences.Add(
                    card.InstanceId,
                    0
                );
            }

            CountZoneCards(
                Deck,
                zoneOccurrences
            );

            CountZoneCards(
                Hand,
                zoneOccurrences
            );

            CountZoneCards(
                ReactionTable,
                zoneOccurrences
            );

            CountZoneCards(
                DiscardPile,
                zoneOccurrences
            );

            foreach (
                CardDeliveryZoneRuntime deliveryZone
                in deliveryZones
            )
            {
                CountZoneCards(
                    deliveryZone,
                    zoneOccurrences
                );
            }

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

        internal void RegisterCreatedCard(
            CardInstance card
        )
        {
            RegisterCard(card);
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

        private static ReadOnlyCollection
            <CardDeliveryZoneRuntime> CopyDeliveryZones(
                IEnumerable<CardDeliveryZoneRuntime> source
            )
        {
            if (source == null)
            {
                throw new ArgumentNullException(
                    nameof(source)
                );
            }

            var result =
                new List<CardDeliveryZoneRuntime>();

            foreach (
                CardDeliveryZoneRuntime deliveryZone
                in source
            )
            {
                if (deliveryZone == null)
                {
                    throw new ArgumentException(
                        "Delivery zone collection cannot contain null entries.",
                        nameof(source)
                    );
                }

                result.Add(deliveryZone);
            }

            return result.AsReadOnly();
        }

        public bool ContainsDeliveryZone(
    CardDeliveryZoneRuntime deliveryZone
)
        {
            if (deliveryZone == null)
            {
                return false;
            }

            foreach (
                CardDeliveryZoneRuntime sessionZone
                in DeliveryZones
            )
            {
                if (ReferenceEquals(
                    sessionZone,
                    deliveryZone
                ))
                {
                    return true;
                }
            }

            return false;
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

                if (!ReferenceEquals(
                    card,
                    registeredCard
                ))
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