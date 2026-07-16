using System;
using System.Collections.Generic;
using Catalyst.Cards.Runtime;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionResolutionPlan
    {
        private readonly CardInstance[] consumedReactants;
        private readonly ReactionProductPlanEntry[] products;

        public ReactionResolutionPlan(
            IEnumerable<CardInstance> consumedReactants,
            IEnumerable<ReactionProductPlanEntry> products,
            int requiredHeat,
            int producedHeat
        )
        {
            if (consumedReactants == null)
            {
                throw new ArgumentNullException(
                    nameof(consumedReactants)
                );
            }

            if (products == null)
            {
                throw new ArgumentNullException(
                    nameof(products)
                );
            }

            if (requiredHeat < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredHeat)
                );
            }

            if (producedHeat < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(producedHeat)
                );
            }

            this.consumedReactants =
                CopyReactants(consumedReactants);

            this.products =
                CopyProducts(products);

            if (this.consumedReactants.Length == 0)
            {
                throw new ArgumentException(
                    "A reaction plan must consume at least one reactant.",
                    nameof(consumedReactants)
                );
            }

            if (this.products.Length == 0)
            {
                throw new ArgumentException(
                    "A reaction plan must produce at least one product.",
                    nameof(products)
                );
            }

            RequiredHeat = requiredHeat;
            ProducedHeat = producedHeat;
        }

        public IReadOnlyList<CardInstance> ConsumedReactants =>
            consumedReactants;

        public IReadOnlyList<ReactionProductPlanEntry> Products =>
            products;

        public int RequiredHeat { get; }

        public int ProducedHeat { get; }

        public int TotalProductCount
        {
            get
            {
                int total = 0;

                foreach (
                    ReactionProductPlanEntry product
                    in products
                )
                {
                    total += product.Quantity;
                }

                return total;
            }
        }

        private static CardInstance[] CopyReactants(
            IEnumerable<CardInstance> source
        )
        {
            var result = new List<CardInstance>();

            foreach (CardInstance card in source)
            {
                if (card == null)
                {
                    throw new ArgumentException(
                        "A reaction plan cannot contain a null reactant.",
                        nameof(source)
                    );
                }

                result.Add(card);
            }

            return result.ToArray();
        }

        private static ReactionProductPlanEntry[] CopyProducts(
            IEnumerable<ReactionProductPlanEntry> source
        )
        {
            var result =
                new List<ReactionProductPlanEntry>();

            foreach (
                ReactionProductPlanEntry product
                in source
            )
            {
                if (product == null)
                {
                    throw new ArgumentException(
                        "A reaction plan cannot contain a null product.",
                        nameof(source)
                    );
                }

                result.Add(product);
            }

            return result.ToArray();
        }
    }
}