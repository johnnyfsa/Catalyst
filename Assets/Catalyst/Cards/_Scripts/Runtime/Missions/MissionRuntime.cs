using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catalyst.Cards.Runtime.Zones;

namespace Catalyst.Cards.Runtime.Missions
{
    public sealed class MissionRuntime
    {
        private readonly ReadOnlyCollection
            <CardDeliveryZoneRuntime> deliveryObjectives;

        public MissionRuntime(
            IEnumerable<CardDeliveryZoneRuntime>
                deliveryObjectives
        )
        {
            this.deliveryObjectives =
                CopyDeliveryObjectives(
                    deliveryObjectives
                );
        }

        public IReadOnlyList<CardDeliveryZoneRuntime>
            DeliveryObjectives => deliveryObjectives;

        public bool HasObjectives =>
            deliveryObjectives.Count > 0;

        public bool IsCompleted
        {
            get
            {
                if (!HasObjectives)
                {
                    return false;
                }

                foreach (
                    CardDeliveryZoneRuntime objective
                    in deliveryObjectives
                )
                {
                    if (!objective.IsCompleted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static ReadOnlyCollection
            <CardDeliveryZoneRuntime>
            CopyDeliveryObjectives(
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
                CardDeliveryZoneRuntime objective
                in source
            )
            {
                if (objective == null)
                {
                    throw new ArgumentException(
                        "Mission delivery objective collection cannot contain null entries.",
                        nameof(source)
                    );
                }

                result.Add(objective);
            }

            return result.AsReadOnly();
        }
    }
}
