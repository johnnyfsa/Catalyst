using System;
using System.Collections.Generic;
using Catalyst.Cards.Runtime;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionFlowResult
    {
        private readonly CardInstance[] createdProducts;

        private ReactionFlowResult(
            bool succeeded,
            ReactionResolutionFailure failure,
            IEnumerable<CardInstance> createdProducts
        )
        {
            Succeeded = succeeded;
            Failure = failure;
            this.createdProducts =
                CopyProducts(createdProducts);
        }

        public bool Succeeded { get; }

        public ReactionResolutionFailure Failure { get; }

        public IReadOnlyList<CardInstance> CreatedProducts =>
            createdProducts;

        public static ReactionFlowResult Success(
            IEnumerable<CardInstance> createdProducts
        )
        {
            if (createdProducts == null)
            {
                throw new ArgumentNullException(
                    nameof(createdProducts)
                );
            }

            CardInstance[] products =
                CopyProducts(createdProducts);

            if (products.Length == 0)
            {
                throw new ArgumentException(
                    "A successful reaction flow must create at least one product.",
                    nameof(createdProducts)
                );
            }

            return new ReactionFlowResult(
                succeeded: true,
                ReactionResolutionFailure.None,
                products
            );
        }

        public static ReactionFlowResult Fail(
            ReactionResolutionFailure failure
        )
        {
            if (failure == ReactionResolutionFailure.None)
            {
                throw new ArgumentException(
                    "A failed reaction flow must contain a failure.",
                    nameof(failure)
                );
            }

            return new ReactionFlowResult(
                succeeded: false,
                failure,
                Array.Empty<CardInstance>()
            );
        }

        private static CardInstance[] CopyProducts(
            IEnumerable<CardInstance> source
        )
        {
            var result =
                new List<CardInstance>();

            foreach (CardInstance product in source)
            {
                if (product == null)
                {
                    throw new ArgumentException(
                        "Created product collection cannot contain null entries.",
                        nameof(source)
                    );
                }

                result.Add(product);
            }

            return result.ToArray();
        }
    }
}