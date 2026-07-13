using System;
using Catalyst.Cards.Definitions;
using Catalyst.Cards.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Catalyst.Tests.EditMode.Cards.Runtime
{
    public sealed class CardInstanceTests
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
        public void Constructor_WithValidArguments_StoresIdAndDefinition()
        {
            Guid expectedId =
                Guid.Parse("7191d1cd-7897-4c14-b627-538f8af25c9c");

            CardInstance instance = new CardInstance(
                expectedId,
                cardDefinition
            );

            Assert.That(
                instance.InstanceId,
                Is.EqualTo(expectedId)
            );

            Assert.That(
                instance.Definition,
                Is.SameAs(cardDefinition)
            );
        }

        [Test]
        public void Constructor_WithEmptyId_Throws()
        {
            Assert.That(
                () => new CardInstance(
                    Guid.Empty,
                    cardDefinition
                ),
                Throws.TypeOf<ArgumentException>()
            );
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Guid validId =
                Guid.Parse("57b77f6d-f265-4183-95b4-42317a776b57");

            Assert.That(
                () => new CardInstance(validId, null),
                Throws.TypeOf<ArgumentNullException>()
            );
        }

        [Test]
        public void InstancesOfSameDefinition_CanHaveDifferentIds()
        {
            Guid firstId =
                Guid.Parse("1cbb598a-bf0c-4770-a9d9-d751db476c08");

            Guid secondId =
                Guid.Parse("7f8f9549-a5b6-4e50-b015-4a625a5df221");

            CardInstance firstInstance = new CardInstance(
                firstId,
                cardDefinition
            );

            CardInstance secondInstance = new CardInstance(
                secondId,
                cardDefinition
            );

            Assert.That(
                firstInstance.Definition,
                Is.SameAs(secondInstance.Definition)
            );

            Assert.That(
                firstInstance.InstanceId,
                Is.Not.EqualTo(secondInstance.InstanceId)
            );
        }

        [Test]
        public void InstanceIdAndDefinition_CannotBeReassigned()
        {
            Guid instanceId =
                Guid.Parse("1edc371d-e00f-4d28-ab99-e97caf58ade0");

            CardInstance instance = new CardInstance(
                instanceId,
                cardDefinition
            );

            Assert.That(
                instance.InstanceId,
                Is.EqualTo(instanceId)
            );

            Assert.That(
                instance.Definition,
                Is.SameAs(cardDefinition)
            );
        }
    }
}