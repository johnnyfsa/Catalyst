using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime;
using Catalyst.Reactions.Runtime.Resolution;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionResolutionPlannerTests
    {
        private TestCardFactory cardFactory;
        private TestReactionFactory reactionFactory;

        private ReactionResolutionPlanner planner;

        private CardDefinition hydrogen;
        private CardDefinition oxygen;
        private CardDefinition water;
        private CardDefinition methane;

        [SetUp]
        public void SetUp()
        {
            cardFactory = new TestCardFactory();
            reactionFactory = new TestReactionFactory();

            hydrogen = cardFactory.CreateDefinition();
            oxygen = cardFactory.CreateDefinition();
            water = cardFactory.CreateDefinition();
            methane = cardFactory.CreateDefinition();

            planner =
                new ReactionResolutionPlanner(
                    new ReactionMatcherService()
                );
        }

        [TearDown]
        public void TearDown()
        {
            reactionFactory?.Dispose();
            reactionFactory = null;

            cardFactory?.DisposeCreatedDefinitions();
            cardFactory = null;
        }

        [Test]
        public void Constructor_WithNullMatcher_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlanner(
                    null
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void TryCreatePlan_WithExactComposition_Succeeds()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance firstHydrogen =
                cardFactory.CreateInstance(hydrogen);

            CardInstance secondHydrogen =
                cardFactory.CreateInstance(hydrogen);

            CardInstance oxygenCard =
                cardFactory.CreateInstance(oxygen);

            CardInstance[] tableCards =
            {
                firstHydrogen,
                secondHydrogen,
                oxygenCard
            };

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                result.Plan,
                Is.Not.Null
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure.None
                )
            );

            Assert.That(
                result.Plan.ConsumedReactants.Count,
                Is.EqualTo(3)
            );

            Assert.That(
                result.Plan.ConsumedReactants[0],
                Is.SameAs(firstHydrogen)
            );

            Assert.That(
                result.Plan.ConsumedReactants[1],
                Is.SameAs(secondHydrogen)
            );

            Assert.That(
                result.Plan.ConsumedReactants[2],
                Is.SameAs(oxygenCard)
            );
        }

        [Test]
        public void TryCreatePlan_WithDifferentCardOrder_Succeeds()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] tableCards =
            {
                cardFactory.CreateInstance(oxygen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen)
            };

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                result.Plan.ConsumedReactants,
                Is.EqualTo(tableCards)
            );
        }

        [Test]
        public void TryCreatePlan_CopiesProductDefinitionsAndQuantities()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    CreateValidTableCards()
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                result.Plan.Products.Count,
                Is.EqualTo(1)
            );

            Assert.That(
                result.Plan.Products[0].Definition,
                Is.SameAs(water)
            );

            Assert.That(
                result.Plan.Products[0].Quantity,
                Is.EqualTo(2)
            );

            Assert.That(
                result.Plan.TotalProductCount,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void TryCreatePlan_CopiesHeatValues()
        {
            ReactionDefinition reaction =
                reactionFactory.Create(
                    "heated_reaction",
                    new[]
                    {
                        new ReactionCardAmount(
                            hydrogen,
                            2
                        ),
                        new ReactionCardAmount(
                            oxygen,
                            1
                        )
                    },
                    new[]
                    {
                        new ReactionCardAmount(
                            water,
                            2
                        )
                    },
                    requiredHeat: 2,
                    producedHeat: 1
                );

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    CreateValidTableCards()
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                result.Plan.RequiredHeat,
                Is.EqualTo(2)
            );

            Assert.That(
                result.Plan.ProducedHeat,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void TryCreatePlan_WithNullReaction_FailsExplicitly()
        {
            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    null,
                    CreateValidTableCards()
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .ReactionIsNull
            );
        }

        [Test]
        public void TryCreatePlan_WithInvalidReaction_PreservesValidationFailure()
        {
            ReactionDefinition reaction =
                reactionFactory.Create(
                    string.Empty,
                    new[]
                    {
                        new ReactionCardAmount(
                            hydrogen,
                            2
                        ),
                        new ReactionCardAmount(
                            oxygen,
                            1
                        )
                    },
                    new[]
                    {
                        new ReactionCardAmount(
                            water,
                            2
                        )
                    }
                );

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    CreateValidTableCards()
                );

            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Plan,
                Is.Null
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(
                    ReactionResolutionFailure
                        .ReactionDefinitionIsInvalid
                )
            );

            Assert.That(
                result.DefinitionFailure,
                Is.EqualTo(
                    ReactionDefinitionValidationFailure
                        .ReactionIdIsEmpty
                )
            );
        }

        [Test]
        public void TryCreatePlan_WithNullTableCollection_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    null
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .TableDoesNotMatch
            );
        }

        [Test]
        public void TryCreatePlan_WithMissingReactant_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] tableCards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .TableDoesNotMatch
            );
        }

        [Test]
        public void TryCreatePlan_WithExcessReactant_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] tableCards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .TableDoesNotMatch
            );
        }

        [Test]
        public void TryCreatePlan_WithUnexpectedSubstance_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] tableCards =
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(methane)
            };

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .TableDoesNotMatch
            );
        }

        [Test]
        public void TryCreatePlan_WithNullCardInsideTable_Fails()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            CardInstance[] tableCards =
            {
                cardFactory.CreateInstance(hydrogen),
                null,
                cardFactory.CreateInstance(oxygen)
            };

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            AssertFailure(
                result,
                ReactionResolutionFailure
                    .TableDoesNotMatch
            );
        }

        [Test]
        public void TryCreatePlan_DoesNotModifySourceCollection()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            var tableCards =
                new List<CardInstance>(
                    CreateValidTableCards()
                );

            CardInstance firstCard =
                tableCards[0];

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                tableCards.Count,
                Is.EqualTo(3)
            );

            Assert.That(
                tableCards[0],
                Is.SameAs(firstCard)
            );
        }

        [Test]
        public void TryCreatePlan_CreatesSnapshotOfSourceCollection()
        {
            ReactionDefinition reaction =
                CreateHydrogenCombustionReaction();

            var tableCards =
                new List<CardInstance>(
                    CreateValidTableCards()
                );

            ReactionResolutionPlanResult result =
                planner.TryCreatePlan(
                    reaction,
                    tableCards
                );

            tableCards.Clear();

            Assert.That(
                result.Succeeded,
                Is.True
            );

            Assert.That(
                tableCards,
                Is.Empty
            );

            Assert.That(
                result.Plan.ConsumedReactants.Count,
                Is.EqualTo(3)
            );
        }

        private ReactionDefinition
            CreateHydrogenCombustionReaction()
        {
            return reactionFactory.Create(
                "hydrogen_combustion",
                new[]
                {
                    new ReactionCardAmount(
                        hydrogen,
                        2
                    ),
                    new ReactionCardAmount(
                        oxygen,
                        1
                    )
                },
                new[]
                {
                    new ReactionCardAmount(
                        water,
                        2
                    )
                },
                producedHeat: 1
            );
        }

        private CardInstance[] CreateValidTableCards()
        {
            return new[]
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };
        }

        private static void AssertFailure(
            ReactionResolutionPlanResult result,
            ReactionResolutionFailure expectedFailure
        )
        {
            Assert.That(
                result.Succeeded,
                Is.False
            );

            Assert.That(
                result.Plan,
                Is.Null
            );

            Assert.That(
                result.Failure,
                Is.EqualTo(expectedFailure)
            );
        }
    }
}