using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Cards.Runtime.Creation;
using Catalyst.Cards.Runtime.Draw;
using Catalyst.Cards.Runtime.Movement;
using Catalyst.Cards.Runtime.Randomness;
using Catalyst.Cards.Runtime.Session;
using Catalyst.Reactions.Runtime.Resolution;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionExecutionValidatorTests
    {
        private CardDefinition reactantDefinition;
        private CardDefinition productDefinition;

        private ReactionExecutionValidator validator;

        [SetUp]
        public void SetUp()
        {
            reactantDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            productDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            validator =
                new ReactionExecutionValidator();
        }

        [TearDown]
        public void TearDown()
        {
            if (reactantDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    reactantDefinition
                );
            }

            if (productDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    productDefinition
                );
            }
        }

        [Test]
        public void Validate_WithNullSession_Fails()
        {
            CardInstance reactant =
                CreateExternalCard(
                    CreateGuid(100)
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    reactant
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    null,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.SessionIsNull
            );
        }

        [Test]
        public void Validate_WithNullPlan_Fails()
        {
            GameSession session =
                BuildSession(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    null
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.PlanIsNull
            );
        }

        [Test]
        public void Validate_WithDuplicateReactantInstance_Fails()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            CardInstance reactant =
                session.ReactionTable.Cards[0];

            ReactionResolutionPlan plan =
                CreatePlan(
                    new[]
                    {
                        reactant,
                        reactant
                    }
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .DuplicateReactantInstance
            );
        }

        [Test]
        public void Validate_WithReactantOutsideSession_Fails()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            CardInstance externalReactant =
                CreateExternalCard(
                    CreateGuid(100)
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    externalReactant
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .ReactantDoesNotBelongToSession
            );
        }

        [Test]
        public void Validate_WithDifferentInstanceUsingRegisteredId_Fails()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            CardInstance registeredReactant =
                session.ReactionTable.Cards[0];

            CardInstance differentInstance =
                new CardInstance(
                    registeredReactant.InstanceId,
                    registeredReactant.Definition
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    differentInstance
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .ReactantInstanceDoesNotMatchSessionCard
            );
        }

        [Test]
        public void Validate_WithReactantOutsideReactionTable_Fails()
        {
            GameSession session =
                BuildSession(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            CardInstance handCard =
                session.Hand.Cards[0];

            ReactionResolutionPlan plan =
                CreatePlan(
                    handCard
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .ReactantIsNotOnReactionTable
            );
        }

        [Test]
        public void Validate_WithExactRequiredHeat_IsValid()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 2,
                    initialElectricity: 0
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    requiredHeat: 2
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            Assert.That(result.IsValid, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );
        }

        [Test]
        public void Validate_WithInsufficientHeat_Fails()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 1,
                    initialElectricity: 0
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    requiredHeat: 2
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.InsufficientHeat
            );
        }

        [Test]
        public void Validate_WithExactRequiredElectricity_IsValid()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 2
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    requiredElectricity: 2
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            Assert.That(result.IsValid, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );
        }

        [Test]
        public void Validate_WithInsufficientElectricity_Fails()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 1
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    requiredElectricity: 2
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .InsufficientElectricity
            );
        }

        [Test]
        public void Validate_WithInsufficientProductCapacity_Fails()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0,
                    handCapacity: 1
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    productCount: 2
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .InsufficientProductCapacity
            );
        }

        [Test]
        public void Validate_WithValidSessionAndPlan_Succeeds()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 2,
                    initialElectricity: 3
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    productCount: 1,
                    requiredHeat: 2,
                    requiredElectricity: 3
                );

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            Assert.That(result.IsValid, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );
        }

        [Test]
        public void Validate_DoesNotMutateSession()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 2,
                    initialElectricity: 3
                );

            CardInstance reactant =
                session.ReactionTable.Cards[0];

            ReactionResolutionPlan plan =
                CreatePlan(
                    reactant,
                    productCount: 1,
                    requiredHeat: 2,
                    requiredElectricity: 3
                );

            int initialHeat =
                session.Heat.Amount;

            int initialElectricity =
                session.Electricity.Amount;

            int initialHandCount =
                session.Hand.Count;

            int initialTableCount =
                session.ReactionTable.Count;

            int initialDiscardCount =
                session.DiscardPile.Count;

            int initialSessionCardCount =
                session.SessionCards.Count;

            ReactionExecutionValidationResult result =
                validator.Validate(
                    session,
                    plan
                );

            Assert.That(result.IsValid, Is.True);

            Assert.That(
                session.Heat.Amount,
                Is.EqualTo(initialHeat)
            );

            Assert.That(
                session.Electricity.Amount,
                Is.EqualTo(initialElectricity)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(initialHandCount)
            );

            Assert.That(
                session.ReactionTable.Count,
                Is.EqualTo(initialTableCount)
            );

            Assert.That(
                session.DiscardPile.Count,
                Is.EqualTo(initialDiscardCount)
            );

            Assert.That(
                session.SessionCards.Count,
                Is.EqualTo(initialSessionCardCount)
            );

            Assert.That(
                session.ReactionTable.Cards[0],
                Is.SameAs(reactant)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        private GameSession BuildSessionWithReactantOnTable(
            int initialHeat,
            int initialElectricity,
            int handCapacity = 1
        )
        {
            GameSession session =
                BuildSession(
                    initialHeat,
                    initialElectricity,
                    handCapacity
                );

            CardInstance reactant =
                session.Hand.Cards[0];

            var movementService =
                new CardMovementService();

            CardMovementResult movementResult =
                movementService.TryMove(
                    reactant,
                    session.Hand,
                    session.ReactionTable
                );

            Assert.That(
                movementResult.Succeeded,
                Is.True
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );

            return session;
        }

        private GameSession BuildSession(
            int initialHeat,
            int initialElectricity,
            int handCapacity = 1
        )
        {
            Guid[] ids =
            {
                CreateGuid(1)
            };

            ICardInstanceIdSource idSource =
                new SequenceIdSource(ids);

            var instanceFactory =
                new CardInstanceFactory(idSource);

            var deckBuilder =
                new DeckRuntimeBuilder(instanceFactory);

            var movementService =
                new CardMovementService();

            var drawService =
                new CardDrawService(movementService);

            var sessionBuilder =
                new GameSessionBuilder(
                    deckBuilder,
                    drawService
                );

            var entries =
                new[]
                {
                    new DeckEntry(
                        reactantDefinition,
                        quantity: 1
                    )
                };

            var config =
                new GameSessionConfig(
                    initialHandSize: 1,
                    maxHandSize: handCapacity,
                    initialHeat: initialHeat,
                    initialElectricity:
                        initialElectricity
                );

            return sessionBuilder.Build(
                entries,
                config,
                new SeededRandomSource(12345)
            );
        }

        private ReactionResolutionPlan CreatePlan(
            CardInstance reactant,
            int productCount = 1,
            int requiredHeat = 0,
            int requiredElectricity = 0
        )
        {
            return CreatePlan(
                new[]
                {
                    reactant
                },
                productCount,
                requiredHeat,
                requiredElectricity
            );
        }

        private ReactionResolutionPlan CreatePlan(
            IEnumerable<CardInstance> reactants,
            int productCount = 1,
            int requiredHeat = 0,
            int requiredElectricity = 0
        )
        {
            var products =
                new[]
                {
                    new ReactionProductPlanEntry(
                        productDefinition,
                        productCount
                    )
                };

            return new ReactionResolutionPlan(
                reactants,
                products,
                requiredHeat,
                producedHeat: 0,
                requiredElectricity,
                producedElectricity: 0
            );
        }

        private CardInstance CreateExternalCard(
            Guid id
        )
        {
            return new CardInstance(
                id,
                reactantDefinition
            );
        }

        private static void AssertFailure(
            ReactionExecutionValidationResult result,
            ReactionResolutionFailure expectedFailure
        )
        {
            Assert.That(result.IsValid, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(expectedFailure)
            );
        }

        private static Guid CreateGuid(
            int value
        )
        {
            string suffix =
                value.ToString("D12");

            return Guid.Parse(
                $"00000000-0000-0000-0000-{suffix}"
            );
        }

        private sealed class SequenceIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public SequenceIdSource(
                IEnumerable<Guid> ids
            )
            {
                this.ids =
                    new Queue<Guid>(ids);
            }

            public Guid NextId()
            {
                if (ids.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No more test IDs are available."
                    );
                }

                return ids.Dequeue();
            }
        }
    }
}