using System;
using Catalyst.Cards.Definitions;
using Catalyst.Reactions.Runtime.Resolution;
using Catalyst.Tests.EditMode.Common.Creation;
using NUnit.Framework;

namespace Catalyst.Tests.EditMode.Reactions.Resolution
{
    public sealed class ReactionProductPlanEntryTests
    {
        private TestCardFactory cardFactory;

        [SetUp]
        public void SetUp()
        {
            cardFactory = new TestCardFactory();
        }

        [TearDown]
        public void TearDown()
        {
            cardFactory?.DisposeCreatedDefinitions();
            cardFactory = null;
        }

        [Test]
        public void Constructor_WithValidArguments_PreservesValues()
        {
            CardDefinition definition =
                cardFactory.CreateDefinition();

            var entry =
                new ReactionProductPlanEntry(
                    definition,
                    2
                );

            Assert.That(
                entry.Definition,
                Is.SameAs(definition)
            );

            Assert.That(
                entry.Quantity,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () => new ReactionProductPlanEntry(
                    null,
                    1
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithNonPositiveQuantity_Throws(
            int quantity
        )
        {
            CardDefinition definition =
                cardFactory.CreateDefinition();

            Assert.That(
                () => new ReactionProductPlanEntry(
                    definition,
                    quantity
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
    }
}