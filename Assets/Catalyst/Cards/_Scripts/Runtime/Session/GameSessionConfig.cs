using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Catalyst.Cards.Runtime.Session
{
    public sealed class GameSessionConfig
    {
        private readonly ReadOnlyCollection
            <CardDeliveryZoneConfig> deliveryZones;

        public GameSessionConfig(
            int initialHandSize,
            int maxHandSize,
            int initialHeat = 0,
            int initialElectricity = 0,
            IEnumerable<CardDeliveryZoneConfig>
                deliveryZones = null
        )
        {
            if (initialHandSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialHandSize),
                    initialHandSize,
                    "Initial hand size must be greater than zero."
                );
            }

            if (maxHandSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHandSize),
                    maxHandSize,
                    "Maximum hand size must be greater than zero."
                );
            }

            if (initialHeat < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialHeat),
                    initialHeat,
                    "Initial heat cannot be negative."
                );
            }

            if (initialElectricity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialElectricity),
                    initialElectricity,
                    "Initial electricity cannot be negative."
                );
            }

            InitialHandSize = initialHandSize;
            MaxHandSize = maxHandSize;
            InitialHeat = initialHeat;
            InitialElectricity = initialElectricity;

            this.deliveryZones =
                CopyDeliveryZones(deliveryZones);
        }

        public int InitialHandSize { get; }

        public int MaxHandSize { get; }

        public int InitialHeat { get; }

        public int InitialElectricity { get; }

        public IReadOnlyList<CardDeliveryZoneConfig>
            DeliveryZones => deliveryZones;

        private static ReadOnlyCollection
            <CardDeliveryZoneConfig> CopyDeliveryZones(
                IEnumerable<CardDeliveryZoneConfig> source
            )
        {
            var result =
                new List<CardDeliveryZoneConfig>();

            if (source == null)
            {
                return result.AsReadOnly();
            }

            foreach (
                CardDeliveryZoneConfig zoneConfig
                in source
            )
            {
                if (zoneConfig == null)
                {
                    throw new ArgumentException(
                        "Delivery zone configuration collection cannot contain null entries.",
                        nameof(source)
                    );
                }

                result.Add(zoneConfig);
            }

            return result.AsReadOnly();
        }
    }
}