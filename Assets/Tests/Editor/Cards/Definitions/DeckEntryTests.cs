using System;
using NUnit.Framework;
using Catalyst.Cards.Definitions;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Definitions
{
    public sealed class DeckEntryTests
    {
        private CardDefinition cardDefinition;

        [SetUp]
        public void SetUp()
        {
            cardDefinition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (cardDefinition != null)
            {
                UnityEngine.Object.DestroyImmediate(cardDefinition);
            }
        }

        [Test]
        public void Constructor_WithValidArguments_StoresDefinitionAndQuantity()
        {
            const int expectedQuantity = 3;

            DeckEntry entry = new DeckEntry(
                cardDefinition,
                expectedQuantity
            );

            Assert.That(
                entry.CardDefinition,
                Is.SameAs(cardDefinition)
            );

            Assert.That(
                entry.Quantity,
                Is.EqualTo(expectedQuantity)
            );
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () => new DeckEntry(null, 1),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-10)]
        public void Constructor_WithNonPositiveQuantity_Throws(
            int invalidQuantity
        )
        {
            Assert.That(
                () => new DeckEntry(
                    cardDefinition,
                    invalidQuantity
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
    }
}