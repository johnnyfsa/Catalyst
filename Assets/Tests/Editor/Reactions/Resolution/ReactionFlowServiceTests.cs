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
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using Catalyst.Reactions.Runtime.Resolution;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionFlowServiceTests
    {
        private CardDefinition reactantDefinition;
        private CardDefinition productDefinition;

        private TestReactionFactory reactionFactory;

        [SetUp]
        public void SetUp()
        {
            reactantDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            productDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();

            reactionFactory =
                new TestReactionFactory();
        }

        [TearDown]
        public void TearDown()
        {
            reactionFactory?.Dispose();

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
        public void Constructor_WithNullPlanner_Throws()
        {
            ReactionExecutionService executionService =
                CreateExecutionService(
                    CreateGuid(100)
                );

            Assert.That(
                () => new ReactionFlowService(
                    null,
                    executionService,
                    Array.Empty<ReactionDefinition>()
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithNullExecutionService_Throws()
        {
            ReactionResolutionPlanner planner =
                CreatePlanner();

            Assert.That(
                () => new ReactionFlowService(
                    planner,
                    null,
                    Array.Empty<ReactionDefinition>()
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithNullAvailableReactions_Throws()
        {
            ReactionResolutionPlanner planner =
                CreatePlanner();

            ReactionExecutionService executionService =
                CreateExecutionService(
                    CreateGuid(100)
                );

            Assert.That(
                () => new ReactionFlowService(
                    planner,
                    executionService,
                    null
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_AvailableReactionsContainsNull_Throws()
        {
            ReactionResolutionPlanner planner =
                CreatePlanner();

            ReactionExecutionService executionService =
                CreateExecutionService(
                    CreateGuid(100)
                );

            Assert.That(
                () => new ReactionFlowService(
                    planner,
                    executionService,
                    new ReactionDefinition[]
                    {
                        null
                    }
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void TryResolve_WithNullSession_Fails()
        {
            ReactionDefinition reaction =
                CreateValidReaction();

            ReactionFlowService service =
                CreateFlowService(
                    reaction,
                    CreateGuid(100)
                );

            ReactionFlowResult result =
                service.TryResolve(
                    null,
                    reaction
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.SessionIsNull
            );
        }

        [Test]
        public void TryResolve_WithNullReaction_FailsAsUnavailable()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            ReactionFlowService service =
                CreateFlowService(
                    new SequenceIdSource(
                        new[]
                        {
                            CreateGuid(100)
                        }
                    )
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    null
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.ReactionUnavailable
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void TryResolve_WithTableMismatch_PropagatesPlanningFailure()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            ReactionDefinition reaction =
                reactionFactory.Create(
                    reactionId: "requires-two-reactants",
                    reactants:
                        new[]
                        {
                            new ReactionCardAmount(
                                reactantDefinition,
                                quantity: 2
                            )
                        },
                    products:
                        new[]
                        {
                            new ReactionCardAmount(
                                productDefinition,
                                quantity: 1
                            )
                        }
                );

            ReactionFlowService service =
                CreateFlowService(
                    reaction,
                    CreateGuid(100)
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    reaction
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.TableDoesNotMatch
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void TryResolve_WithExecutionFailure_PropagatesFailure()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 1,
                    initialElectricity: 0
                );

            ReactionDefinition reaction =
                reactionFactory.Create(
                    reactionId: "requires-heat",
                    reactants:
                        new[]
                        {
                            new ReactionCardAmount(
                                reactantDefinition,
                                quantity: 1
                            )
                        },
                    products:
                        new[]
                        {
                            new ReactionCardAmount(
                                productDefinition,
                                quantity: 1
                            )
                        },
                    requiredHeat: 2
                );

            ReactionFlowService service =
                CreateFlowService(
                    reaction,
                    CreateGuid(100)
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    reaction
                );

            AssertFailure(
                result,
                ReactionResolutionFailure.InsufficientHeat
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void TryResolve_WithUnavailableReaction_FailsWithoutMutation()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            ReactionDefinition availableReaction =
                CreateValidReaction();

            ReactionDefinition unavailableReaction =
                reactionFactory.Create(
                    reactionId: "unavailable",
                    reactants:
                        new[]
                        {
                            new ReactionCardAmount(
                                reactantDefinition,
                                quantity: 1
                            )
                        },
                    products:
                        new[]
                        {
                            new ReactionCardAmount(
                                productDefinition,
                                quantity: 1
                            )
                        }
                );

            ReactionFlowService service =
                CreateFlowService(
                    new SequenceIdSource(
                        new[]
                        {
                            CreateGuid(100)
                        }
                    ),
                    availableReaction
                );

            SessionSnapshot snapshot =
                CaptureSnapshot(session);

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    unavailableReaction
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .ReactionUnavailable
            );

            AssertSessionUnchanged(
                session,
                snapshot
            );
        }

        [Test]
        public void TryResolve_EquivalentButDifferentReactionAsset_FailsAsUnavailable()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            ReactionDefinition registeredReaction =
                CreateValidReaction();

            ReactionDefinition equivalentReaction =
                reactionFactory.Create(
                    reactionId:
                        registeredReaction.ReactionId,
                    reactants:
                        new[]
                        {
                            new ReactionCardAmount(
                                reactantDefinition,
                                quantity: 1
                            )
                        },
                    products:
                        new[]
                        {
                            new ReactionCardAmount(
                                productDefinition,
                                quantity: 1
                            )
                        }
                );

            ReactionFlowService service =
                CreateFlowService(
                    new SequenceIdSource(
                        new[]
                        {
                            CreateGuid(100)
                        }
                    ),
                    registeredReaction
                );

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    equivalentReaction
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .ReactionUnavailable
            );
        }

        [Test]
        public void TryResolve_WithValidReaction_ExecutesAndReturnsProducts()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 2,
                    initialElectricity: 3,
                    handCapacity: 3
                );

            ReactionDefinition reaction =
                reactionFactory.Create(
                    reactionId: "valid-reaction",
                    reactants:
                        new[]
                        {
                            new ReactionCardAmount(
                                reactantDefinition,
                                quantity: 1
                            )
                        },
                    products:
                        new[]
                        {
                            new ReactionCardAmount(
                                productDefinition,
                                quantity: 2
                            )
                        },
                    requiredHeat: 1,
                    producedHeat: 3,
                    requiredElectricity: 2,
                    producedElectricity: 1
                );

            Guid firstProductId =
                CreateGuid(100);

            Guid secondProductId =
                CreateGuid(101);

            ReactionFlowService service =
                CreateFlowService(
                    reaction,
                    firstProductId,
                    secondProductId
                );

            CardInstance reactant =
                session.ReactionTable.Cards[0];

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    reaction
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );

            Assert.That(
                result.CreatedProducts.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                session.ReactionTable.IsEmpty,
                Is.True
            );

            Assert.That(
                session.DiscardPile.Contains(reactant),
                Is.True
            );

            Assert.That(
                session.Hand.Count,
                Is.EqualTo(2)
            );

            Assert.That(
                session.Heat.Amount,
                Is.EqualTo(4)
            );

            Assert.That(
                session.Electricity.Amount,
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
                    session.ContainsCard(product.InstanceId),
                    Is.True
                );

                Assert.That(
                    session.Hand.Contains(product),
                    Is.True
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

            Assert.That(
                () => session.ValidateState(),
                Throws.Nothing
            );
        }

        [Test]
        public void TryResolve_WithPlanningFailure_DoesNotCreateProducts()
        {
            GameSession session =
                BuildSessionWithReactantOnTable(
                    initialHeat: 0,
                    initialElectricity: 0
                );

            ReactionDefinition reaction =
                reactionFactory.Create(
                    reactionId: "mismatch",
                    reactants:
                        new[]
                        {
                            new ReactionCardAmount(
                                reactantDefinition,
                                quantity: 2
                            )
                        },
                    products:
                        new[]
                        {
                            new ReactionCardAmount(
                                productDefinition,
                                quantity: 1
                            )
                        }
                );

            var productIdSource =
                new CountingIdSource(
                    CreateGuid(100)
                );

            ReactionFlowService service =
                CreateFlowService(
                    productIdSource,
                    reaction
                );

            ReactionFlowResult result =
                service.TryResolve(
                    session,
                    reaction
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                productIdSource.RequestCount,
                Is.Zero
            );
        }

        private ReactionDefinition CreateValidReaction()
        {
            return reactionFactory.Create(
                reactionId: "valid",
                reactants:
                    new[]
                    {
                        new ReactionCardAmount(
                            reactantDefinition,
                            quantity: 1
                        )
                    },
                products:
                    new[]
                    {
                        new ReactionCardAmount(
                            productDefinition,
                            quantity: 1
                        )
                    }
            );
        }

        private ReactionFlowService CreateFlowService(
            ReactionDefinition availableReaction,
            params Guid[] productIds
        )
        {
            return CreateFlowService(
                new SequenceIdSource(productIds),
                availableReaction
            );
        }

        private ReactionFlowService CreateFlowService(
            ICardInstanceIdSource productIdSource,
            params ReactionDefinition[] availableReactions
        )
        {
            ReactionResolutionPlanner planner =
                CreatePlanner();

            var productFactory =
                new CardInstanceFactory(
                    productIdSource
                );

            var executionService =
                new ReactionExecutionService(
                    new ReactionExecutionValidator(),
                    productFactory,
                    new CardMovementService()
                );

            return new ReactionFlowService(
                planner,
                executionService,
                availableReactions
            );
        }

        private static ReactionResolutionPlanner CreatePlanner()
        {
            return new ReactionResolutionPlanner(
                new ReactionMatcherService()
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
            var instanceFactory =
                new CardInstanceFactory(
                    new SequenceIdSource(
                        new[]
                        {
                            CreateGuid(1)
                        }
                    )
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

        private static void AssertFailure(
            ReactionFlowResult result,
            ReactionResolutionFailure expectedFailure
        )
        {
            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.Failure,
                Is.EqualTo(expectedFailure)
            );

            Assert.That(
                result.CreatedProducts,
                Is.Empty
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

        private sealed class CountingIdSource
            : ICardInstanceIdSource
        {
            private readonly Queue<Guid> ids;

            public CountingIdSource(
                params Guid[] ids
            )
            {
                this.ids =
                    new Queue<Guid>(ids);
            }

            public int RequestCount { get; private set; }

            public Guid NextId()
            {
                RequestCount++;

                if (ids.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No more test IDs are available."
                    );
                }

                return ids.Dequeue();
            }
        }

        private ReactionExecutionService CreateExecutionService(
    params Guid[] productIds
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
    }
}