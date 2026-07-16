using System;
using System.Collections.Generic;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using Catalyst.Reactions.Definitions;
using Catalyst.Reactions.Runtime.Resolution;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionResolutionPlanTests
    {
        private TestCardFactory cardFactory;
        private TestReactionFactory reactionFactory;

        private CardDefinition hydrogen;
        private CardDefinition oxygen;
        private CardDefinition water;


        [SetUp]
        public void SetUp()
        {
            cardFactory = new TestCardFactory();
            reactionFactory = new TestReactionFactory();

            hydrogen = cardFactory.CreateDefinition();
            oxygen = cardFactory.CreateDefinition();
            water = cardFactory.CreateDefinition();
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
        public void Constructor_WithValidArguments_PreservesValues()
        {
            CardInstance firstHydrogen =
                cardFactory.CreateInstance(hydrogen);

            CardInstance secondHydrogen =
                cardFactory.CreateInstance(hydrogen);

            CardInstance oxygenCard =
                cardFactory.CreateInstance(oxygen);

            var product =
                new ReactionProductPlanEntry(
                    water,
                    2
                );

            var plan =
                new ReactionResolutionPlan(
                    new[]
                    {
                        firstHydrogen,
                        secondHydrogen,
                        oxygenCard
                    },
                    new[]
                    {
                        product
                    },
                    requiredHeat: 0,
                    producedHeat: 1
                );


            Assert.That(
                plan.ConsumedReactants.Count,
                Is.EqualTo(3)
            );

            Assert.That(
                plan.ConsumedReactants[0],
                Is.SameAs(firstHydrogen)
            );

            Assert.That(
                plan.ConsumedReactants[1],
                Is.SameAs(secondHydrogen)
            );

            Assert.That(
                plan.ConsumedReactants[2],
                Is.SameAs(oxygenCard)
            );

            Assert.That(
                plan.Products.Count,
                Is.EqualTo(1)
            );

            Assert.That(
                plan.Products[0],
                Is.SameAs(product)
            );

            Assert.That(
                plan.RequiredHeat,
                Is.EqualTo(0)
            );

            Assert.That(
                plan.ProducedHeat,
                Is.EqualTo(1)
            );

            Assert.That(
                plan.TotalProductCount,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Constructor_WithMultipleProducts_CalculatesTotalProductCount()
        {
            CardDefinition carbonDioxide =
                cardFactory.CreateDefinition();

            var plan =
                new ReactionResolutionPlan(
                    CreateValidReactants(),
                    new[]
                    {
                        new ReactionProductPlanEntry(
                            water,
                            2
                        ),
                        new ReactionProductPlanEntry(
                            carbonDioxide,
                            1
                        )
                    },
                    requiredHeat: 0,
                    producedHeat: 1
                );

            Assert.That(
                plan.TotalProductCount,
                Is.EqualTo(3)
            );
        }

        [Test]
        public void Constructor_CopiesReactantCollection()
        {
            var source =
                new List<CardInstance>(
                    CreateValidReactants()
                );

            var plan =
                new ReactionResolutionPlan(
                    source,
                    CreateValidProducts(),
                    requiredHeat: 0,
                    producedHeat: 1
                );

            source.Clear();

            Assert.That(
                source,
                Is.Empty
            );

            Assert.That(
                plan.ConsumedReactants.Count,
                Is.EqualTo(3)
            );
        }

        [Test]
        public void Constructor_CopiesProductCollection()
        {
            var source =
                new List<ReactionProductPlanEntry>(
                    CreateValidProducts()
                );

            var plan =
                new ReactionResolutionPlan(
                    CreateValidReactants(),
                    source,
                    requiredHeat: 0,
                    producedHeat: 1
                );

            source.Clear();

            Assert.That(
                source,
                Is.Empty
            );

            Assert.That(
                plan.Products.Count,
                Is.EqualTo(1)
            );
        }

        [Test]
        public void Constructor_PreservesSpecificReactantInstances()
        {
            CardInstance firstHydrogen =
                cardFactory.CreateInstance(hydrogen);

            CardInstance secondHydrogen =
                cardFactory.CreateInstance(hydrogen);

            var plan =
                new ReactionResolutionPlan(
                    new[]
                    {
                        firstHydrogen,
                        secondHydrogen,
                        cardFactory.CreateInstance(oxygen)
                    },
                    CreateValidProducts(),
                    requiredHeat: 0,
                    producedHeat: 1
                );

            Assert.That(
                firstHydrogen.InstanceId,
                Is.Not.EqualTo(
                    secondHydrogen.InstanceId
                )
            );

            Assert.That(
                plan.ConsumedReactants[0],
                Is.SameAs(firstHydrogen)
            );

            Assert.That(
                plan.ConsumedReactants[1],
                Is.SameAs(secondHydrogen)
            );
        }


        [Test]
        public void Constructor_WithNullReactantCollection_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    null,
                    CreateValidProducts(),
                    requiredHeat: 0,
                    producedHeat: 1
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithEmptyReactantCollection_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    Array.Empty<CardInstance>(),
                    CreateValidProducts(),
                    requiredHeat: 0,
                    producedHeat: 1
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Constructor_WithNullReactantEntry_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    new CardInstance[]
                    {
                        cardFactory.CreateInstance(
                            hydrogen
                        ),
                        null
                    },
                    CreateValidProducts(),
                    requiredHeat: 0,
                    producedHeat: 1
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Constructor_WithNullProductCollection_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    CreateValidReactants(),
                    null,
                    requiredHeat: 0,
                    producedHeat: 1
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void Constructor_WithEmptyProductCollection_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    CreateValidReactants(),
                    Array.Empty<
                        ReactionProductPlanEntry
                    >(),
                    requiredHeat: 0,
                    producedHeat: 1
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Constructor_WithNullProductEntry_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    CreateValidReactants(),
                    new ReactionProductPlanEntry[]
                    {
                        null
                    },
                    requiredHeat: 0,
                    producedHeat: 1
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Constructor_WithNegativeRequiredHeat_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    CreateValidReactants(),
                    CreateValidProducts(),
                    requiredHeat: -1,
                    producedHeat: 0
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        [Test]
        public void Constructor_WithNegativeProducedHeat_Throws()
        {
            Assert.That(
                () => new ReactionResolutionPlan(
                    CreateValidReactants(),
                    CreateValidProducts(),
                    requiredHeat: 0,
                    producedHeat: -1
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }

        private CardInstance[] CreateValidReactants()
        {
            return new[]
            {
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(hydrogen),
                cardFactory.CreateInstance(oxygen)
            };
        }

        private ReactionProductPlanEntry[]
            CreateValidProducts()
        {
            return new[]
            {
                new ReactionProductPlanEntry(
                    water,
                    2
                )
            };
        }
    }
}