using System;
using System.Collections.Generic;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Session;

namespace Catalyst.Reactions.Runtime.Resolution
{
    public sealed class ReactionExecutionService
    {
        private readonly ReactionExecutionValidator validator;
        private readonly CardInstanceFactory instanceFactory;
        private readonly CardMovementService movementService;

        public ReactionExecutionService(
            ReactionExecutionValidator validator,
            CardInstanceFactory instanceFactory,
            CardMovementService movementService
        )
        {
            this.validator = validator
                ?? throw new ArgumentNullException(
                    nameof(validator)
                );

            this.instanceFactory = instanceFactory
                ?? throw new ArgumentNullException(
                    nameof(instanceFactory)
                );

            this.movementService = movementService
                ?? throw new ArgumentNullException(
                    nameof(movementService)
                );
        }

        public ReactionExecutionResult Execute(
            GameSession session,
            ReactionResolutionPlan plan
        )
        {
            ReactionExecutionValidationResult validation =
                validator.Validate(
                    session,
                    plan
                );

            if (!validation.IsValid)
            {
                return ReactionExecutionResult.Fail(
                    validation.Failure
                );
            }

            CardInstance[] createdProducts =
                CreateProducts(plan);

            ReactionResolutionFailure preparationFailure =
                ValidatePreparation(
                    session,
                    plan,
                    createdProducts
                );

            if (preparationFailure
                != ReactionResolutionFailure.None)
            {
                return ReactionExecutionResult.Fail(
                    preparationFailure
                );
            }

            Commit(
                session,
                plan,
                createdProducts
            );

            session.ValidateState();

            return ReactionExecutionResult.Success(
                createdProducts
            );
        }

        private CardInstance[] CreateProducts(
            ReactionResolutionPlan plan
        )
        {
            var products =
                new List<CardInstance>(
                    plan.TotalProductCount
                );

            foreach (
                ReactionProductPlanEntry productEntry
                in plan.Products
            )
            {
                for (
                    int index = 0;
                    index < productEntry.Quantity;
                    index++
                )
                {
                    products.Add(
                        instanceFactory.Create(
                            productEntry.Definition
                        )
                    );
                }
            }

            return products.ToArray();
        }

        private static ReactionResolutionFailure
            ValidatePreparation(
                GameSession session,
                ReactionResolutionPlan plan,
                IReadOnlyList<CardInstance> createdProducts
            )
        {
            ReactionResolutionFailure productFailure =
                ValidateCreatedProducts(
                    session,
                    createdProducts
                );

            if (productFailure
                != ReactionResolutionFailure.None)
            {
                return productFailure;
            }

            if (WouldOverflow(
                    session.Heat.Amount,
                    plan.ProducedHeat
                ))
            {
                return ReactionResolutionFailure
                    .HeatOverflow;
            }

            if (WouldOverflow(
                    session.Electricity.Amount,
                    plan.ProducedElectricity
                ))
            {
                return ReactionResolutionFailure
                    .ElectricityOverflow;
            }

            return ReactionResolutionFailure.None;
        }

        private static ReactionResolutionFailure
            ValidateCreatedProducts(
                GameSession session,
                IReadOnlyList<CardInstance> createdProducts
            )
        {
            var createdIds =
                new HashSet<Guid>();

            foreach (
                CardInstance product
                in createdProducts
            )
            {
                if (!createdIds.Add(product.InstanceId))
                {
                    return ReactionResolutionFailure
                        .DuplicateCreatedProductInstance;
                }

                if (session.ContainsCard(
                        product.InstanceId
                    ))
                {
                    return ReactionResolutionFailure
                        .CreatedProductIdAlreadyExists;
                }
            }

            return ReactionResolutionFailure.None;
        }

        private static bool WouldOverflow(
            int currentAmount,
            int producedAmount
        )
        {
            return producedAmount
                > int.MaxValue - currentAmount;
        }

        private void Commit(
            GameSession session,
            ReactionResolutionPlan plan,
            IReadOnlyList<CardInstance> createdProducts
        )
        {
            MoveReactantsToDiscard(
                session,
                plan
            );

            ConsumeResources(
                session,
                plan
            );

            RegisterAndAddProducts(
                session,
                createdProducts
            );

            ProduceResources(
                session,
                plan
            );
        }

        private void MoveReactantsToDiscard(
            GameSession session,
            ReactionResolutionPlan plan
        )
        {
            foreach (
                CardInstance reactant
                in plan.ConsumedReactants
            )
            {
                CardMovementResult movementResult =
                    movementService.TryMove(
                        reactant,
                        session.ReactionTable,
                        session.DiscardPile
                    );

                if (!movementResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"A validated reaction reactant could not be moved to the discard pile. Failure: '{movementResult.Failure}'."
                    );
                }
            }
        }

        private static void ConsumeResources(
            GameSession session,
            ReactionResolutionPlan plan
        )
        {
            bool heatConsumed =
                session.Heat.TryConsume(
                    plan.RequiredHeat
                );

            if (!heatConsumed)
            {
                throw new InvalidOperationException(
                    "Validated reaction heat could not be consumed."
                );
            }

            bool electricityConsumed =
                session.Electricity.TryConsume(
                    plan.RequiredElectricity
                );

            if (!electricityConsumed)
            {
                throw new InvalidOperationException(
                    "Validated reaction electricity could not be consumed."
                );
            }
        }

        private static void RegisterAndAddProducts(
            GameSession session,
            IReadOnlyList<CardInstance> createdProducts
        )
        {
            foreach (
                CardInstance product
                in createdProducts
            )
            {
                session.RegisterCreatedCard(
                    product
                );

                bool added =
                    session.Hand.TryAdd(product);

                if (!added)
                {
                    throw new InvalidOperationException(
                        "A validated reaction product could not be added to the hand."
                    );
                }
            }
        }

        private static void ProduceResources(
            GameSession session,
            ReactionResolutionPlan plan
        )
        {
            session.Heat.Add(
                plan.ProducedHeat
            );

            session.Electricity.Add(
                plan.ProducedElectricity
            );
        }
    }
}