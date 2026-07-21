using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime.Session;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime.Session
{
    public sealed class CardDeliveryZoneConfigTests
    {
        private CardDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition =
                ScriptableObject.CreateInstance<CardDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (definition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    definition
                );
            }
        }

        [Test]
        public void Constructor_StoresConfiguredValues()
        {
            var config =
                new CardDeliveryZoneConfig(
                    definition,
                    requiredAmount: 10
                );

            Assert.That(
                config.AcceptedDefinition,
                Is.SameAs(definition)
            );

            Assert.That(
                config.RequiredAmount,
                Is.EqualTo(10)
            );
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () => new CardDeliveryZoneConfig(
                    acceptedDefinition: null,
                    requiredAmount: 10
                ),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidRequiredAmount_Throws(
            int requiredAmount
        )
        {
            Assert.That(
                () => new CardDeliveryZoneConfig(
                    definition,
                    requiredAmount
                ),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
    }
}