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
    public sealed class ReactionExecutionServiceTests
    {
        private CardDefinition reactantDefinition;
        private CardDefinition productDefinition;

        [SetUp]
        public void SetUp()
        {
            reactantDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            productDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();
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
        public void Execute_WithValidPlan_MovesReactantsToDiscard()
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
                    reactant
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        CreateGuid(100)
                    }
                );

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.ReactionTable.Contains(reactant),
                Is.False
            );

            Assert.That(
                session.DiscardPile.Contains(reactant),
                Is.True
            );

            Assert.That(
                session.DiscardPile.Count,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void Execute_WithValidPlan_CreatesRegistersAndAddsProductsToHand()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0,
                    handCapacity: 3
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    productCount: 2
                );

            Guid firstProductId =
                CreateGuid(100);

            Guid secondProductId =
                CreateGuid(101);

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        firstProductId,
                        secondProductId
                    }
                );

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.CreatedProducts.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(2)
            );

            foreach (
                CardInstance product
                in result.CreatedProducts
            )
            {
                Assert.That(
                    product.Definition,
                    Is.SameAs(productDefinition)
                );

                Assert.That(
                    session.ContainsCard(
                        product.InstanceId
                    ),
                    Is.True
                );

                Assert.That(
                    session.Hand.Contains(product),
                    Is.True
                );

                bool found =
                    session.TryGetCard(
                        product.InstanceId,
                        out CardInstance registeredCard
                    );

                Assert.That(found, Is.True);

                Assert.That(
                    registeredCard,
                    Is.SameAs(product)
                );
            }

            Guid[] createdIds =
                result.CreatedProducts
                    .Select(card => card.InstanceId)
                    .ToArray();

            Assert.That(
                createdIds,
                Is.EqualTo(
                    new[]
                    {
                        firstProductId,
                        secondProductId
                    }
                )
            );
        }

        [Test]
        public void Execute_WithValidPlan_UpdatesHeatAndElectricity()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 3,
                    initialElectricity: 4
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    requiredHeat: 2,
                    producedHeat: 5,
                    requiredElectricity: 3,
                    producedElectricity: 2
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        CreateGuid(100)
                    }
                );

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                session.Heat.Amount,
                Is.EqualTo(6)
            );

            Assert.That(
                session.Electricity.Amount,
                Is.EqualTo(3)
            );
        }

        [Test]
        public void Execute_WithValidationFailure_DoesNotMutateSession()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 1,
                    initialElectricity: 0
                );

            CardInstance reactant =
                session.ReactionTable.Cards[0];

            ReactionResolutionPlan plan =
                CreatePlan(
                    reactant,
                    requiredHeat: 2
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        CreateGuid(100)
                    }
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .InsufficientHeat
                )
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );

            Assert.That(
                session.ReactionTable.Contains(reactant),
                Is.True
            );
        }

        [Test]
        public void Execute_WithDuplicateCreatedProductIds_FailsWithoutMutation()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0,
                    handCapacity: 3
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    productCount: 2
                );

            Guid duplicateId =
                CreateGuid(100);

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        duplicateId,
                        duplicateId
                    }
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .DuplicateCreatedProductInstance
                )
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void Execute_WithExistingProductId_FailsWithoutMutation()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            CardInstance registeredCard =
                session.SessionCards[0];

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0]
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        registeredCard.InstanceId
                    }
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .CreatedProductIdAlreadyExists
                )
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void Execute_WithHeatOverflow_FailsWithoutMutation()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: int.MaxValue,
                    initialElectricity: 0
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    producedHeat: 1
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        CreateGuid(100)
                    }
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .HeatOverflow
                )
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void Execute_WithElectricityOverflow_FailsWithoutMutation()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: int.MaxValue
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    producedElectricity: 1
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        CreateGuid(100)
                    }
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .ElectricityOverflow
                )
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void Execute_WithValidPlan_LeavesSessionStateValid()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 2,
                    initialElectricity: 2,
                    handCapacity: 3
                );

            ReactionResolutionPlan plan =
                CreatePlan(
                    session.ReactionTable.Cards[0],
                    productCount: 2,
                    requiredHeat: 1,
                    producedHeat: 2,
                    requiredElectricity: 1,
                    producedElectricity: 1
                );

            ReactionExecutionService service =
                CreateExecutionService(
                    productIds: new[]
                    {
                        CreateGuid(100),
                        CreateGuid(101)
                    }
                );

            ReactionExecutionResult result =
                service.Execute(
                    session,
                    plan
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );

            int totalCardsInZones =
                session.Deck.Count
                + session.Hand.Count
                + session.ReactionTable.Count
                + session.DiscardPile.Count;

            Assert.That(
                totalCardsInZones,
                Is.EqualTo(
                    session.SessionCards.Count
                )
            );
        }

        [Test]
        public void Constructor_WithNullValidator_Throws()
        {
            CardInstanceFactory factory =
                CreateInstanceFactory(
                    CreateGuid(100)
                );

            Assert.That(
                () => new ReactionExecutionService(
                    null,
                    factory,
                    new CardMovementService()
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithNullFactory_Throws()
        {
            Assert.That(
                () => new ReactionExecutionService(
                    new ReactionExecutionValidator(),
                    null,
                    new CardMovementService()
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithNullMovementService_Throws()
        {
            CardInstanceFactory factory =
                CreateInstanceFactory(
                    CreateGuid(100)
                );

            Assert.That(
                () => new ReactionExecutionService(
                    new ReactionExecutionValidator(),
                    factory,
                    null
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        private ReactionExecutionService CreateExecutionService(
            IEnumerable<Guid> productIds
        )
        {
            CardInstanceFactory factory =
                new CardInstanceFactory(
                    new SequenceIdSource(productIds)
                );

            return new ReactionExecutionService(
                new ReactionExecutionValidator(),
                factory,
                new CardMovementService()
            );
        }

        private static CardInstanceFactory CreateInstanceFactory(
            params Guid[] ids
        )
        {
            return new CardInstanceFactory(
                new SequenceIdSource(ids)
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

            CardMovementResult movementResult =
                new CardMovementService().TryMove(
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
            int handCapacity
        )
        {
            var sessionIdSource =
                new SequenceIdSource(
                    new[]
                    {
                        CreateGuid(1)
                    }
                );

            var instanceFactory =
                new CardInstanceFactory(
                    sessionIdSource
                );

            var deckBuilder =
                new DeckRuntimeBuilder(
                    instanceFactory
                );

            var movementService =
                new CardMovementService();

            var drawService =
                new CardDrawService(
                    movementService
                );

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
            int producedHeat = 0,
            int requiredElectricity = 0,
            int producedElectricity = 0
        )
        {
            return new ReactionResolutionPlan(
                consumedReactants:
                    new[]
                    {
                        reactant
                    },
                products:
                    new[]
                    {
                        new ReactionProductPlanEntry(
                            productDefinition,
                            productCount
                        )
                    },
                requiredHeat,
                producedHeat,
                requiredElectricity,
                producedElectricity
            );
        }

        private static SessionSnapshot CaptureSnapshot(
            GameSession session
        )
        {
            return new SessionSnapshot(
                session.SessionCards.ToArray(),
                session.Hand.Cards.ToArray(),
                session.ReactionTable.Cards.ToArray(),
                session.DiscardPile.Cards.ToArray(),
                session.Heat.Amount,
                session.Electricity.Amount
            );
        }

        private static void AssertSessionUnchanged(
            GameSession session,
            SessionSnapshot snapshot
        )
        {
            Assert.That(
                session.SessionCards,
                Is.EqualTo(snapshot.SessionCards)
            );

            Assert.That(
                session.Hand.Cards,
                Is.EqualTo(snapshot.HandCards)
            );

            Assert.That(
                session.ReactionTable.Cards,
                Is.EqualTo(snapshot.TableCards)
            );

            Assert.That(
                session.DiscardPile.Cards,
                Is.EqualTo(snapshot.DiscardCards)
            );

            Assert.That(
                session.Heat.Amount,
                Is.EqualTo(snapshot.Heat)
            );

            Assert.That(
                session.Electricity.Amount,
                Is.EqualTo(snapshot.Electricity)
            );

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
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

        private sealed class SessionSnapshot
        {
            public SessionSnapshot(
                CardInstance[] sessionCards,
                CardInstance[] handCards,
                CardInstance[] tableCards,
                CardInstance[] discardCards,
                int heat,
                int electricity
            )
            {
                SessionCards = sessionCards;
                HandCards = handCards;
                TableCards = tableCards;
                DiscardCards = discardCards;
                Heat = heat;
                Electricity = electricity;
            }

            public CardInstance[] SessionCards { get; }

            public CardInstance[] HandCards { get; }

            public CardInstance[] TableCards { get; }

            public CardInstance[] DiscardCards { get; }

            public int Heat { get; }

            public int Electricity { get; }
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